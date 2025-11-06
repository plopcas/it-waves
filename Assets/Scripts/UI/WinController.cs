using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ITWaves.Systems;

namespace ITWaves.UI
{
    public class WinController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField, Tooltip("Credits button - show game credits.")]
        private Button creditsButton;

        [SerializeField, Tooltip("Main menu button - return to main menu.")]
        private Button mainMenuButton;

        [Header("Panels")]
        [SerializeField, Tooltip("Stats panel - shows game statistics.")]
        private GameObject statsPanel;

        [SerializeField, Tooltip("Credits panel - shows game credits.")]
        private GameObject creditsPanel;

        [Header("Statistics Display")]
        [SerializeField, Tooltip("Final score text.")]
        private TextMeshProUGUI finalScoreText;

        [SerializeField, Tooltip("Total deaths text.")]
        private TextMeshProUGUI totalDeathsText;

        [SerializeField, Tooltip("Total playtime text.")]
        private TextMeshProUGUI totalPlaytimeText;

        [SerializeField, Tooltip("Boxes destroyed text.")]
        private TextMeshProUGUI boxesDestroyedText;

        [SerializeField, Tooltip("Crawlers killed text.")]
        private TextMeshProUGUI crawlersKilledText;

        [SerializeField, Tooltip("Skitterers killed text.")]
        private TextMeshProUGUI skitterersKilledText;

        [Header("Credits Display")]
        [SerializeField, Tooltip("Credits text.")]
        private TextMeshProUGUI creditsText;

        [SerializeField, Tooltip("Back button in credits panel.")]
        private Button creditsBackButton;

        [Header("Audio")]
        [SerializeField, Tooltip("Sound to play when clicking menu buttons.")]
        private AudioClip menuClickSound;

        private const string CREDITS_TEXT = @"IT-WAVES

GAME DESIGN & PROGRAMMING
[Your Name]

SPECIAL THANKS
Unity Technologies
Augment Code

Thank you for playing!";

        private void Start()
        {
            // Setup buttons
            if (creditsButton != null)
            {
                creditsButton.onClick.AddListener(HandleShowCredits);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.AddListener(HandleMainMenu);
            }

            if (creditsBackButton != null)
            {
                creditsBackButton.onClick.AddListener(HandleBackToStats);
            }

            // Set credits text
            if (creditsText != null)
            {
                creditsText.text = CREDITS_TEXT;
            }

            // Display stats and show stats panel
            DisplayFinalStats();
            ShowStatsPanel();
        }

        private void DisplayFinalStats()
        {
            // Get final score from SaveManager
            int finalScore = SaveManager.GetCurrentScore();

            // Get statistics from SaveManager
            int totalDeaths = SaveManager.GetTotalDeaths();
            float totalPlaytimeSeconds = SaveManager.GetTotalPlaytime();
            int boxesDestroyed = SaveManager.GetBoxesDestroyed();
            int crawlersKilled = SaveManager.GetCrawlersKilled();
            int skitterersKilled = SaveManager.GetSkitterersKilled();

            // Display statistics
            if (finalScoreText != null)
            {
                finalScoreText.text = $"FINAL SCORE: {finalScore}";
            }

            if (totalDeathsText != null)
            {
                totalDeathsText.text = $"DEATHS: {totalDeaths}";
            }

            if (totalPlaytimeText != null)
            {
                totalPlaytimeText.text = $"TIME: {FormatTime(totalPlaytimeSeconds)}";
            }

            if (boxesDestroyedText != null)
            {
                boxesDestroyedText.text = $"BOXES DESTROYED: {boxesDestroyed}";
            }

            if (crawlersKilledText != null)
            {
                crawlersKilledText.text = $"CRAWLERS KILLED: {crawlersKilled}";
            }

            if (skitterersKilledText != null)
            {
                skitterersKilledText.text = $"SKITTERERS KILLED: {skitterersKilled}";
            }
        }

        private string FormatTime(float seconds)
        {
            int hours = Mathf.FloorToInt(seconds / 3600f);
            int minutes = Mathf.FloorToInt((seconds % 3600f) / 60f);
            int secs = Mathf.FloorToInt(seconds % 60f);

            if (hours > 0)
            {
                return $"{hours:D2}:{minutes:D2}:{secs:D2}";
            }
            else
            {
                return $"{minutes:D2}:{secs:D2}";
            }
        }

        private void HandleShowCredits()
        {
            PlayMenuSound();
            ShowCreditsPanel();
        }

        private void HandleMainMenu()
        {
            PlayMenuSound();
            SceneManager.LoadScene("MainMenu");
        }

        private void HandleBackToStats()
        {
            PlayMenuSound();
            ShowStatsPanel();
        }

        private void ShowStatsPanel()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(true);
            }

            if (creditsPanel != null)
            {
                creditsPanel.SetActive(false);
            }
        }

        private void ShowCreditsPanel()
        {
            if (statsPanel != null)
            {
                statsPanel.SetActive(false);
            }

            if (creditsPanel != null)
            {
                creditsPanel.SetActive(true);
            }
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

