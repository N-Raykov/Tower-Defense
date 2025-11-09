// Projectile that explodes in an area, damaging all enemies in
// a radius at the impact point.

using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    public class AoeProjectile : MonoBehaviour
    {
        [Header("Movement Defaults")]
        [SerializeField] private float defaultSpeed = 5f;
        [SerializeField] private float defaultMaxLifetime = 5f;
        [SerializeField] private float defaultHitRadius = 0.15f;

        [Header("Homing")]
        [SerializeField] private float defaultHomingStrength = 0.6f;
        [SerializeField] private float turnRate = 8f;

        [Header("Explosion Defaults")]
        [SerializeField] private float defaultExplosionRadius = 1.5f;
        [SerializeField] private int defaultExplosionDamage = 3;
        [SerializeField] private LayerMask enemyLayerMask;

        [Header("Explosion VFX")]
        [SerializeField] private GameObject explosionEffectPrefab;
        [SerializeField] private float explosionEffectLifetime = 1f;

        [Header("Visual")]
        [SerializeField] private float rotationOffset = 0f;

        // runtime values
        private float speed;
        private float maxLifetime;
        private float hitRadius;
        private float homingStrength;
        private float explosionRadius;
        private int explosionDamage;

        private Vector3 currentDirection;


        private IEnemy targetEnemy;
        private Transform targetTransform;
        private Vector3 lastKnownTargetPos;
        private float lifeTimer;

        public void Initialize(
            IEnemy target,
            float? speedOverride = null,
            float? hitRadiusOverride = null,
            float? homingOverride = null,
            float? maxLifetimeOverride = null,
            float? explosionRadiusOverride = null,
            int? explosionDamageOverride = null)
        {
            targetEnemy = target;

            speed = speedOverride ?? defaultSpeed;
            hitRadius = hitRadiusOverride ?? defaultHitRadius;
            homingStrength = homingOverride ?? defaultHomingStrength;
            maxLifetime = maxLifetimeOverride ?? defaultMaxLifetime;
            explosionRadius = explosionRadiusOverride ?? defaultExplosionRadius;
            explosionDamage = explosionDamageOverride ?? defaultExplosionDamage;

            if (targetEnemy is Component c)
            {
                targetTransform = c.transform;
                lastKnownTargetPos = targetTransform.position;
                currentDirection = (lastKnownTargetPos - transform.position).normalized;
            }
            else
            {
                Debug.LogError("AoeProjectile.Initialize: Target is not a Component.");
                Destroy(gameObject);
            }
        }


        private void Update()
        {
            lifeTimer += Time.deltaTime;
            if (lifeTimer >= maxLifetime)
            {
                Destroy(gameObject);
                return;
            }

            if (targetTransform != null && targetEnemy != null && targetEnemy.IsAlive)
            {
                lastKnownTargetPos = targetTransform.position;
            }

            Vector3 position = transform.position;
            Vector3 toTarget = lastKnownTargetPos - position;

            if (toTarget.sqrMagnitude <= hitRadius * hitRadius)
            {
                Explode();
                return;
            }

            Vector3 desiredDir = toTarget.normalized;
            if (currentDirection.sqrMagnitude < 0.0001f)
            {
                currentDirection = desiredDir;
            }
            else
            {
                float t = Mathf.Clamp01(homingStrength * Time.deltaTime * turnRate);
                currentDirection = Vector3.Lerp(currentDirection, desiredDir, t).normalized;
            }

            transform.position = position + currentDirection * speed * Time.deltaTime;

            if (currentDirection.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
            }

        }

        private void Explode()
        {
            if (explosionEffectPrefab != null)
            {
                GameObject fx = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
                if (explosionEffectLifetime > 0f)
                {
                    Destroy(fx, explosionEffectLifetime);
                }
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, enemyLayerMask);

            foreach (Collider2D hit in hits)
            {
                IEnemy enemy = hit.GetComponent<IEnemy>();
                if (enemy != null && enemy.IsAlive)
                {
                    enemy.TakeDamage(explosionDamage);
                }
            }

            Destroy(gameObject);
        }



        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}

