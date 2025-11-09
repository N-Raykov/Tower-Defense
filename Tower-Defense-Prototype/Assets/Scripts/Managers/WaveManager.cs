// Handles spawning of multiple waves using EnemyWaveConfig assets
// and integrates with GameManager game states.

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Level;
using TowerDefense.Enemies;
using TowerDefense.Managers;

namespace TowerDefense.Managers
{
    public class WaveManager : MonoBehaviour
    {
        [Header("Waves")]
        [Tooltip("List of wave configurations in order.")]
        [SerializeField] private List<EnemyWaveConfig> waves = new List<EnemyWaveConfig>();

        [Tooltip("Automatically start the first wave when the scene starts.")]
        [SerializeField] private bool autoStartFirstWave = false;

        [Header("References")]
        [SerializeField] private LevelGrid levelGrid;

        private int currentWaveIndex = -1;
        private bool isSpawning;

        private void Start()
        {
            if (levelGrid == null)
            {
                levelGrid = FindObjectOfType<LevelGrid>();
            }

            if (autoStartFirstWave && waves.Count > 0)
            {
                StartNextWave();
            }
        }

        public void StartNextWave()
        {
            if (isSpawning)
            {
                return;
            }

            if (waves.Count == 0)
            {
                Debug.LogWarning("WaveManager: No waves configured.");
                return;
            }

            if (currentWaveIndex >= waves.Count - 1)
            {
                Debug.Log("WaveManager: No more waves to start.");
                return;
            }

            currentWaveIndex++;
            EnemyWaveConfig waveConfig = waves[currentWaveIndex];

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.WaveInProgress);
            }

            StartCoroutine(SpawnWaveCoroutine(waveConfig));
        }

        private IEnumerator SpawnWaveCoroutine(EnemyWaveConfig waveConfig)
        {
            isSpawning = true;

            if (waveConfig.initialDelay > 0f)
            {
                yield return new WaitForSeconds(waveConfig.initialDelay);
            }

            if (levelGrid == null)
            {
                Debug.LogError("WaveManager: No LevelGrid assigned or found.");
                isSpawning = false;
                yield break;
            }

            Vector3 spawnPos = levelGrid.SpawnWorldPosition;

            // Spawn each instruction group
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

                if (instruction.delayAfterGroup > 0f)
                {
                    yield return new WaitForSeconds(instruction.delayAfterGroup);
                }
            }

            // All enemies for this wave have been spawned.
            // Wait until they are all gone (killed or reached goal).
            if (GameManager.Instance != null)
            {
                yield return new WaitUntil(() => GameManager.Instance.ActiveEnemies == 0);

                bool hasMoreWaves = currentWaveIndex < waves.Count - 1;
                GameManager.Instance.HandleWaveCleared(hasMoreWaves);
            }

            isSpawning = false;
        }
    }
}

