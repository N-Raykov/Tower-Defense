// Basic tower implementation that periodically damages a single
// enemy within range. Uses IEnemy as the integration point and
// respects GameManager game state.

using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Managers;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(Collider2D))]
    public class SingleTargetTower : MonoBehaviour, ITower
    {
        [Header("Attack Settings")]
        [SerializeField] private float range = 2.5f;
        [SerializeField] private int damagePerShot = 2;
        [SerializeField] private float shotsPerSecond = 1f;

        [Header("Targeting")]
        [Tooltip("Layer mask used to find enemy colliders.")]
        [SerializeField] private LayerMask enemyLayerMask;

        [Tooltip("Rotation offset in degrees so the sprite visually faces the shot direction.")]
        [SerializeField] private float rotationOffset = 0f;

        [Header("Projectile")]
        [Tooltip("Projectile prefab to spawn when shooting.")]
        [SerializeField] private GameObject projectilePrefab;

        [Tooltip("Where the projectile should spawn from. If null, uses the tower's position.")]
        [SerializeField] private Transform firePoint;

        [Header("Debug")]
        [SerializeField] private bool drawRangeGizmo = true;

        private float shotCooldown;
        private bool isActive = true;
        private bool gameAllowsAttacking = true; // depends on GameState

        private IEnemy currentTargetEnemy;
        private Transform currentTargetTransform;

        // ITower implementation
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
            {
                return;
            }

            shotCooldown -= Time.deltaTime;
            if (shotCooldown > 0f)
            {
                return;
            }

            if (currentTargetEnemy != null && currentTargetEnemy.IsAlive)
            {
                ShootProjectileAtCurrentTarget();
                shotCooldown = 1f / Mathf.Max(shotsPerSecond, 0.01f);
            }
        }

        private void AcquireTarget()
        {
            currentTargetEnemy = null;
            currentTargetTransform = null;

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, range, enemyLayerMask);

            foreach (Collider2D hit in hits)
            {
                IEnemy enemy = hit.GetComponent<IEnemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    currentTargetEnemy = enemy;
                    currentTargetTransform = hit.transform;
                    return;
                }
            }
        }

        private void ShootProjectileAtCurrentTarget()
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("SingleTargetTower: No projectilePrefab assigned.");
                return;
            }

            if (currentTargetEnemy == null || currentTargetTransform == null)
            {
                return;
            }

            Vector3 spawnPos = firePoint != null ? firePoint.position : transform.position;

            GameObject projGO = Instantiate(projectilePrefab, spawnPos, Quaternion.identity);
            Projectile projectile = projGO.GetComponent<Projectile>();

            if (projectile == null)
            {
                Debug.LogError("SingleTargetTower: Projectile prefab has no Projectile component.");
                Destroy(projGO);
                return;
            }

            projectile.Initialize(currentTargetEnemy, damagePerShot);
        }

        private void OnDrawGizmosSelected()
        {
            if (!drawRangeGizmo) return;

            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(transform.position, range);
        }
    }
}
