// Attached to each placed tower to track its definition,
// level, total invested gold, and provide sell/upgrade logic.

using UnityEngine;
using TowerDefense.Managers;

namespace TowerDefense.Towers
{
    public class TowerInstance : MonoBehaviour
    {
        [Header("Definition")]
        public TowerDefinition definition;

        [Header("State")]
        [SerializeField] private int currentLevel = 1;
        [SerializeField] private int totalInvestedGold = 0;

        public TowerDefinition Definition => definition;
        public int CurrentLevel => currentLevel;
        public int TotalInvestedGold => totalInvestedGold;

        public bool CanUpgrade => definition != null && definition.upgradeDefinition != null;
        public int UpgradeCost => definition != null ? definition.upgradeCost : 0;

        private void Awake()
        {
            if (definition != null && totalInvestedGold <= 0)
            {
                totalInvestedGold = definition.cost;
            }
        }
        public void Sell(float refundFraction)
        {
            var gm = GameManager.Instance;
            if (gm != null)
            {
                int refund = Mathf.FloorToInt(totalInvestedGold * Mathf.Clamp01(refundFraction));
                gm.AddGold(refund);
            }

            Destroy(gameObject);
        }

        public TowerInstance Upgrade()
        {
            if (definition == null || definition.upgradeDefinition == null)
                return null;

            var gm = GameManager.Instance;
            if (gm == null)
                return null;

            int cost = definition.upgradeCost;
            if (!gm.TrySpendGold(cost))
            {
                // Not enough gold
                return null;
            }

            int newTotalInvested = totalInvestedGold + cost;
            TowerDefinition upgradeDef = definition.upgradeDefinition;

            Transform parent = transform.parent;
            Vector3 pos = transform.position;
            Quaternion rot = transform.rotation;

            GameObject newObj = Instantiate(upgradeDef.towerPrefab, pos, rot, parent);
            TowerInstance newInstance = newObj.GetComponent<TowerInstance>();
            if (newInstance == null)
            {
                Debug.LogError("Upgraded tower prefab is missing TowerInstance component.");
                // Refund and abort
                gm.AddGold(cost);
                Destroy(newObj);
                return null;
            }

            newInstance.definition = upgradeDef;
            newInstance.currentLevel = this.currentLevel + 1;
            newInstance.totalInvestedGold = newTotalInvested;

            Destroy(gameObject);

            return newInstance;
        }
    }
}

