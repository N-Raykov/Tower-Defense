// Simple homing-ish projectile that flies toward a target enemy.
// On reaching the target (or its last known position), it deals
// damage and destroys itself.


using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    public class Projectile : MonoBehaviour
    {
        [Header("Movement")]
        [SerializeField] private float speed = 6f;
        [SerializeField] private float maxLifetime = 5f;
        [SerializeField] private float hitRadius = 0.1f;

        [Tooltip("Rotation offset in degrees so the sprite appears correctly oriented.")]
        [SerializeField] private float rotationOffset = 0f;

        private int damage;
        private IEnemy targetEnemy;
        private Transform targetTransform;
        private Vector3 lastKnownTargetPos;
        private float lifeTimer;

        public void Initialize(IEnemy target, int damage)
        {
            this.damage = damage;
            targetEnemy = target;

            if (targetEnemy is Component targetComponent)
            {
                targetTransform = targetComponent.transform;
                lastKnownTargetPos = targetTransform.position;
            }
            else
            {
                Debug.LogError("Projectile.Initialize: Target enemy is not a Component.");
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

            Vector3 direction = toTarget.normalized;
            transform.position = position + direction * speed * Time.deltaTime;

            if (direction.sqrMagnitude > 0.0001f)
            {
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle + rotationOffset);
            }
        }

        private void HitTarget()
        {
            if (targetEnemy != null && targetEnemy.IsAlive)
            {
                targetEnemy.TakeDamage(damage);
            }

            Destroy(gameObject);
        }
    }
}

