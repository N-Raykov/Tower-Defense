// Very simple spawner for testing enemy movement. Spawns a single enemy on Start.

using UnityEngine;
using TowerDefense.Level;

namespace TowerDefense.Enemies
{
    public class EnemyTestSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject enemyPrefab;
        [SerializeField] private LevelGrid levelGrid;

        private void Start()
        {
            if (enemyPrefab == null)
            {
                Debug.LogError("EnemyTestSpawner: Enemy prefab is not assigned.");
                return;
            }

            if (levelGrid == null)
            {
                levelGrid = FindObjectOfType<LevelGrid>();
            }

            if (levelGrid == null)
            {
                Debug.LogError("EnemyTestSpawner: No LevelGrid found in the scene.");
                return;
            }

            Vector3 spawnPos = levelGrid.SpawnWorldPosition;
            Instantiate(enemyPrefab, spawnPos, Quaternion.identity);
        }
    }
}
