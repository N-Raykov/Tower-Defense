// Simple enemy implementation with health, reward gold, and
// base damage on reaching the goal.

using System;
using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Managers;
using TowerDefense.UI;

namespace TowerDefense.Enemies
{
    [RequireComponent(typeof(EnemyMover))]
    public class BasicEnemy : MonoBehaviour, IEnemy, IBurnable, IPathEnemy
    {
        [Header("Stats")]
        [SerializeField] private int maxHealth = 10;
        [SerializeField] private int rewardGold = 5;
        [SerializeField] private int damageToBase = 1;
        public int MaxHealth => maxHealth;
        public int CurrentHealth => currentHealth;

        public event Action<int, int> OnHealthChanged;

        [Header("Burn Settings")]
        [SerializeField] private bool canBeBurned = true;


        [Header("Burn Visual")]
        [SerializeField] private GameObject burnEffectPrefab;

        private GameObject activeBurnEffect;

        [Header("Gold Popup")]
        [SerializeField] private GameObject goldPopupPrefab;



        [Header("Visuals")]
        [SerializeField] private SpriteRenderer spriteRenderer;
        

        public bool IsAlive => currentHealth > 0;

        private int currentHealth;
        private EnemyMover mover;
        private bool hasNotifiedGoal;

        // Burn state
        private bool isBurning;
        private int burnDamagePerTick;
        private float burnRemainingTime;
        private float burnTickInterval = 0.5f;
        private float burnTickTimer;

        public float PathProgress => mover != null ? mover.PathProgress : 0f;

        private void Awake()
        {
            mover = GetComponent<EnemyMover>();
            if (mover != null)
            {
                mover.OnPathCompleted += HandleReachedGoal;
            }

            currentHealth = maxHealth;
            OnHealthChanged?.Invoke(currentHealth, maxHealth);

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

        private void Update()
        {
            UpdateBurn(Time.deltaTime);
        }

        private void UpdateBurn(float deltaTime)
        {
            if (!isBurning)
                return;

            burnRemainingTime -= deltaTime;
            burnTickTimer -= deltaTime;

            if (burnTickTimer <= 0f)
            {
                burnTickTimer = burnTickInterval;
                if (burnDamagePerTick > 0)
                {
                    TakeDamage(burnDamagePerTick);
                }
            }

            if (burnRemainingTime <= 0f)
            {
                isBurning = false;

                if (activeBurnEffect != null)
                {
                    Destroy(activeBurnEffect);
                    activeBurnEffect = null;
                }
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
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
                Die();
            }
            else
            {
                OnHealthChanged?.Invoke(currentHealth, maxHealth);
            }
        }

        public void ApplyBurn(int damagePerTick, float duration, float tickInterval)
        {
            if (!canBeBurned || damagePerTick <= 0 || duration <= 0f)
                return;

            isBurning = true;

            burnDamagePerTick = Mathf.Max(burnDamagePerTick, damagePerTick);
            burnRemainingTime = Mathf.Max(burnRemainingTime, duration);
            burnTickInterval = tickInterval > 0f ? tickInterval : burnTickInterval;
            burnTickTimer = 0f;

            // Visual: ensure burn effect is active
            if (burnEffectPrefab != null && activeBurnEffect == null)
            {
                activeBurnEffect = Instantiate(burnEffectPrefab, transform);
                activeBurnEffect.transform.localPosition = Vector3.zero;
            }
        }

        private void Die()
        {
            if (activeBurnEffect != null)
            {
                Destroy(activeBurnEffect);
                activeBurnEffect = null;
            }

            if (GameManager.Instance != null)
            {
                GameManager.Instance.AddGold(rewardGold);
            }

            if (goldPopupPrefab != null)
            {
                GameObject popup = Instantiate(goldPopupPrefab, transform.position, Quaternion.identity);
                var popupScript = popup.GetComponent<GoldPopup>();
                if (popupScript != null)
                {
                    popupScript.Initialize(rewardGold);
                }
            }

            Destroy(gameObject);
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
