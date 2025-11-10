// Central controller for the game. Responsible for global resources (money, health) 
// and game state transitions (ex. wave in progress)

using System;
using UnityEngine;

namespace TowerDefense.Managers
{
    public enum GameState
    {
        MainMenu,
        BuildPhase,
        WaveInProgress,
        Paused,
        GameOver,
        Victory
    }

    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Starting Values")]
        [SerializeField] private int startingLives = 20;
        [SerializeField] private int startingGold = 100;

        [Header("Runtime Debug (read-only)")]
        [SerializeField] private GameState currentState = GameState.BuildPhase;
        [SerializeField] private int lives;
        [SerializeField] private int gold;
        [SerializeField] private int activeEnemies;

        [Header("Game Speed")]
        [SerializeField] private float currentGameSpeed = 1f;

        public float CurrentGameSpeed => currentGameSpeed;

        public event Action<float> OnGameSpeedChanged;

        private GameState previousNonPausedState;

        public GameState CurrentState => currentState;

        public int Lives => lives;

        public int Gold => gold;

        public int ActiveEnemies => activeEnemies;

        public event Action<int> OnActiveEnemiesChanged;

        // Events (observer-style) for other systems (UI, spawners, etc.).
        public event Action<GameState> OnGameStateChanged;
        public event Action<int> OnLivesChanged;
        public event Action<int> OnGoldChanged;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeGame();
        }
        private void InitializeGame()
        {
            lives = startingLives;
            gold = startingGold;
            activeEnemies = 0;

            currentGameSpeed = 1f;
            Time.timeScale = currentGameSpeed;

            OnLivesChanged?.Invoke(lives);
            OnGoldChanged?.Invoke(gold);
            OnActiveEnemiesChanged?.Invoke(activeEnemies);

            ChangeState(GameState.BuildPhase);
        }

        public void SetGameSpeed(float speed)
        {
            // Clamp to a reasonable range
            currentGameSpeed = Mathf.Clamp(speed, 0.1f, 4f);

            if (currentState != GameState.Paused)
            {
                Time.timeScale = currentGameSpeed;
            }

            OnGameSpeedChanged?.Invoke(currentGameSpeed);
        }


        public void ChangeState(GameState newState)
        {
            if (newState == currentState)
            {
                return;
            }

            if (newState == GameState.Paused)
            {
                previousNonPausedState = currentState;
            }

            currentState = newState;

            if (currentState == GameState.Paused)
            {
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = currentGameSpeed;
            }

            OnGameStateChanged?.Invoke(currentState);
        }

        public void ResumeFromPause()
        {
            if (currentState == GameState.Paused)
            {
                ChangeState(previousNonPausedState);
            }
        }

        // ------------------------------------------------------------------------------
        // RestartGame
        // ------------------------------------------------------------------------------
        public void RestartGame()
        {
            Debug.Log("Restarting game...");

            // Reset time
            Time.timeScale = 1f;
            currentGameSpeed = 1f;

            // Reset player stats
            lives = startingLives;
            gold = startingGold;

            OnLivesChanged?.Invoke(lives);
            OnGoldChanged?.Invoke(gold);

            // Destroy all existing towers
            var towersParent = GameObject.Find("Towers");
            if (towersParent != null)
            {
                for (int i = towersParent.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(towersParent.transform.GetChild(i).gameObject);
                }
            }

            foreach (var enemy in FindObjectsOfType<TowerDefense.Enemies.BasicEnemy>())
            {
                Destroy(enemy.gameObject);
            }

            activeEnemies = 0;
            OnActiveEnemiesChanged?.Invoke(activeEnemies);

            var waveManager = FindObjectOfType<TowerDefense.Managers.WaveManager>();
            if (waveManager != null)
            {
                waveManager.ResetWaves();
            }

            ChangeState(GameState.BuildPhase);

            Debug.Log("Game restarted to Wave 1.");
        }


        public void AddGold(int amount)
        {
            gold += amount;
            if (gold < 0)
            {
                gold = 0;
            }

            OnGoldChanged?.Invoke(gold);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("TrySpendGold called with negative amount.");
                return false;
            }

            if (gold < amount)
            {
                return false;
            }

            gold -= amount;
            OnGoldChanged?.Invoke(gold);
            return true;
        }



        public void RegisterEnemySpawned()
        {
            activeEnemies++;
            OnActiveEnemiesChanged?.Invoke(activeEnemies);
        }

        public void RegisterEnemyDestroyed()
        {
            if (activeEnemies > 0)
            {
                activeEnemies--;
                OnActiveEnemiesChanged?.Invoke(activeEnemies);
            }
        }



        public void DamageBase(int amount)
        {
            if (amount <= 0)
                return;

            // If we're already dead or finished, ignore further damage
            if (currentState == GameState.GameOver || currentState == GameState.Victory)
                return;

            lives -= amount;

            if (lives <= 0)
            {
                lives = 0;
                OnLivesChanged?.Invoke(lives);
                HandleGameOver();
            }
            else
            {
                OnLivesChanged?.Invoke(lives);
            }
        }

        private void HandleGameOver()
        {
            Debug.Log("GAME OVER");

            ChangeState(GameState.GameOver);

            var waveManager = FindObjectOfType<TowerDefense.Managers.WaveManager>();
            if (waveManager != null)
            {
                waveManager.StopAllCoroutines();
            }

            foreach (var enemy in FindObjectsOfType<TowerDefense.Enemies.BasicEnemy>())
            {
                Destroy(enemy.gameObject);
            }

            activeEnemies = 0;
            OnActiveEnemiesChanged?.Invoke(activeEnemies);

            var towersParent = GameObject.Find("Towers");
            if (towersParent != null)
            {
                for (int i = towersParent.transform.childCount - 1; i >= 0; i--)
                {
                    Destroy(towersParent.transform.GetChild(i).gameObject);
                }
            }

        }


        public void HandleWaveCleared(bool hasMoreWaves)
        {
            if (hasMoreWaves)
            {
                ChangeState(GameState.BuildPhase);
            }
            else
            {
                ChangeState(GameState.Victory);
            }
        }

        public void TogglePause()
        {
            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.BuildPhase);
                Time.timeScale = 1f;
            }
            else
            {
                ChangeState(GameState.Paused);
                Time.timeScale = 0f;
            }
        }
    }
}
