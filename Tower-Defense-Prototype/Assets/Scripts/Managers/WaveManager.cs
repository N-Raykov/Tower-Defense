// Handles spawning of multiple waves using EnemyWaveConfig assets
// and integrates with GameManager game states.

using System;
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

        [Header("References")]
        [SerializeField] private LevelGrid levelGrid;

        [Header("Rest Settings")]
        [SerializeField] private float restDuration = 5f;

        public float RestDuration => restDuration;

        public float RestTimeRemaining { get; private set; }

        private bool isInRestPeriod;


        private int currentWaveIndex = -1;
        private bool isSpawning;

        public int CurrentWaveNumber => currentWaveIndex + 1;
        public int TotalWaves => waves != null ? waves.Count : 0;

        public event Action<int, int> OnWaveStarted;


        private void Start()
        {
            StartCoroutine(InitialRestThenFirstWave());
        }


        private void Update()
        {
            if (isInRestPeriod)
            {
                RestTimeRemaining = Mathf.Max(0f, RestTimeRemaining - Time.deltaTime);
            }
        }

        public void StartNextWave()
        {
            if (isSpawning)
                return;

            if (waves == null || waves.Count == 0)
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

            OnWaveStarted?.Invoke(CurrentWaveNumber, TotalWaves);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.WaveInProgress);
            }

            isSpawning = true;
            StartCoroutine(SpawnWaveCoroutine(waveConfig));
        }



        public void ResetWaves()
        {
            StopAllCoroutines();

            currentWaveIndex = -1;
            isSpawning = false;
            RestTimeRemaining = 0f;

            StartCoroutine(InitialRestThenFirstWave());
        }


        private IEnumerator RestPhase()
        {
            RestTimeRemaining = restDuration;

            float elapsed = 0f;
            while (elapsed < restDuration)
            {
                yield return null;
                elapsed += Time.deltaTime;
                RestTimeRemaining = Mathf.Max(0f, restDuration - elapsed);
            }

            RestTimeRemaining = 0f;
        }


        private IEnumerator InitialRestThenFirstWave()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.ChangeState(GameState.BuildPhase);
            }

            if (restDuration > 0f)
            {
                yield return RestPhase();
            }

            StartNextWave();
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

            if (GameManager.Instance != null)
            {
                yield return new WaitUntil(() => GameManager.Instance.ActiveEnemies == 0);

                bool hasMoreWaves = currentWaveIndex < waves.Count - 1;

                isSpawning = false;

                if (hasMoreWaves)
                {
                    GameManager.Instance.ChangeState(GameState.BuildPhase);

                    yield return RestPhase();

                    StartNextWave();
                }
                else
                {
                    GameManager.Instance.ChangeState(GameState.Victory);
                }
            }
            else
            {
                isSpawning = false;
            }

        }
    }
}

