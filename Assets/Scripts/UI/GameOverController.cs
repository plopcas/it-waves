using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ITWaves.Systems;

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
            // Get final score and wave from PlayerPrefs (saved by GameManager)
            int finalScore = PlayerPrefs.GetInt("FinalScore", 0);
            int deathWave = PlayerPrefs.GetInt("DeathWave", 1);
            
            if (waveText != null)
            {
                waveText.text = $"WAVE: {deathWave}";
            }
            
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {finalScore}";
            }
        }
        
        private void HandleContinue()
        {
            // Continue from highest wave reached
            int highestWave = SaveManager.GetHighestWaveReached();
            PlayerPrefs.SetInt("StartWave", highestWave);
            SceneManager.LoadScene("Game");
        }
        
        private void HandleQuit()
        {
            // Return to main menu
            SceneManager.LoadScene("MainMenu");
        }
    }
}

