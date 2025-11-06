using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ITWaves.Systems;
using ITWaves.Core;

namespace ITWaves.UI
{
    public class GameOverController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField, Tooltip("Continue button - retry from highest wave reached.")]
        private Button continueButton;

        [SerializeField, Tooltip("Quit button - return to main menu.")]
        private Button quitButton;
        
        [Header("Display")]
        [SerializeField, Tooltip("Wave number text.")]
        private TextMeshProUGUI waveText;

        [SerializeField, Tooltip("Score text.")]
        private TextMeshProUGUI scoreText;

        [Header("Audio")]
        [SerializeField, Tooltip("Sound to play when clicking menu buttons.")]
        private AudioClip menuClickSound;

        private void Start()
        {
            // Setup buttons
            if (continueButton != null)
            {
                continueButton.onClick.AddListener(HandleContinue);
            }

            if (quitButton != null)
            {
                quitButton.onClick.AddListener(HandleQuit);
            }

            // Display final stats
            DisplayFinalStats();
        }
        
        private void DisplayFinalStats()
        {
            // Get scores and wave from SaveManager
            int deathScore = SaveManager.GetDeathScore(); // Score when player died
            int continueScore = SaveManager.GetCurrentScore(); // Score they'll have when continuing
            int deathWave = SaveManager.GetDeathWave();

            if (waveText != null)
            {
                waveText.text = $"WAVE: {deathWave}";
            }

            if (scoreText != null)
            {
                // Show both scores: what they achieved and what they'll continue with
                scoreText.text = $"SCORE: {deathScore} -> {continueScore}";
            }
        }

        private void HandleContinue()
        {
            PlayMenuSound();
            // Continue from highest wave reached - score is already saved in SaveManager
            SceneManager.LoadScene("Game");
        }

        private void HandleQuit()
        {
            PlayMenuSound();
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
    }
}

