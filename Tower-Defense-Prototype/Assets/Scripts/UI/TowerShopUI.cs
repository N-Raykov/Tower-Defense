// Controls the tower shop UI: populates items and opens/closes

using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Towers;

namespace TowerDefense.UI
{
    public class TowerShopUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject shopPanelRoot;
        [SerializeField] private Transform itemsContainer;
        [SerializeField] private TowerShopItemUI itemPrefab;

        [Header("Available Towers")]
        [SerializeField] private List<TowerDefinition> availableTowers = new List<TowerDefinition>();

        [Header("Placement")]
        [SerializeField] private TowerPlacementManager placementManager;

        private bool isInitialized;

        private void Start()
        {
            if (shopPanelRoot != null)
            {
                shopPanelRoot.SetActive(false);
            }

            BuildShop();
        }

        private void BuildShop()
        {
            if (isInitialized) return;
            isInitialized = true;

            if (shopPanelRoot == null || itemsContainer == null || itemPrefab == null)
            {
                Debug.LogError("TowerShopUI: Missing references.");
                return;
            }

            for (int i = itemsContainer.childCount - 1; i >= 0; i--)
            {
                Destroy(itemsContainer.GetChild(i).gameObject);
            }

            foreach (var def in availableTowers)
            {
                if (def == null) continue;

                TowerShopItemUI item =
                    Instantiate(itemPrefab, itemsContainer);
                item.Initialize(def, this);
            }
        }

        public void HandleItemSelected(TowerDefinition def)
        {
            if (placementManager != null)
            {
                placementManager.SelectTower(def);
            }

            // Optional: close shop after selecting
            CloseShop();
        }

        public void OpenShop()
        {
            if (shopPanelRoot != null)
                shopPanelRoot.SetActive(true);
        }

        public void CloseShop()
        {
            if (shopPanelRoot != null)
                shopPanelRoot.SetActive(false);
        }

        public void ToggleShop()
        {
            if (shopPanelRoot == null) return;
            shopPanelRoot.SetActive(!shopPanelRoot.activeSelf);
        }
    }
}


