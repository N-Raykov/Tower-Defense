// Handles selecting a tower type and placing it on the grid when
// the player clicks a valid tile and has enough gold.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TowerDefense.Level;
using TowerDefense.Managers;

namespace TowerDefense.Towers
{
    public class TowerPlacementManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private LevelGrid levelGrid;
        [SerializeField] private Transform towersParent;
        [SerializeField] private LayerMask towerLayerMask;

        [Header("Placement")]
        [SerializeField] private TowerDefinition selectedTower;

        [Header("Grid")]
        [SerializeField] private Grid grid; 

        [Header("Ghost Preview")]
        [SerializeField] private TowerGhostVisual ghostPrefab;

        [Header("Selection")]
        [SerializeField] private TowerDefense.UI.TowerActionUI towerActionUI;

        private TowerInstance selectedPlacedTower;


        private TowerGhostVisual activeGhost;


        private Camera mainCamera;

        private readonly HashSet<Vector3Int> occupiedCells = new HashSet<Vector3Int>();

        private void Awake()
        {
            if (levelGrid == null)
            {
                levelGrid = FindObjectOfType<LevelGrid>();
            }

            if (towersParent == null)
            {
                GameObject parent = GameObject.Find("Towers");
                if (parent == null)
                {
                    parent = new GameObject("Towers");
                }
                towersParent = parent.transform;
            }

            if (grid == null)
            {
                grid = FindObjectOfType<Grid>();
            }

            mainCamera = Camera.main;

            if (ghostPrefab != null)
            {
                activeGhost = Instantiate(ghostPrefab);
                activeGhost.gameObject.SetActive(false);
            }

            if (grid != null && towersParent != null)
            {
                foreach (Transform child in towersParent)
                {
                    Vector3Int cell = grid.WorldToCell(child.position);
                    occupiedCells.Add(cell);
                }
            }
        }


        public void SelectTower(TowerDefinition towerDef)
        {
            selectedTower = towerDef;

            if (activeGhost != null)
            {
                if (selectedTower == null)
                {
                    activeGhost.gameObject.SetActive(false);
                    return;
                }

                // Try to grab a sprite from the tower prefab
                Sprite sprite = null;
                if (selectedTower.towerPrefab != null)
                {
                    var sr = selectedTower.towerPrefab.GetComponentInChildren<SpriteRenderer>();
                    if (sr != null)
                    {
                        sprite = sr.sprite;
                    }
                }

                activeGhost.SetSprite(sprite);
                activeGhost.gameObject.SetActive(true);
            }
        }

        public void UnregisterTowerCell(Vector3 worldPosition)
        {
            if (grid == null) return;
            Vector3Int cell = grid.WorldToCell(worldPosition);
            occupiedCells.Remove(cell);
        }

        private void Update()
        {
            bool overUI = EventSystem.current != null &&
                          EventSystem.current.IsPointerOverGameObject();

            if (selectedTower != null)
            {
                HandleGhostPreview(overUI);

                if (Input.GetMouseButtonDown(0) && !overUI)
                {
                    TryPlaceSelectedTower();
                }

                if (Input.GetMouseButtonDown(1))
                {
                    selectedTower = null;

                    if (activeGhost != null)
                    {
                        activeGhost.gameObject.SetActive(false);
                    }

                    if (towerActionUI != null)
                    {
                        towerActionUI.ClearSelection();
                    }
                }

                return;
            }

            if (activeGhost != null && activeGhost.gameObject.activeSelf)
            {
                activeGhost.gameObject.SetActive(false);
            }

            if (Input.GetMouseButtonDown(0) && !overUI)
            {
                TrySelectTowerUnderMouse();
            }

            if (Input.GetMouseButtonDown(1))
            {
                if (towerActionUI != null)
                {
                    towerActionUI.ClearSelection();
                }
            }
        }

        private void HandleGhostPreview(bool overUI)
        {
            if (activeGhost == null || mainCamera == null || levelGrid == null || grid == null)
                return;

            if (overUI)
            {
                if (activeGhost.gameObject.activeSelf)
                    activeGhost.gameObject.SetActive(false);
                return;
            }

            if (!activeGhost.gameObject.activeSelf)
                activeGhost.gameObject.SetActive(true);

            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            worldPos.z = 0f;

            Vector3 snappedPos = levelGrid.SnapToGrid(worldPos);
            activeGhost.transform.position = snappedPos;

            Vector3Int cell = grid.WorldToCell(snappedPos);
            bool cellOccupied = IsCellOccupied(cell);

            bool canBuildHere = levelGrid.IsBuildable(snappedPos) && !cellOccupied;
            bool hasEnoughGold = GameManager.Instance != null &&
                                 GameManager.Instance.Gold >= selectedTower.cost;

            activeGhost.SetState(canBuildHere, hasEnoughGold);
        }


        private void TrySelectTowerUnderMouse()
        {
            if (mainCamera == null || towerActionUI == null)
                return;

            Vector3 mouseWorld = mainCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorld.z = 0f; // 2D world, z is irrelevant but nice to clean it
            Vector2 point = mouseWorld;

            // OverlapPoint with layer mask: finds any collider under the cursor on the Tower layer(s)
            Collider2D hit = Physics2D.OverlapPoint(point, towerLayerMask);

            if (hit == null)
            {
                Debug.Log($"No tower hit at {point}");
                return;
            }

            // Log to confirm we actually hit something
            Debug.Log($"Hit {hit.name} on layer {LayerMask.LayerToName(hit.gameObject.layer)}");

            TowerInstance instance = hit.GetComponentInParent<TowerInstance>();
            if (instance != null)
            {
                selectedPlacedTower = instance;
                towerActionUI.SetSelectedTower(instance);
                Debug.Log($"Selected tower: {instance.Definition?.towerName ?? instance.name}");
            }
        }




        private void TryPlaceSelectedTower()
        {
            if (mainCamera == null || levelGrid == null || selectedTower == null || grid == null)
                return;

            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            worldPos.z = 0f;

            Vector3 snappedPos = levelGrid.SnapToGrid(worldPos);
            Vector3Int cell = grid.WorldToCell(snappedPos);

            if (!levelGrid.IsBuildable(snappedPos))
            {
                Debug.Log("Cannot place tower: tile is not buildable.");
                return;
            }

            if (IsCellOccupied(cell))
            {
                Debug.Log("Cannot place tower: there is already a tower on this tile.");
                return;
            }

            // Check if player can afford the tower
            if (GameManager.Instance == null)
            {
                Debug.LogError("TowerPlacementManager: No GameManager instance.");
                return;
            }

            if (!GameManager.Instance.TrySpendGold(selectedTower.cost))
            {
                Debug.Log("Not enough gold to place this tower.");
                return;
            }

            // Place tower
            GameObject tower = Instantiate(
                selectedTower.towerPrefab,
                snappedPos,
                Quaternion.identity,
                towersParent);

            // Snap again just in case
            var snap = tower.GetComponentInChildren<TowerDefense.Level.SnapToGrid>();
            if (snap != null)
            {
                snap.Snap();
            }

            occupiedCells.Add(cell);
        }



        private bool IsCellOccupied(Vector3Int cell)
        {
            return occupiedCells.Contains(cell);
        }
    }
}
