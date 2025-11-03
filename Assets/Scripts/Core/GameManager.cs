using System;
using UnityEngine;

namespace ITWaves.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance => instance;
        
        [Header("Game State")]
        [SerializeField, Tooltip("Current score.")]
        private int currentScore;
        
        [SerializeField, Tooltip("Current level.")]
        private int currentLevel = 1;
        
        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnLevelChanged;
        
        public int CurrentScore => currentScore;
        public int CurrentLevel => currentLevel;
        
        private void Awake()
        {
            // Singleton pattern
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        private void Start()
        {
            // Subscribe to level manager events
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelStarted += HandleLevelStarted;
            }
        }

        public void AddScore(int amount)
        {
            currentScore += amount;
            OnScoreChanged?.Invoke(currentScore);
            
            // Update HUD if available
            var hud = FindFirstObjectByType<UI.HUDController>();
            if (hud != null)
            {
                hud.SetScore(currentScore);
            }
        }

        public void ResetScore()
        {
            currentScore = 0;
            OnScoreChanged?.Invoke(currentScore);
        }

        public void SetLevel(int level)
        {
            currentLevel = level;
            OnLevelChanged?.Invoke(currentLevel);
        }

        public void ResetGameState()
        {
            currentScore = 0;
            currentLevel = 1;
            OnScoreChanged?.Invoke(currentScore);
            OnLevelChanged?.Invoke(currentLevel);
        }

        public void SaveFinalScore()
        {
            PlayerPrefs.SetInt("FinalScore", currentScore);
            PlayerPrefs.SetInt("DeathLevel", currentLevel);
            PlayerPrefs.Save();
        }
        
        private void HandleLevelStarted(int levelIndex)
        {
            SetLevel(levelIndex);
        }
        
        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
            
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnLevelStarted -= HandleLevelStarted;
            }
        }
    }
}

