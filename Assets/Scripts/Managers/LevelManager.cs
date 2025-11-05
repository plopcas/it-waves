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

        [Header("Configuration")]
        [SerializeField, Tooltip("Level configuration.")]
        private LevelConfig levelConfig;

        [SerializeField, Tooltip("Difficulty profile (scales with waves).")]
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
        [SerializeField, Tooltip("Is game currently active.")]
        private bool isLevelActive;

        [SerializeField, Tooltip("Current wave number (starts at 1).")]
        private int currentWave = 1;

        private GameObject currentPlayer;
        private SnakeController currentSnake;

        public bool IsLevelActive => isLevelActive;
        public int CurrentWave => currentWave;

        // Events
        public event Action OnGameStarted;
        public event Action OnPlayerDied;
        public event Action<int> OnWaveStarted; // (wave)

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

                StartGame();
            }
        }

        public void StartGame()
        {
            currentWave = 1;
            isLevelActive = true;

            // Calculate difficulty based on wave
            int difficulty = GetWaveDifficulty();

            // Generate layout
            if (layoutGenerator != null && levelConfig != null && difficultyProfile != null)
            {
                layoutGenerator.Generate(levelConfig, difficultyProfile, difficulty);
            }

            // Spawn player at centre
            SpawnPlayer();

            // Spawn snake
            SpawnSnake();

            // Start enemy spawning
            if (enemySpawner != null && difficultyProfile != null)
            {
                enemySpawner.SetLevel(difficulty);
                enemySpawner.SetSpawnRate(difficultyProfile.GetEnemySpawnRate(difficulty));
                enemySpawner.SetMaxActive(difficultyProfile.GetEnemyCap(difficulty));
                enemySpawner.StartSpawning();
            }

            OnGameStarted?.Invoke();
        }

        /// <summary>
        /// Calculate difficulty level based on current wave.
        /// Every 2 waves increases difficulty by 1 level.
        /// </summary>
        private int GetWaveDifficulty()
        {
            // Start at difficulty 1, increase by 1 every 2 waves
            int difficulty = 1 + (currentWave - 1) / 2;

            // Cap at level 20 to stay within difficulty curve bounds
            return Mathf.Clamp(difficulty, 1, 20);
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

            // Calculate difficulty for snake speed
            int difficulty = GetWaveDifficulty();

            // Calculate segment count: base segments + (wave - 1) additional segments
            int baseSegments = difficultyProfile.GetSnakeSegments(1); // Always use base from difficulty 1
            int segmentCount = baseSegments + (currentWave - 1);

            // Use wave difficulty for snake speed, segment count for length
            currentSnake = snakeSpawner.SpawnWithSegmentCount(spawnPos, difficulty, segmentCount);

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

            // Check if player has reached a high wave milestone (e.g., wave 20)
            if (currentWave >= 20)
            {
                // Victory!
                SaveManager.UpdateHighestLevel(currentWave);
                Invoke(nameof(LoadWinScene), 2f);
            }
            else
            {
                // Snake defeated means wave complete - start next wave
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

            // Calculate difficulty based on new wave
            int difficulty = GetWaveDifficulty();

            // Regenerate layout with increased difficulty (boxes will be in different positions)
            if (layoutGenerator != null && levelConfig != null && difficultyProfile != null)
            {
                layoutGenerator.Generate(levelConfig, difficultyProfile, difficulty);
            }

            // Spawn new snake with more segments
            SpawnSnake();

            // Restart enemy spawning with increased difficulty
            if (enemySpawner != null && difficultyProfile != null)
            {
                enemySpawner.SetLevel(difficulty);
                enemySpawner.SetSpawnRate(difficultyProfile.GetEnemySpawnRate(difficulty));
                enemySpawner.SetMaxActive(difficultyProfile.GetEnemyCap(difficulty));
                enemySpawner.StartSpawning();
            }

            // Notify listeners that a new wave has started
            OnWaveStarted?.Invoke(currentWave);
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
