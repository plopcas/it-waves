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
                LevelManager.Instance.OnGameStarted += HandleGameStarted;
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
        
        public void SetWave(int waveNumber)
        {
            if (levelText != null)
            {
                levelText.text = $"Wave {waveNumber}";
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
        
        private void HandleGameStarted()
        {
            SetWave(1);

            // Re-find player and snake for new game
            Invoke(nameof(FindAndSubscribeToPlayer), 0.1f);
            Invoke(nameof(FindAndSubscribeToSnake), 0.1f);
        }

        private void HandleWaveStarted(int waveNumber)
        {
            SetWave(waveNumber);

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
                LevelManager.Instance.OnGameStarted -= HandleGameStarted;
                LevelManager.Instance.OnWaveStarted -= HandleWaveStarted;
            }
        }
    }
}

