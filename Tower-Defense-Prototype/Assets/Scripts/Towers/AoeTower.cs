// Tower that fires projectiles which explode in an area, damaging
// multiple enemies at once.

using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Managers;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(Collider2D))]
    public class AoeTower : MonoBehaviour, ITower
    {
        [Header("Attack Settings")]
        [SerializeField] private float range = 3f;
        [SerializeField] private float shotsPerSecond = 0.5f;

        [Header("Projectile Overrides")]
        [SerializeField] private float projectileSpeed = 5f;
        [SerializeField] private float projectileHitRadius = 0.15f;
        [SerializeField] private float projectileHomingStrength = 0.8f; // nice rocket homing
        [SerializeField] private float projectileMaxLifetime = 4f;
        [SerializeField] private float projectileExplosionRadius = 1.5f;
        [SerializeField] private int projectileExplosionDamage = 4;


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
                Debug.LogWarning("AoeTower: projectilePrefab not assigned.");
                return;
            }

            if (currentTargetEnemy == null || currentTargetTransform == null)
                return;

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

            GameObject projGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            AoeProjectile proj = projGO.GetComponent<AoeProjectile>();

            if (proj == null)
            {
                Debug.LogError("AoeTower: projectile prefab is missing AoeProjectile component.");
                Destroy(projGO);
                return;
            }

            proj.Initialize(
                currentTargetEnemy,
                projectileSpeed,
                projectileHitRadius,
                projectileHomingStrength,
                projectileMaxLifetime,
                projectileExplosionRadius,
                projectileExplosionDamage
            );
        }


        private void OnDrawGizmosSelected()
        {
            if (!drawRangeGizmo) return;

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
