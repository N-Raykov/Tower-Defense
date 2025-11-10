// Displays a simple health bar above an enemy by scaling a
// SpriteRenderer based on current / max health.

using UnityEngine;

namespace TowerDefense.Enemies
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private BasicEnemy enemy;
        [SerializeField] private SpriteRenderer fillSprite;

        [Tooltip("Offset from the enemy's position in world units.")]
        [SerializeField] private Vector3 worldOffset = new Vector3(0f, 0.5f, 0f);

        private Vector3 initialScale;

        private void Awake()
        {
            if (enemy == null)
            {
                enemy = GetComponentInParent<BasicEnemy>();
            }

            if (fillSprite == null)
            {
                fillSprite = GetComponent<SpriteRenderer>();
            }

            if (fillSprite != null)
            {
                initialScale = fillSprite.transform.localScale;
            }

            if (enemy != null)
            {
                enemy.OnHealthChanged += HandleHealthChanged;
                // Ensure initial visual
                HandleHealthChanged(enemy.CurrentHealth, enemy.MaxHealth);
            }
        }

        private void OnDestroy()
        {
            if (enemy != null)
            {
                enemy.OnHealthChanged -= HandleHealthChanged;
            }
        }

        private void LateUpdate()
        {
            // Keep bar positioned above enemy
            if (enemy != null)
            {
                transform.position = enemy.transform.position + worldOffset;

                transform.rotation = Quaternion.identity;
            }
        }

        private void HandleHealthChanged(int current, int max)
        {
            if (fillSprite == null || max <= 0) return;

            float fraction = Mathf.Clamp01((float)current / max);
            Vector3 scale = initialScale;
            scale.x = initialScale.x * fraction;

            fillSprite.transform.localScale = scale;
        }
    }
}
