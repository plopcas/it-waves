using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using ITWaves.Systems;

namespace ITWaves.UI
{
    public class PauseMenuController : MonoBehaviour
    {
        [Header("UI Elements")]
        [SerializeField, Tooltip("Pause menu panel.")]
        private GameObject pauseMenuPanel;

        [SerializeField, Tooltip("Resume button.")]
        private Button resumeButton;

        [SerializeField, Tooltip("Main Menu button.")]
        private Button mainMenuButton;

        [Header("Audio")]
        [SerializeField, Tooltip("Sound to play when clicking menu buttons.")]
        private AudioClip menuClickSound;

        private bool isPaused = false;

        private void Start()
        {
            // Setup buttons
            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(HandleResume);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(HandleMainMenu);
            }

            // Hide pause menu initially
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }
        }

        private void Update()
        {
            // Check for Escape key press using new Input System
            if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                if (isPaused)
                {
                    HandleResume();
                }
                else
                {
                    HandlePause();
                }
            }
        }

        private void HandlePause()
        {
            isPaused = true;

            // Show pause menu
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(true);
            }

            // Pause the game
            Time.timeScale = 0f;

            // Play menu sound
            PlayMenuSound();
        }

        private void HandleResume()
        {
            isPaused = false;

            // Hide pause menu
            if (pauseMenuPanel != null)
            {
                pauseMenuPanel.SetActive(false);
            }

            // Resume the game
            Time.timeScale = 1f;

            // Play menu sound
            PlayMenuSound();
        }

        private void HandleMainMenu()
        {
            // Play menu sound
            PlayMenuSound();

            // Resume time before loading scene
            Time.timeScale = 1f;

            // Return to main menu
            SceneManager.LoadScene("MainMenu");
        }

        private void PlayMenuSound()
        {
            if (MusicManager.Instance != null && menuClickSound != null)
            {
                MusicManager.Instance.PlayUISound(menuClickSound);
            }
        }

        private void OnDestroy()
        {
            // Ensure time scale is reset when this object is destroyed
            Time.timeScale = 1f;
        }
    }
}

