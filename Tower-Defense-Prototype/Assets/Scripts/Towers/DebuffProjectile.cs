// Projectile that deals impact damage and applies a burning debuff
// to enemies that implement IBurnable.

using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    public class DebuffProjectile : MonoBehaviour
    {
        [Header("Movement Defaults")]
        [SerializeField] private float defaultSpeed = 6f;
        [SerializeField] private float defaultMaxLifetime = 5f;
        [SerializeField] private float defaultHitRadius = 0.1f;

        [Header("Burn Defaults")]
        [SerializeField] private int defaultBurnDamagePerTick = 1;
        [SerializeField] private float defaultBurnDuration = 3f;
        [SerializeField] private float defaultBurnTickInterval = 0.5f;

        private int burnDamagePerTick;
        private float burnDuration;
        private float burnTickInterval;

        [Header("Homing")]
        [SerializeField] private float defaultHomingStrength = 0f; 
        [SerializeField] private float turnRate = 8f;

        [Header("Visual")]
        [SerializeField] private float rotationOffset = 0f;

        private float speed;
        private float maxLifetime;
        private float hitRadius;
        private float homingStrength;
        private Vector3 currentDirection;

        private int impactDamage;
        private IEnemy targetEnemy;
        private Transform targetTransform;
        private Vector3 lastKnownTargetPos;
        private float lifeTimer;

        public void Initialize(
            IEnemy target,
            int impactDamage,
            float? speedOverride = null,
            float? hitRadiusOverride = null,
            float? homingOverride = null,
            float? maxLifetimeOverride = null,
            int? burnDamageOverride = null,
            float? burnDurationOverride = null,
            float? burnTickIntervalOverride = null)
        {
            this.impactDamage = impactDamage;
            targetEnemy = target;

            speed = speedOverride ?? defaultSpeed;
            hitRadius = hitRadiusOverride ?? defaultHitRadius;
            homingStrength = homingOverride ?? defaultHomingStrength;
            maxLifetime = maxLifetimeOverride ?? defaultMaxLifetime;

            burnDamagePerTick = burnDamageOverride ?? defaultBurnDamagePerTick;
            burnDuration = burnDurationOverride ?? defaultBurnDuration;
            burnTickInterval = burnTickIntervalOverride ?? defaultBurnTickInterval;

            if (targetEnemy is Component c)
            {
                targetTransform = c.transform;
                lastKnownTargetPos = targetTransform.position;
                currentDirection = (lastKnownTargetPos - transform.position).normalized;
            }
            else
            {
                Debug.LogError("DebuffProjectile.Initialize: Target is not a Component.");
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
                HitTarget();
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

        private void HitTarget()
        {
            if (targetEnemy != null && targetEnemy.IsAlive)
            {
                targetEnemy.TakeDamage(impactDamage);

                if (targetEnemy is IBurnable burnable)
                {
                    burnable.ApplyBurn(burnDamagePerTick, burnDuration, burnTickInterval);
                }
            }

            Destroy(gameObject);
        }
    }
}
