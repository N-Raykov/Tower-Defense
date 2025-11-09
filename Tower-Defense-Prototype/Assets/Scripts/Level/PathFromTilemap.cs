// Builds a list of world-space waypoints for enemies to follow
// based on a path Tilemap between a spawn and goal point.

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace TowerDefense.Level
{
    // Generates a path (list of waypoints) from the path Tilemap, starting at
    // the spawn point's cell and ending at the goal point's cell.
    // Assumes a single-tile-wide connected path using cardinal neighbors.

    public class PathFromTilemap : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Grid grid;
        [SerializeField] private Tilemap pathTilemap;
        [SerializeField] private Transform spawnPoint;
        [SerializeField] private Transform goalPoint;

        private readonly List<Vector3> waypoints = new List<Vector3>();
        public IReadOnlyList<Vector3> Waypoints => waypoints;

        private static readonly Vector3Int[] NeighborOffsets =
        {
            new Vector3Int(1, 0, 0),
            new Vector3Int(-1, 0, 0),
            new Vector3Int(0, 1, 0),
            new Vector3Int(0, -1, 0)
        };

        private void Awake()
        {
            if (grid == null)
            {
                grid = GetComponent<Grid>();
            }

            if (pathTilemap == null)
            {
                Debug.LogError("PathFromTilemap: Path Tilemap is not assigned.");
                return;
            }

            if (spawnPoint == null || goalPoint == null)
            {
                Debug.LogError("PathFromTilemap: SpawnPoint or GoalPoint is not assigned.");
                return;
            }

            BuildPath();
        }

        private void BuildPath()
        {
            waypoints.Clear();

            Vector3Int startCell = grid.WorldToCell(spawnPoint.position);
            Vector3Int goalCell = grid.WorldToCell(goalPoint.position);

            // BFS to find a path through connected path tiles
            var cameFrom = new Dictionary<Vector3Int, Vector3Int>();
            var queue = new Queue<Vector3Int>();
            var visited = new HashSet<Vector3Int>();

            queue.Enqueue(startCell);
            visited.Add(startCell);

            bool found = false;

            while (queue.Count > 0)
            {
                Vector3Int current = queue.Dequeue();

                if (current == goalCell)
                {
                    found = true;
                    break;
                }

                foreach (Vector3Int offset in NeighborOffsets)
                {
                    Vector3Int next = current + offset;

                    if (visited.Contains(next))
                        continue;

                    if (!pathTilemap.HasTile(next))
                        continue;

                    visited.Add(next);
                    queue.Enqueue(next);
                    cameFrom[next] = current;
                }
            }

            if (!found)
            {
                Debug.LogError("PathFromTilemap: No path found between spawn and goal on the path Tilemap.");
                return;
            }

            // Reconstruct path from goal back to start
            var pathCells = new List<Vector3Int>();
            Vector3Int step = goalCell;
            pathCells.Add(step);

            while (step != startCell)
            {
                step = cameFrom[step];
                pathCells.Add(step);
            }

            pathCells.Reverse();

            // Convert cells to world-space centers
            foreach (Vector3Int cell in pathCells)
            {
                Vector3 worldPos = grid.GetCellCenterWorld(cell);
                waypoints.Add(worldPos);
            }

#if UNITY_EDITOR
            Debug.Log($"PathFromTilemap: Built path with {waypoints.Count} waypoints.");
#endif
        }

        private void OnDrawGizmosSelected()
        {
            // Visualize waypoints in the editor
            Gizmos.DrawIcon(transform.position, "sv_icon_dot3_pix16_gizmo", true);

            if (waypoints == null || waypoints.Count == 0)
                return;

            for (int i = 0; i < waypoints.Count; i++)
            {
                Gizmos.DrawSphere(waypoints[i], 0.1f);

                if (i < waypoints.Count - 1)
                {
                    Gizmos.DrawLine(waypoints[i], waypoints[i + 1]);
                }
            }
        }
    }
}

