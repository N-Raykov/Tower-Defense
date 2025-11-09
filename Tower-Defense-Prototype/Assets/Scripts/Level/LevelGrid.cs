// Handles grid/tilemap access and provides helper methods for
// snapping positions to the grid and checking buildable tiles.

using UnityEngine;
using UnityEngine.Tilemaps;

namespace TowerDefense.Level
{

    // Provides access to the grid and tilemaps used for the level.
    // Allows other systems to query buildable cells and convert between world and grid positions

    public class LevelGrid : MonoBehaviour
    {
        public static LevelGrid Instance { get; private set; }

        [Header("References")]
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap groundTilemap;
        [SerializeField] private Tilemap pathTilemap;
        [SerializeField] private Tilemap buildableTilemap;

        [Header("Path Endpoints (snapped to grid)")]
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform goalPoint;

        // World positions where enemies should spawn and go
        public Vector3 SpawnWorldPosition => spawnPoint != null ? spawnPoint.position : Vector3.zero;

        public Vector3 GoalWorldPosition => goalPoint != null ? goalPoint.position : Vector3.zero;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (grid == null)
            {
                grid = GetComponent<Grid>();
            }

            if (grid == null)
            {
                Debug.LogError("LevelGrid: No Grid component assigned or found.");
            }
        }

        // Converts a world position to a grid cell.
        public Vector3Int WorldToCell(Vector3 worldPosition)
        {
            return grid.WorldToCell(worldPosition);
        }

        // Converts a grid cell position to the center of that cell in world space.
        public Vector3 CellToWorldCenter(Vector3Int cellPosition)
        {
            return grid.GetCellCenterWorld(cellPosition);
        }

        // Snaps a world position to the center of the nearest grid cell.
        public Vector3 SnapToGrid(Vector3 worldPosition)
        {
            Vector3Int cell = grid.WorldToCell(worldPosition);
            return grid.GetCellCenterWorld(cell);
        }

        public bool IsBuildable(Vector3 worldPosition)
        {
            Vector3Int cell = grid.WorldToCell(worldPosition);

            if (pathTilemap != null && pathTilemap.HasTile(cell))
            {
                return false;
            }

            if (buildableTilemap != null)
            {
                return buildableTilemap.HasTile(cell);
            }

            if (groundTilemap != null)
            {
                return groundTilemap.HasTile(cell);
            }

            return false;
        }

        public bool IsPath(Vector3 worldPosition)
        {
            Vector3Int cell = grid.WorldToCell(worldPosition);
            return pathTilemap != null && pathTilemap.HasTile(cell);
        }
    }
}
