// Controls the pause menu and allows adjusting game speed.

// ------------------------------------------------------------------------------
// PauseMenuUI.cs (robust version)
// ------------------------------------------------------------------------------
using UnityEngine;
using TowerDefense.Managers;

namespace TowerDefense.UI
{
    public class PauseMenuUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private GameObject pausePanel;

        [Header("Optional")]
        [SerializeField] private bool toggleWithEscapeKey = true;

        private GameManager gameManager;

        // Helper: always try to get GameManager.Instance on demand
        private GameManager GM
        {
            get
            {
                if (gameManager == null)
                {
                    gameManager = GameManager.Instance;
                    if (gameManager == null)
                    {
                        Debug.LogError("PauseMenuUI: GameManager.Instance is null. " +
                                       "Make sure a GameManager exists in the scene before PauseMenuUI.");
                    }
                }

                return gameManager;
            }
        }

        private void Awake()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void Update()
        {
            if (!toggleWithEscapeKey)
                return;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePause();
            }
        }

        public void TogglePause()
        {
            var gm = GM; // try to resolve

            if (gm == null)
            {
                // Still allow panel to show/hide so button isn't "dead"
                if (pausePanel != null)
                {
                    bool newState = !pausePanel.activeSelf;
                    pausePanel.SetActive(newState);
                }
                return;
            }

            if (gm.CurrentState == GameState.Paused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }

        public void Pause()
        {
            var gm = GM;
            if (gm == null) return;

            gm.ChangeState(GameState.Paused);

            if (pausePanel != null)
            {
                pausePanel.SetActive(true);
            }
        }

        public void Resume()
        {
            var gm = GM;
            if (gm == null) return;

            gm.ResumeFromPause();

            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        // Called by UI buttons to change speed (0.5x, 1x, 2x, etc.)
        public void SetSpeed(float speed)
        {
            var gm = GM;
            if (gm == null) return;

            gm.SetGameSpeed(speed);
        }

        public void RestartGame()
        {
            var gm = GM;
            if (gm == null) return;

            gm.ResumeFromPause();

            gm.RestartGame();

            // Hide pause menu
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

    }
}
