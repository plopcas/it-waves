using System;
using UnityEngine;
using ITWaves.Systems;

namespace ITWaves.Core
{
    public class GameManager : MonoBehaviour
    {
        private static GameManager instance;
        public static GameManager Instance => instance;
        
        [Header("Game State")]
        [SerializeField, Tooltip("Current score.")]
        private int currentScore;

        [SerializeField, Tooltip("Current wave.")]
        private int currentWave = 1;

        // Events
        public event Action<int> OnScoreChanged;
        public event Action<int> OnWaveChanged;

        public int CurrentScore => currentScore;
        public int CurrentWave => currentWave;
        
        private void Awake()
        {
            // Simple singleton - no DontDestroyOnLoad, gets recreated with each Game scene
            if (instance != null && instance != this)
            {
                Destroy(this);
                return;
            }

            instance = this;

            // Restore score from SaveManager
            currentScore = SaveManager.GetCurrentScore();
            if (currentScore > 0)
            {
                OnScoreChanged?.Invoke(currentScore);
            }
        }
        
        private void Start()
        {
            // Subscribe to level manager events
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnGameStarted += HandleGameStarted;
                LevelManager.Instance.OnWaveStarted += HandleWaveStarted;
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

        public void SetWave(int wave)
        {
            currentWave = wave;
            OnWaveChanged?.Invoke(currentWave);
        }

        public void ResetGameState()
        {
            currentScore = 0;
            currentWave = 1;
            OnScoreChanged?.Invoke(currentScore);
            OnWaveChanged?.Invoke(currentWave);
        }

        private void HandleGameStarted()
        {
            SetWave(1);
        }

        private void HandleWaveStarted(int waveNumber)
        {
            SetWave(waveNumber);
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnGameStarted -= HandleGameStarted;
                LevelManager.Instance.OnWaveStarted -= HandleWaveStarted;
            }
        }
    }
}

