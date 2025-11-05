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
        [SerializeField, Tooltip("Center message text (for power-ups, etc).")]
        private TextMeshProUGUI messageText;

        [SerializeField, Tooltip("Message display duration.")]
        private float messageDuration = 1f;

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

            // Initialize display
            SetWave(1);
            SetScore(0);

            // Hide message text initially
            if (messageText != null)
            {
                messageText.gameObject.SetActive(false);
            }
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
            SetWave(1);
            SetScore(0);
        }

        private void HandleWaveStarted(int waveNumber)
        {
            SetWave(waveNumber);
        }

        /// <summary>
        /// Display a temporary message in the center of the screen.
        /// </summary>
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
            messageText.text = message;
            messageText.gameObject.SetActive(true);

            // Use WaitForSecondsRealtime so it works even when game is paused
            yield return new WaitForSecondsRealtime(messageDuration);

            messageText.gameObject.SetActive(false);
            messageCoroutine = null;
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnGameStarted -= HandleGameStarted;
                LevelManager.Instance.OnWaveStarted -= HandleWaveStarted;
            }
        }
    }
}

