// Helper component that snaps its GameObject to the nearest
// cell center of a given Grid. Works in edit mode as well.

using UnityEngine;

namespace TowerDefense.Level
{
    // Attach to any GameObject that should sit exactly on a grid cell. 

    [ExecuteAlways]
    public class SnapToGrid : MonoBehaviour
    {
        [SerializeField] private Grid grid;

        [Tooltip("If enabled, keeps snapping in the editor whenever the object moves.")]
        [SerializeField] private bool snapContinuouslyInEditor = true;

        private void OnValidate()
        {
            Snap();
        }

        private void Update()
        {
            if (!Application.isPlaying && snapContinuouslyInEditor)
            {
                Snap();
            }
        }


        public void Snap()
        {
            if (grid == null)
            {
                grid = GetComponentInParent<Grid>();
            }

            if (grid == null)
            {
                return;
            }

            Vector3Int cell = grid.WorldToCell(transform.position);
            Vector3 center = grid.GetCellCenterWorld(cell);
            transform.position = center;
        }
    }
}
