// Simple enemy implementation with health, reward gold, and
// base damage on reaching the goal.

using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Managers;

namespace TowerDefense.Enemies
{
    [RequireComponent(typeof(EnemyMover))]
    public class BasicEnemy : MonoBehaviour, IEnemy
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private int rewardGold = 5;
        [SerializeField] private int damageToBase = 1;

        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;

        public bool IsAlive => currentHealth > 0;

        private int currentHealth;
        private EnemyMover mover;
        private bool hasNotifiedGoal;

        private void Awake()
        {
            mover = GetComponent<EnemyMover>();
            if (mover != null)
            {
                mover.OnPathCompleted += HandleReachedGoal;
            }

            currentHealth = maxHealth;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        private void OnEnable()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemySpawned();
            }
        }

        public void TakeDamage(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("BasicEnemy.TakeDamage called with negative amount.");
                return;
            }

            if (!IsAlive)
            {
                return;
            }

            currentHealth -= amount;

            if (currentHealth <= 0)
            {
                currentHealth = 0;
                Die();
            }
        }

        private void Die()
        {
            if (!IsAlive) 
            {
                if (GameManager.Instance != null)
                {
                    GameManager.Instance.AddGold(rewardGold);
                }

                Destroy(gameObject);
            }
        }

        private void HandleReachedGoal(EnemyMover _)
        {
            if (hasNotifiedGoal)
                return;

            hasNotifiedGoal = true;

            if (GameManager.Instance != null)
            {
                GameManager.Instance.DamageBase(damageToBase);
            }

            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.RegisterEnemyDestroyed();
            }

            if (mover != null)
            {
                mover.OnPathCompleted -= HandleReachedGoal;
            }
        }

    }
}
