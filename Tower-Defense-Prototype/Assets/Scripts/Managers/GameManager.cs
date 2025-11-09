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

            OnLivesChanged?.Invoke(lives);
            OnGoldChanged?.Invoke(gold);
            OnActiveEnemiesChanged?.Invoke(activeEnemies);

            ChangeState(currentState);
        }



        public void ChangeState(GameState newState)
        {
            if (newState == currentState)
            {
                return;
            }

            currentState = newState;
            OnGameStateChanged?.Invoke(currentState);
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
            if (amount < 0)
            {
                Debug.LogWarning("DamageBase called with negative amount.");
                return;
            }

            lives -= amount;
            if (lives < 0)
            {
                lives = 0;
            }

            OnLivesChanged?.Invoke(lives);

            if (lives <= 0 && currentState != GameState.GameOver)
            {
                ChangeState(GameState.GameOver);
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
