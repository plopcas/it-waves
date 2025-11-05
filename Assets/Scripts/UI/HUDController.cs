using UnityEngine;
using TMPro;

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

        private int currentScore;

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

