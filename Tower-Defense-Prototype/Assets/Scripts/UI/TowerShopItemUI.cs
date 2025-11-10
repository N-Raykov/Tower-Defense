// Binds a TowerDefinition to a UI item (icon, name, cost, stats).

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Towers;

namespace TowerDefense.UI
{
    public class TowerShopItemUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI costText;
        [SerializeField] private TextMeshProUGUI statsText;
        [SerializeField] private Button selectButton;

        private TowerDefinition definition;
        private TowerShopUI shop;

        /// <summary>
        /// Bind this UI element to a specific tower definition.
        /// </summary>
        public void Initialize(TowerDefinition towerDef, TowerShopUI shopRef)
        {
            definition = towerDef;
            shop = shopRef;

            if (selectButton == null)
            {
                selectButton = GetComponent<Button>();
            }

            if (selectButton != null)
            {
                selectButton.onClick.RemoveAllListeners();
                selectButton.onClick.AddListener(OnClicked);
            }

            if (definition == null)
            {
                Debug.LogError("TowerShopItemUI.Initialize called with null definition.");
                return;
            }

            if (iconImage != null)
                iconImage.sprite = definition.icon;

            if (nameText != null)
                nameText.text = definition.towerName;

            if (costText != null)
                costText.text = $"Cost: {definition.cost}";

            if (statsText != null)
            {
                statsText.text =
                    $"Range: {definition.range}\n" +
                    $"Rate: {definition.shotsPerSecond:0.##}/s\n" +
                    $"Damage: {definition.baseDamage}\n" +
                    $"{definition.specialEffect}";
            }
        }

        private void OnClicked()
        {
            if (shop != null && definition != null)
            {
                shop.HandleItemSelected(definition);
            }
        }
    }
}

