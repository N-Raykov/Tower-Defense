// Spawns enemies according to an EnemyWaveConfig. Configurable
// via the Inspector (enemy types, counts, delays) without code changes.

using System.Collections;
using UnityEngine;
using TowerDefense.Level;

namespace TowerDefense.Enemies
{
    public class EnemySpawner : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private EnemyWaveConfig waveConfig;

        [Tooltip("Automatically start spawning when the scene starts.")]
        [SerializeField] private bool autoStart = true;

        [Header("References")]
        [SerializeField] private LevelGrid levelGrid;

        private bool isSpawning;

        private void Start()
        {
            if (levelGrid == null)
            {
                levelGrid = FindObjectOfType<LevelGrid>();
            }

            if (autoStart && waveConfig != null)
            {
                StartWave();
            }
        }
        public void StartWave()
        {
            if (waveConfig == null)
            {
                Debug.LogError("EnemySpawner: No waveConfig assigned.");
                return;
            }

            if (levelGrid == null)
            {
                Debug.LogError("EnemySpawner: No LevelGrid found.");
                return;
            }

            if (isSpawning)
            {
                return;
            }

            StartCoroutine(SpawnWaveCoroutine());
        }

        private IEnumerator SpawnWaveCoroutine()
        {
            isSpawning = true;

            if (waveConfig.initialDelay > 0f)
            {
                yield return new WaitForSeconds(waveConfig.initialDelay);
            }

            Vector3 spawnPos = levelGrid.SpawnWorldPosition;

            foreach (var instruction in waveConfig.spawnInstructions)
            {
                if (instruction.enemyPrefab == null || instruction.count <= 0)
                {
                    continue;
                }

                for (int i = 0; i < instruction.count; i++)
                {
                    Instantiate(instruction.enemyPrefab, spawnPos, Quaternion.identity);

                    if (instruction.delayBetweenSpawns > 0f && i < instruction.count - 1)
                    {
                        yield return new WaitForSeconds(instruction.delayBetweenSpawns);
                    }
                }
            }

            isSpawning = false;
        }
    }
}

