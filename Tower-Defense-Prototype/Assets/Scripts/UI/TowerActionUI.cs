// UI for interacting with a selected tower (sell/upgrade).
// Appears near the tower in screen space.

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Managers;
using TowerDefense.Towers;

namespace TowerDefense.UI
{
    public class TowerActionUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private TextMeshProUGUI sellText;
        [SerializeField] private TextMeshProUGUI upgradeText;
        [SerializeField] private Button sellButton;
        [SerializeField] private Button upgradeButton;

        [Header("Economy")]
        [Tooltip("Fraction of invested gold refunded when selling.")]
        [Range(0f, 1f)]
        [SerializeField] private float sellRefundFraction = 0.75f;

        [Header("Placement")]
        [SerializeField] private TowerPlacementManager placementManager;

        private TowerInstance selectedTower;
        private Camera mainCamera;

        private GameManager gameManager;
        private GameManager GM
        {
            get
            {
                if (gameManager == null)
                {
                    gameManager = GameManager.Instance;
                }
                return gameManager;
            }
        }


        private void Awake()
        {
            mainCamera = Camera.main;
            gameManager = GameManager.Instance;

            if (panelRoot != null)
                panelRoot.gameObject.SetActive(false);

            if (sellButton != null)
            {
                sellButton.onClick.RemoveAllListeners();
                sellButton.onClick.AddListener(OnSellClicked);
            }

            if (upgradeButton != null)
            {
                upgradeButton.onClick.RemoveAllListeners();
                upgradeButton.onClick.AddListener(OnUpgradeClicked);
            }
        }

        private void OnEnable()
        {
            if (GM != null)
            {
                GM.OnGoldChanged += HandleGoldChanged;
            }
        }

        private void OnDisable()
        {
            if (GM != null)
            {
                GM.OnGoldChanged -= HandleGoldChanged;
            }
        }


        public void SetSelectedTower(TowerInstance tower)
        {
            selectedTower = tower;

            if (selectedTower == null || panelRoot == null)
            {
                if (panelRoot != null)
                    panelRoot.gameObject.SetActive(false);
                return;
            }

            panelRoot.gameObject.SetActive(true);
            RefreshUI();
            RepositionPanel();
        }

        public void ClearSelection()
        {
            selectedTower = null;
            if (panelRoot != null)
                panelRoot.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (selectedTower != null && panelRoot != null && panelRoot.gameObject.activeSelf)
            {
                RepositionPanel();
            }
        }

        private void RefreshUI()
        {
            if (selectedTower == null) return;

            if (nameText != null)
                nameText.text = selectedTower.Definition != null
                    ? selectedTower.Definition.towerName
                    : "Tower";

            if (levelText != null)
                levelText.text = $"Lv {selectedTower.CurrentLevel}";

            // Sell value
            int sellValue = Mathf.FloorToInt(selectedTower.TotalInvestedGold * sellRefundFraction);
            if (sellText != null)
                sellText.text = $"Sell: +{sellValue}";

            // Upgrade
            if (selectedTower.CanUpgrade)
            {
                int cost = selectedTower.UpgradeCost;
                if (upgradeText != null)
                    upgradeText.text = $"Upgrade: {cost}";

                var gm = GM;
                bool enoughGold = gm != null && gm.Gold >= cost;

                if (upgradeButton != null)
                    upgradeButton.interactable = enoughGold;
            }
            else
            {
                if (upgradeText != null)
                    upgradeText.text = "Upgrade: Max";

                if (upgradeButton != null)
                    upgradeButton.interactable = false;
            }
        }

        private void RepositionPanel()
        {
            if (selectedTower == null || mainCamera == null || panelRoot == null)
                return;

            Vector3 worldPos = selectedTower.transform.position;
            Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

            // Offset panel to the right of the tower
            Vector3 offset = new Vector3(80f, 0f, 0f);
            panelRoot.anchoredPosition = screenPos + offset;
        }

        private void HandleGoldChanged(int gold)
        {
            // Update upgrade button interactability when gold changes
            if (panelRoot != null && panelRoot.gameObject.activeSelf)
            {
                RefreshUI();
            }
        }

        private void OnSellClicked()
        {
            if (selectedTower == null)
                return;

            Vector3 pos = selectedTower.transform.position;

            // Free the grid cell in the placement manager
            if (placementManager != null)
            {
                placementManager.UnregisterTowerCell(pos);
            }

            selectedTower.Sell(sellRefundFraction);
            ClearSelection();
        }

        private void OnUpgradeClicked()
        {
            if (selectedTower == null)
                return;

            TowerInstance newTower = selectedTower.Upgrade();
            if (newTower != null)
            {
                // Keep selection on the upgraded tower
                SetSelectedTower(newTower);
            }
            else
            {
                // Not enough gold or upgrade failed
                RefreshUI();
            }
        }
    }
}

