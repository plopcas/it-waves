using UnityEngine;
using UnityEngine.SceneManagement;
using ITWaves.Level;
using ITWaves.Systems;
using ITWaves.Snake;
using ITWaves.Core;
using System;

namespace ITWaves
{
    public class LevelManager : MonoBehaviour
    {
        public static LevelManager Instance { get; private set; }

        [Header("Level Configuration")]
        [SerializeField, Tooltip("Current level index (1-20).")]
        private int currentLevel = 1;

        [SerializeField, Tooltip("Level configuration.")]
        private LevelConfig levelConfig;

        [SerializeField, Tooltip("Difficulty profile for all levels.")]
        private LevelDifficultyProfile difficultyProfile;

        [Header("References")]
        [SerializeField, Tooltip("Procedural layout generator.")]
        private ProceduralLayoutGenerator layoutGenerator;

        [SerializeField, Tooltip("Enemy spawner.")]
        private Spawner enemySpawner;

        [SerializeField, Tooltip("Snake spawner.")]
        private SnakeSpawner snakeSpawner;

        [SerializeField, Tooltip("Player prefab.")]
        private GameObject playerPrefab;

        [Header("State")]
        [SerializeField, Tooltip("Is level currently active.")]
        private bool isLevelActive;

        [SerializeField, Tooltip("Current wave number within the level.")]
        private int currentWave = 1;

        private GameObject currentPlayer;
        private SnakeController currentSnake;

        public int CurrentLevel => currentLevel;
        public bool IsLevelActive => isLevelActive;
        public int CurrentWave => currentWave;

        // Events
        public event Action<int> OnLevelStarted;
        public event Action OnPlayerDied;
        public event Action<int, int> OnWaveStarted; // (level, wave)

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            // Auto-start if in Game scene
            if (SceneManager.GetActiveScene().name == "Game")
            {
                // Ensure GridManager is initialized before starting
                if (Core.GridManager.Instance == null)
                {
                    Debug.LogError("GridManager not found in scene! Please add GridManager component to the scene.");
                    return;
                }

                StartRun(currentLevel);
            }
        }

        public void StartRun(int levelIndex)
        {
            currentLevel = Mathf.Clamp(levelIndex, 1, 20);
            currentWave = 1; // Reset wave counter when starting a new level
            isLevelActive = true;

            // Generate layout
            if (layoutGenerator != null && levelConfig != null && difficultyProfile != null)
            {
                layoutGenerator.Generate(levelConfig, difficultyProfile, currentLevel);
            }

            // Spawn player at centre
            SpawnPlayer();

            // Spawn snake
            SpawnSnake();

            // Start enemy spawning
            if (enemySpawner != null && difficultyProfile != null)
            {
                enemySpawner.SetLevel(currentLevel);
                enemySpawner.SetSpawnRate(difficultyProfile.GetEnemySpawnRate(currentLevel));
                enemySpawner.SetMaxActive(difficultyProfile.GetEnemyCap(currentLevel));
                enemySpawner.StartSpawning();
            }

            OnLevelStarted?.Invoke(currentLevel);
        }

        private void SpawnPlayer()
        {
            if (playerPrefab == null)
            {
                Debug.LogError("Player prefab not assigned!");
                return;
            }

            // Spawn at centre
            currentPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);

            // Subscribe to player death
            var playerHealth = currentPlayer.GetComponent<Player.PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.OnDied += HandlePlayerDied;
            }
        }

        private void SpawnSnake()
        {
            if (snakeSpawner == null || difficultyProfile == null || levelConfig == null)
            {
                Debug.LogError("Snake spawner, difficulty profile, or level config not assigned!");
                return;
            }

            // Spawn away from player
            Vector2 spawnPos = layoutGenerator != null
                ? layoutGenerator.GetRandomEdgeSpawnPosition(levelConfig)
                : new Vector2(10f, 10f);

            // Calculate segment count: base segments for level + (wave - 1) additional segments
            int baseSegments = difficultyProfile.GetSnakeSegments(currentLevel);
            int segmentCount = baseSegments + (currentWave - 1);

            currentSnake = snakeSpawner.SpawnWithSegmentCount(spawnPos, currentLevel, segmentCount);

            // Snake now uses segment-based damage system
            // No need to subscribe to health events - snake handles its own lifecycle
        }

        public void HandleSnakeEscaped()
        {
            isLevelActive = false;

            // Clear enemies
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
                enemySpawner.ClearAll();
            }

            // Clear boxes
            if (layoutGenerator != null)
            {
                layoutGenerator.ClearLayout();
            }

            // Increment wave counter
            currentWave++;

            // Restart the wave with increased difficulty
            Invoke(nameof(RestartWave), 2f);
        }

        public void HandleSnakeDefeated()
        {
            isLevelActive = false;

            // Stop spawning and clear enemies
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
                enemySpawner.ClearAll();
            }

            // Clear boxes
            if (layoutGenerator != null)
            {
                layoutGenerator.ClearLayout();
            }

            if (currentLevel >= 20)
            {
                // Victory!
                SaveManager.UpdateHighestLevel(20);
                Invoke(nameof(LoadWinScene), 2f);
            }
            else
            {
                // Shouldn't happen, but treat as escape
                HandleSnakeEscaped();
            }
        }

        public void HandlePlayerDied()
        {
            isLevelActive = false;

            // Stop spawning and clear enemies
            if (enemySpawner != null)
            {
                enemySpawner.StopSpawning();
                enemySpawner.ClearAll();
            }

            // Clear boxes
            if (layoutGenerator != null)
            {
                layoutGenerator.ClearLayout();
            }

            // Save final score (optional - requires GameManager in scene)
            // var gameManager = FindFirstObjectByType<ITWaves.Core.GameManager>();
            // if (gameManager != null)
            // {
            //     gameManager.SaveFinalScore();
            // }

            OnPlayerDied?.Invoke();

            // Load game over scene
            Invoke(nameof(LoadGameOverScene), 2f);
        }

        private void RestartWave()
        {
            isLevelActive = true;

            // Regenerate layout
            if (layoutGenerator != null && levelConfig != null && difficultyProfile != null)
            {
                layoutGenerator.Generate(levelConfig, difficultyProfile, currentLevel);
            }

            // Spawn new snake with more segments
            SpawnSnake();

            // Restart enemy spawning
            if (enemySpawner != null && difficultyProfile != null)
            {
                enemySpawner.SetLevel(currentLevel);
                enemySpawner.SetSpawnRate(difficultyProfile.GetEnemySpawnRate(currentLevel));
                enemySpawner.SetMaxActive(difficultyProfile.GetEnemyCap(currentLevel));
                enemySpawner.StartSpawning();
            }

            // Notify listeners that a new wave has started
            OnWaveStarted?.Invoke(currentLevel, currentWave);
        }

        private void LoadWinScene()
        {
            SceneManager.LoadScene("Win");
        }

        private void LoadGameOverScene()
        {
            SceneManager.LoadScene("GameOver");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }
    }
}
