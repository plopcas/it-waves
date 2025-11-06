using UnityEngine;
using TMPro;
using System.Collections;

namespace ITWaves.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Wave Display")]
        [SerializeField, Tooltip("Wave number text.")]
        private TextMeshProUGUI waveText;

        [Header("Score Display")]
        [SerializeField, Tooltip("Score text.")]
        private TextMeshProUGUI scoreText;

        [Header("Message Display")]
        [SerializeField, Tooltip("Message bar panel (parent of message text).")]
        private GameObject messageBar;

        [SerializeField, Tooltip("Center message text (for power-ups, etc).")]
        private TextMeshProUGUI messageText;

        [SerializeField, Tooltip("Message display duration.")]
        private float messageDuration = 1f;

        [Header("Upgrade Display")]
        [SerializeField, Tooltip("Upgrade indicator text (bottom left).")]
        private TextMeshProUGUI upgradeText;

        private int currentScore;
        private Coroutine messageCoroutine;

        private void Start()
        {
            // Subscribe to events
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnGameStarted += HandleGameStarted;
                LevelManager.Instance.OnWaveStarted += HandleWaveStarted;
            }

            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnScoreChanged += SetScore;
                // Initialize display with current values from GameManager
                SetWave(Core.GameManager.Instance.CurrentWave);
                SetScore(Core.GameManager.Instance.CurrentScore);
            }
            else
            {
                SetWave(1);
                SetScore(0);
            }

            // Hide message bar initially
            if (messageBar != null)
            {
                messageBar.SetActive(false);
            }

            // Update upgrade display
            UpdateUpgradeDisplay();
        }

        public void SetWave(int waveNumber)
        {
            if (waveText != null)
            {
                waveText.text = $"WAVE: {waveNumber}";
            }
        }

        public void SetScore(int value)
        {
            currentScore = value;
            if (scoreText != null)
            {
                scoreText.text = $"SCORE: {currentScore}";
            }
        }

        public void AddScore(int value)
        {
            SetScore(currentScore + value);
        }

        private void HandleGameStarted()
        {
            // Don't reset score here - it's managed by GameManager
            // Wave is set by HandleWaveStarted event
        }

        private void HandleWaveStarted(int waveNumber)
        {
            SetWave(waveNumber);
        }

        // Display a temporary message in the center of the screen.
        public void ShowMessage(string message)
        {
            if (messageText == null) return;

            // Stop any existing message
            if (messageCoroutine != null)
            {
                StopCoroutine(messageCoroutine);
            }

            messageCoroutine = StartCoroutine(ShowMessageRoutine(message));
        }

        private IEnumerator ShowMessageRoutine(string message)
        {
            // Show the message bar panel
            if (messageBar != null)
            {
                messageBar.SetActive(true);
            }

            // Set the message text
            if (messageText != null)
            {
                messageText.text = message;
            }

            // Use WaitForSecondsRealtime so it works even when game is paused
            yield return new WaitForSecondsRealtime(messageDuration);

            // Hide the message bar panel
            if (messageBar != null)
            {
                messageBar.SetActive(false);
            }

            messageCoroutine = null;

            // Update upgrade display after treasure collection message
            UpdateUpgradeDisplay();
        }

        public void UpdateUpgradeDisplay()
        {
            if (upgradeText == null) return;

            int highestTreasure = Systems.SaveManager.GetHighestTreasureCollected();

            if (highestTreasure == 0)
            {
                upgradeText.text = "";
            }
            else
            {
                string boxes = "";
                for (int i = 0; i < highestTreasure; i++)
                {
                    boxes += "■ ";
                }

                upgradeText.text = $"UPGRADES: {boxes.TrimEnd()}";
            }
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnGameStarted -= HandleGameStarted;
                LevelManager.Instance.OnWaveStarted -= HandleWaveStarted;
            }

            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.OnScoreChanged -= SetScore;
            }
        }
    }
}

