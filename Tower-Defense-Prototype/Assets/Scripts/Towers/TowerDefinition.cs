// ScriptableObject describing a tower for use in the shop and
// other systems (cost, stats, prefab, icon, etc.).


using UnityEngine;

namespace TowerDefense.Towers
{
    [CreateAssetMenu(
        fileName = "TowerDefinition",
        menuName = "TowerDefense/Tower Definition")]
    public class TowerDefinition : ScriptableObject
    {
        [Header("Basic Info")]
        public string towerName = "New Tower";
        [TextArea]
        public string description;

        [Header("Visuals")]
        public Sprite icon;

        [Header("Prefab")]
        public GameObject towerPrefab;

        [Header("Economy")]
        public int cost = 50;

        [Header("Upgrade")]
        [Tooltip("Next level of this tower (optional).")]
        public TowerDefinition upgradeDefinition;

        [Tooltip("Gold cost to upgrade from THIS definition to its upgradeDefinition.")]
        public int upgradeCost;


        [Header("Core Stats (for display)")]
        public float range = 2.5f;
        public float baseDamage = 1;
        public float shotsPerSecond = 1f;

        [Header("Special (for display)")]
        public string specialEffect;
    }
}

