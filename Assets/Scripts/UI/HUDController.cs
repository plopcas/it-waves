using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace ITWaves.UI
{
    public class HUDController : MonoBehaviour
    {
        [Header("Level Display")]
        [SerializeField, Tooltip("Level text.")]
        private TextMeshProUGUI levelText;
        
        [Header("Player Health")]
        [SerializeField, Tooltip("Player health text.")]
        private TextMeshProUGUI playerHealthText;
        
        [SerializeField, Tooltip("Player health bar fill.")]
        private Image playerHealthBar;
        
        [Header("Snake Health")]
        [SerializeField, Tooltip("Snake health text.")]
        private TextMeshProUGUI snakeHealthText;
        
        [SerializeField, Tooltip("Snake health bar fill.")]
        private Image snakeHealthBar;
        
        [Header("Score")]
        [SerializeField, Tooltip("Score text.")]
        private TextMeshProUGUI scoreText;
        
        private int currentScore;
        
        private void Start()
        {
            // Subscribe to events
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelStarted += HandleLevelStarted;
                LevelManager.Instance.OnWaveStarted += HandleWaveStarted;
            }

            // Find player and snake
            FindAndSubscribeToPlayer();
            FindAndSubscribeToSnake();
        }
        
        private void FindAndSubscribeToPlayer()
        {
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                var playerHealth = playerObj.GetComponent<Player.PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.OnHealthChanged += HandlePlayerHealthChanged;
                    HandlePlayerHealthChanged(playerHealth.CurrentHealth, playerHealth.MaxHealth);
                }
            }
        }
        
        private void FindAndSubscribeToSnake()
        {
            var snakeObj = GameObject.FindGameObjectWithTag("Snake");
            if (snakeObj != null)
            {
                var snakeController = snakeObj.GetComponent<Snake.SnakeController>();
                if (snakeController != null)
                {
                    // Display initial segment count
                    SetSnakeHP(snakeController.SegmentCount, snakeController.SegmentCount);
                }
            }
        }
        
        public void SetLevel(int levelIndex)
        {
            if (levelText != null)
            {
                // Check if we're in a wave-based level
                if (LevelManager.Instance != null && LevelManager.Instance.CurrentWave > 1)
                {
                    levelText.text = $"Level {levelIndex} - Wave {LevelManager.Instance.CurrentWave}";
                }
                else
                {
                    levelText.text = $"Level {levelIndex}";
                }
            }
        }
        
        public void SetPlayerHP(int current, int max)
        {
            if (playerHealthText != null)
            {
                playerHealthText.text = $"HP: {current}/{max}";
            }
            
            if (playerHealthBar != null)
            {
                playerHealthBar.fillAmount = max > 0 ? (float)current / max : 0f;
            }
        }
        
        public void SetSnakeHP(int current, int max)
        {
            if (snakeHealthText != null)
            {
                snakeHealthText.text = $"Snake: {current} segments";
            }

            if (snakeHealthBar != null)
            {
                snakeHealthBar.fillAmount = max > 0 ? (float)current / max : 0f;
            }
        }
        
        public void SetScore(int value)
        {
            currentScore = value;
            if (scoreText != null)
            {
                scoreText.text = $"Score: {currentScore}";
            }
        }
        
        public void AddScore(int value)
        {
            SetScore(currentScore + value);
        }
        
        private void HandleLevelStarted(int levelIndex)
        {
            SetLevel(levelIndex);

            // Re-find player and snake for new level
            Invoke(nameof(FindAndSubscribeToPlayer), 0.1f);
            Invoke(nameof(FindAndSubscribeToSnake), 0.1f);
        }

        private void HandleWaveStarted(int levelIndex, int waveNumber)
        {
            SetLevel(levelIndex);

            // Re-find snake for new wave
            Invoke(nameof(FindAndSubscribeToSnake), 0.1f);
        }

        private void HandlePlayerHealthChanged(int current, int max)
        {
            SetPlayerHP(current, max);
        }

        private void OnDestroy()
        {
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelStarted -= HandleLevelStarted;
                LevelManager.Instance.OnWaveStarted -= HandleWaveStarted;
            }
        }
    }
}

