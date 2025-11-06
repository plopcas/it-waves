using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using ITWaves.Systems;
using System.Collections;
using UnityEngine.InputSystem;

namespace ITWaves.UI
{
    public class WinController : MonoBehaviour
    {
        [Header("Buttons")]
        [SerializeField, Tooltip("Return to title button - return to boot scene after credits.")]
        private Button returnToTitleButton;

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
        [SerializeField, Tooltip("Credits scroll container - parent that moves up.")]
        private RectTransform creditsScrollContainer;

        [SerializeField, Tooltip("Thank you text.")]
        private TextMeshProUGUI thankYouText;

        [SerializeField, Tooltip("Game by text.")]
        private TextMeshProUGUI gameByText;

        [SerializeField, Tooltip("Canvas group for fade out effect.")]
        private CanvasGroup creditsCanvasGroup;

        [Header("Timing")]
        [SerializeField, Tooltip("Time to show stats before auto-showing credits.")]
        private float statsDisplayTime = 5f;

        [SerializeField, Tooltip("Time to pause on each credit section.")]
        private float creditsPauseTime = 2.5f;

        [SerializeField, Tooltip("Speed of credits scrolling.")]
        private float scrollSpeed = 100f;

        [SerializeField, Tooltip("Speed multiplier when shoot button is held.")]
        private float fastScrollMultiplier = 3f;

        [SerializeField, Tooltip("Fade out duration.")]
        private float fadeOutDuration = 1f;

        [Header("Audio")]
        [SerializeField, Tooltip("Sound to play when clicking menu buttons.")]
        private AudioClip menuClickSound;

        private bool creditsSequenceRunning = false;
        private bool isSkipping = false;

        private void Start()
        {
            // Setup buttons
            if (returnToTitleButton != null)
            {
                returnToTitleButton.onClick.AddListener(HandleReturnToTitle);
                returnToTitleButton.gameObject.SetActive(false); // Hidden initially
            }

            // Set credits text
            if (thankYouText != null)
            {
                thankYouText.text = "THANK YOU FOR PLAYING";
            }

            if (gameByText != null)
            {
                gameByText.text = "A GAME BY\nPEDRO LOPEZ";
            }

            // Display stats and show stats panel
            DisplayFinalStats();
            ShowStatsPanel();

            // Start automatic credits sequence after delay
            StartCoroutine(AutoShowCreditsSequence());
        }

        private void Update()
        {
            // Check for shoot button input to speed up credits
            if (creditsSequenceRunning)
            {
                // Check for shoot input using new Input System (left mouse button or space)
                bool isMousePressed = Mouse.current != null && Mouse.current.leftButton.isPressed;
                bool isSpacePressed = Keyboard.current != null && Keyboard.current.spaceKey.isPressed;

                if (isMousePressed || isSpacePressed)
                {
                    isSkipping = true;
                }
                else
                {
                    isSkipping = false;
                }
            }
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

        private IEnumerator AutoShowCreditsSequence()
        {
            // Wait for stats display time
            yield return new WaitForSeconds(statsDisplayTime);

            // Hide stats panel and show credits panel
            ShowCreditsPanel();

            // Start credits sequence
            yield return StartCoroutine(PlayCreditsSequence());
        }

        private IEnumerator PlayCreditsSequence()
        {
            creditsSequenceRunning = true;

            // Make sure all credits are initially visible
            if (creditsCanvasGroup != null)
            {
                creditsCanvasGroup.alpha = 1f;
            }

            // Position credits at starting position
            if (creditsScrollContainer != null)
            {
                Vector2 startPos = creditsScrollContainer.anchoredPosition;
                float currentY = startPos.y;

                // Show "THANK YOU FOR PLAYING" - pause first, then scroll
                if (thankYouText != null)
                {
                    yield return new WaitForSeconds(GetPauseTime()); // Pause on first text
                    float targetY = currentY + GetScrollDistanceToCenter(thankYouText.rectTransform);
                    yield return StartCoroutine(ScrollToPosition(currentY, targetY));
                    currentY = targetY;
                }

                // Show "A GAME BY PEDRO LOPEZ" - scroll to center then pause
                if (gameByText != null)
                {
                    float targetY = currentY + GetScrollDistanceToCenter(gameByText.rectTransform);
                    yield return StartCoroutine(ScrollToPosition(currentY, targetY));
                    currentY = targetY;
                    yield return new WaitForSeconds(GetPauseTime());
                }
            }

            // Fade out credits
            if (creditsCanvasGroup != null)
            {
                float elapsed = 0f;
                while (elapsed < fadeOutDuration)
                {
                    elapsed += Time.deltaTime * (isSkipping ? fastScrollMultiplier : 1f);
                    creditsCanvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeOutDuration);
                    yield return null;
                }
                creditsCanvasGroup.alpha = 0f;
            }

            // Show "Return to Title" button
            if (returnToTitleButton != null)
            {
                returnToTitleButton.gameObject.SetActive(true);
            }

            creditsSequenceRunning = false;
        }

        private IEnumerator ScrollToPosition(float startY, float targetY)
        {
            if (creditsScrollContainer == null) yield break;

            float distance = targetY - startY;
            float duration = Mathf.Abs(distance) / scrollSpeed;
            float elapsed = 0f;

            while (elapsed < duration)
            {
                float speedMultiplier = isSkipping ? fastScrollMultiplier : 1f;
                elapsed += Time.deltaTime * speedMultiplier;
                float t = Mathf.Clamp01(elapsed / duration);
                float currentY = Mathf.Lerp(startY, targetY, t);
                creditsScrollContainer.anchoredPosition = new Vector2(creditsScrollContainer.anchoredPosition.x, currentY);
                yield return null;
            }

            creditsScrollContainer.anchoredPosition = new Vector2(creditsScrollContainer.anchoredPosition.x, targetY);
        }

        private float GetScrollDistanceToCenter(RectTransform textRect)
        {
            // Calculate distance needed to scroll to center the text on screen
            // This is a simplified version - adjust based on your UI layout
            return 300f; // Adjust this value based on spacing between credits
        }

        private float GetPauseTime()
        {
            // Return reduced pause time if skipping
            return isSkipping ? creditsPauseTime / fastScrollMultiplier : creditsPauseTime;
        }

        private void HandleReturnToTitle()
        {
            PlayMenuSound();
            SceneManager.LoadScene("Boot");
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

