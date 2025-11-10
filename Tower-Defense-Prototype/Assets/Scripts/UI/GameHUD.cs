// Displays player lives, gold, wave, and game state in the UI.

using UnityEngine;
using TMPro;
using TowerDefense.Managers;

namespace TowerDefense.UI
{
    public class GameHUD : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI healthText;
        [SerializeField] private TextMeshProUGUI goldText;
        [SerializeField] private TextMeshProUGUI waveText;
        [SerializeField] private TextMeshProUGUI stateText;

        [SerializeField] private TextMeshProUGUI restTimerText;

        [Header("Managers")]
        [SerializeField] private GameManager gameManager;
        [SerializeField] private WaveManager waveManager;

        private void Awake()
        {
            if (gameManager == null)
            {
                gameManager = GameManager.Instance;
            }

            if (waveManager == null)
            {
                waveManager = FindObjectOfType<WaveManager>();
            }
        }

        private void OnEnable()
        {
            if (gameManager != null)
            {
                gameManager.OnLivesChanged += HandleLivesChanged;
                gameManager.OnGoldChanged += HandleGoldChanged;
                gameManager.OnGameStateChanged += HandleGameStateChanged;
            }

            if (waveManager != null)
            {
                waveManager.OnWaveStarted += HandleWaveStarted;
            }

            // Initial values
            RefreshAll();
        }

        private void OnDisable()
        {
            if (gameManager != null)
            {
                gameManager.OnLivesChanged -= HandleLivesChanged;
                gameManager.OnGoldChanged -= HandleGoldChanged;
                gameManager.OnGameStateChanged -= HandleGameStateChanged;
            }

            if (waveManager != null)
            {
                waveManager.OnWaveStarted -= HandleWaveStarted;
            }
        }

        private void Update()
        {
            UpdateRestTimer();
        }

        private void UpdateRestTimer()
        {
            if (restTimerText == null || waveManager == null || gameManager == null)
                return;

            if (gameManager.CurrentState != GameState.BuildPhase ||
                waveManager.RestTimeRemaining <= 0.01f)
            {
                restTimerText.text = "";
                return;
            }

            float remaining = Mathf.Max(0f, waveManager.RestTimeRemaining);

            restTimerText.text = $"Next wave in: {remaining:0.0}s";
        }



        private void RefreshAll()
        {
            if (gameManager != null)
            {
                HandleLivesChanged(gameManager.Lives);
                HandleGoldChanged(gameManager.Gold);
                HandleGameStateChanged(gameManager.CurrentState);
            }

            if (waveManager != null)
            {
                if (waveManager.TotalWaves > 0 && waveManager.CurrentWaveNumber > 0)
                {
                    HandleWaveStarted(waveManager.CurrentWaveNumber, waveManager.TotalWaves);
                }
                else
                {
                    UpdateWaveText(0, waveManager != null ? waveManager.TotalWaves : 0);
                }
            }
        }

        private void HandleLivesChanged(int lives)
        {
            if (healthText != null)
            {
                healthText.text = $"Health: {lives}";
            }
        }

        private void HandleGoldChanged(int gold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {gold}";
            }
        }

        private void HandleWaveStarted(int currentWave, int totalWaves)
        {
            UpdateWaveText(currentWave, totalWaves);
        }

        private void UpdateWaveText(int currentWave, int totalWaves)
        {
            if (waveText == null) return;

            if (totalWaves <= 0)
            {
                waveText.text = "Wave: -";
            }
            else
            {
                waveText.text = $"Wave: {currentWave} / {totalWaves}";
            }
        }

        private void HandleGameStateChanged(GameState state)
        {
            if (stateText == null) return;

            string text;
            switch (state)
            {
                case GameState.BuildPhase:
                    text = "State: Build";
                    break;
                case GameState.WaveInProgress:
                    text = "State: Wave";
                    break;
                case GameState.Paused:
                    text = "State: Paused";
                    break;
                case GameState.Victory:
                    text = "State: Victory";
                    break;
                case GameState.GameOver:
                    text = "State: Game Over";
                    break;
                default:
                    text = $"State: {state}";
                    break;
            }

            stateText.text = text;
        }
    }
}

