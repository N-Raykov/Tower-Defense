// Tower that fires projectiles which set enemies on fire.

using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Managers;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(Collider2D))]
    public class DebuffTower : MonoBehaviour, ITower
    {
        [Header("Attack Settings")]
        [SerializeField] private float range = 2.5f;
        [SerializeField] private int impactDamage = 1;
        [SerializeField] private float shotsPerSecond = 0.75f;

        [Header("Burn Overrides")]
        [SerializeField] private int burnDamagePerTick = 1;
        [SerializeField] private float burnDuration = 3f;
        [SerializeField] private float burnTickInterval = 0.5f;


        [Header("Projectile Overrides")]
        [SerializeField] private float projectileSpeed = 7f;
        [SerializeField] private float projectileHitRadius = 0.1f;
        [SerializeField] private float projectileHomingStrength = 0.2f; 
        [SerializeField] private float projectileMaxLifetime = 3f;

        [Header("Targeting")]
        [SerializeField] private LayerMask enemyLayerMask;
        [SerializeField] private float rotationOffset = 0f;

        [Header("Projectile")]
        [SerializeField] private GameObject projectilePrefab;
        [SerializeField] private Transform firePoint;

        [Header("Debug")]
        [SerializeField] private bool drawRangeGizmo = true;

        private float shotCooldown;
        private bool isActive = true;
        private bool gameAllowsAttacking = true;

        private IEnemy currentTargetEnemy;
        private Transform currentTargetTransform;

        public float Range => range;
        public bool IsActive => isActive;

        private void Awake()
        {
            shotCooldown = 0f;

            if (firePoint == null)
            {
                firePoint = transform;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged += HandleGameStateChanged;
                HandleGameStateChanged(GameManager.Instance.CurrentState);
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGameStateChanged -= HandleGameStateChanged;
            }
        }

        public void SetActive(bool active)
        {
            isActive = active;
        }

        private void HandleGameStateChanged(GameState newState)
        {
            gameAllowsAttacking = newState == GameState.WaveInProgress;
        }

        private void Update()
        {
            AcquireTarget();

            if (currentTargetTransform != null)
            {
                Vector3 dir = currentTargetTransform.position - transform.position;
                if (dir.sqrMagnitude > 0.0001f)
                {
                    float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
                }
            }

            if (!isActive || !gameAllowsAttacking)
                return;

            shotCooldown -= Time.deltaTime;
            if (shotCooldown > 0f)
                return;

            if (currentTargetEnemy != null && currentTargetEnemy.IsAlive)
            {
                ShootProjectile();
                shotCooldown = 1f / Mathf.Max(shotsPerSecond, 0.01f);
            }
        }

        private void AcquireTarget()
        {
            currentTargetEnemy = null;
            currentTargetTransform = null;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayerMask);

            float bestProgress = -1f;
            IEnemy bestEnemy = null;
            Transform bestTransform = null;

            foreach (Collider2D hit in hits)
            {
                IEnemy enemy = hit.GetComponent<IEnemy>();
                if (enemy == null || !enemy.IsAlive)
                    continue;

                float progress = 0f;
                if (enemy is IPathEnemy pathEnemy)
                {
                    progress = pathEnemy.PathProgress;
                }

                if (progress > bestProgress)
                {
                    bestProgress = progress;
                    bestEnemy = enemy;
                    bestTransform = hit.transform;
                }
            }

            currentTargetEnemy = bestEnemy;
            currentTargetTransform = bestTransform;
        }


        private void ShootProjectile()
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("DebuffTower: projectilePrefab not assigned.");
                return;
            }

            if (currentTargetEnemy == null || currentTargetTransform == null)
                return;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

            GameObject projGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            DebuffProjectile proj = projGO.GetComponent<DebuffProjectile>();

            if (proj == null)
            {
                Debug.LogError("DebuffTower: projectile prefab is missing DebuffProjectile component.");
                Destroy(projGO);
                return;
            }

            proj.Initialize(
                currentTargetEnemy,
                impactDamage,
                projectileSpeed,
                projectileHitRadius,
                projectileHomingStrength,
                projectileMaxLifetime,
                burnDamagePerTick,
                burnDuration,
                burnTickInterval
            );

        }


        private void OnDrawGizmosSelected()
        {
            if (!drawRangeGizmo) return;

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}

