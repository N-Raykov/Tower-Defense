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

        public GameState CurrentState { get; private set; } = GameState.BuildPhase;

        public int Lives { get; private set; }

        public int Gold { get; private set; }

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
            Lives = startingLives;
            Gold = startingGold;

            OnLivesChanged?.Invoke(Lives);
            OnGoldChanged?.Invoke(Gold);

            // Start in build phase by default; can be changed later from a menu.
            ChangeState(CurrentState);
        }

        public void ChangeState(GameState newState)
        {
            if (newState == CurrentState)
            {
                return;
            }

            CurrentState = newState;
            OnGameStateChanged?.Invoke(CurrentState);
        }

        public void AddGold(int amount)
        {
            Gold += amount;
            if (Gold < 0)
            {
                Gold = 0;
            }

            OnGoldChanged?.Invoke(Gold);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("TrySpendGold called with negative amount.");
                return false;
            }

            if (Gold < amount)
            {
                return false;
            }

            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public void DamageBase(int amount)
        {
            if (amount < 0)
            {
                Debug.LogWarning("DamageBase called with negative amount.");
                return;
            }

            Lives -= amount;
            if (Lives < 0)
            {
                Lives = 0;
            }

            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0 && CurrentState != GameState.GameOver)
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
