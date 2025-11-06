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

        [Header("Audio")]
        [SerializeField, Tooltip("Sound played when wave is completed.")]
        private AudioClip waveWinSound;

        [SerializeField, Range(0f, 1f), Tooltip("Volume of the wave win sound.")]
        private float waveWinVolume = 1f;

        [SerializeField, Tooltip("Sound played when player dies.")]
        private AudioClip waveFailSound;

        [SerializeField, Range(0f, 1f), Tooltip("Volume of the wave fail sound.")]
        private float waveFailVolume = 1f;

        [Header("State")]
        [SerializeField, Tooltip("Is game currently active.")]
        private bool isLevelActive;

        [SerializeField, Tooltip("Current wave number (starts at 1).")]
        private int currentWave = 1;

        private GameObject currentPlayer;
        private SnakeController currentSnake;
        private bool snakeHasEscaped = false; // Track if snake has completed retreat
        private AudioSource audioSource;
        private float waveStartTime; // Track when current wave started

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

            // Get or add AudioSource component
            audioSource = GetComponent<AudioSource>();
            if (audioSource == null)
            {
                audioSource = gameObject.AddComponent<AudioSource>();
            }
            audioSource.playOnAwake = false;
        }

        private void Start()
        {
            Debug.Log($"LevelManager Start() called. Scene: {SceneManager.GetActiveScene().name}");

            // Auto-start if in Game scene
            if (SceneManager.GetActiveScene().name == "Game")
            {
                // Ensure GridManager is initialized before starting
                if (Core.GridManager.Instance == null)
                {
                    Debug.LogError("GridManager not found in scene! Please add GridManager component to the scene.");
                    return;
                }

                Debug.Log("GridManager found, starting from highest wave");

                // Start from highest wave reached
                int startWave = SaveManager.GetHighestWaveReached();
                Debug.Log($"Starting game at wave {startWave}");
                StartGame(startWave);
            }
        }

        public void StartGame(int startingWave = 1)
        {
            currentWave = startingWave;
            isLevelActive = true;

            // Calculate difficulty based on wave
            int difficulty = GetWaveDifficulty();

            // Generate layout
            if (layoutGenerator != null && levelConfig != null && difficultyProfile != null)
            {
                layoutGenerator.Generate(levelConfig, difficultyProfile, difficulty, currentWave);
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
            OnWaveStarted?.Invoke(currentWave);
        }

        // Calculate difficulty level based on current wave.
        // Every 2 waves increases difficulty by 1 level.
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
            snakeHasEscaped = true; // Mark that snake has escaped

            // Add wave time to total playtime
            float waveTime = Time.time - waveStartTime;
            SaveManager.AddPlaytime(waveTime);

            // Play wave win sound
            if (audioSource != null && waveWinSound != null)
            {
                float sfxVolume = Systems.SaveManager.GetSFXVolume();
                audioSource.PlayOneShot(waveWinSound, waveWinVolume * sfxVolume);
            }

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

            // Save progress - player completed previous wave and is now on this wave
            Debug.Log($"Wave completed! Saving progress: Wave {currentWave}");
            SaveManager.UpdateHighestWave(currentWave);

            // Save current score (player completed the wave with this score)
            if (Core.GameManager.Instance != null)
            {
                int currentScore = Core.GameManager.Instance.CurrentScore;
                SaveManager.SaveScore(currentScore, currentWave);
                Debug.Log($"Wave completed - saved score: {currentScore}");
            }

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

            // Check if player has reached wave 20 (final wave with mega head boss)
            if (currentWave == 20)
            {
                // Victory! Player defeated the mega head on wave 20
                Debug.Log("Wave 20 mega head defeated - VICTORY!");

                // Play wave win sound for final victory
                if (audioSource != null && waveWinSound != null)
                {
                    float sfxVolume = Systems.SaveManager.GetSFXVolume();
                    audioSource.PlayOneShot(waveWinSound, waveWinVolume * sfxVolume);
                }

                SaveManager.UpdateHighestWave(currentWave);
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

            // Add wave time to total playtime (even on death)
            float waveTime = Time.time - waveStartTime;
            SaveManager.AddPlaytime(waveTime);

            // Increment death counter
            SaveManager.IncrementDeaths();

            // Play wave fail sound
            if (audioSource != null && waveFailSound != null)
            {
                float sfxVolume = Systems.SaveManager.GetSFXVolume();
                audioSource.PlayOneShot(waveFailSound, waveFailVolume * sfxVolume);
            }

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

            // Check if snake has escaped - if not, player died during retreat
            // In this case, we should NOT increment the wave (restart same wave)
            if (snakeHasEscaped)
            {
                // Snake escaped, so wave was completed - player died after wave completion
                // This shouldn't normally happen, but if it does, treat as normal death
                Debug.Log($"Player died on wave {currentWave} after snake escaped");
            }
            else
            {
                // Snake hasn't escaped yet - player died during the wave
                // Don't increment wave, restart same wave
                Debug.Log($"Player died on wave {currentWave} before snake escaped - will restart same wave");
            }

            // Save progress - player reached this wave (even though they died on it)
            SaveManager.UpdateHighestWave(currentWave);

            // Save death score for GameOver display (but continue score is already saved from wave start)
            if (Core.GameManager.Instance != null)
            {
                int deathScore = Core.GameManager.Instance.CurrentScore;
                SaveManager.SaveDeathScore(deathScore);
                Debug.Log($"Player died with score: {deathScore}");
            }

            OnPlayerDied?.Invoke();

            // Load game over scene
            Invoke(nameof(LoadGameOverScene), 2f);
        }

        private void RestartWave()
        {
            isLevelActive = true;
            snakeHasEscaped = false; // Reset flag for new wave

            // Track wave start time for playtime statistics
            waveStartTime = Time.time;

            // Save current score at wave start (so we can restore it if player dies)
            if (Core.GameManager.Instance != null)
            {
                int currentScore = Core.GameManager.Instance.CurrentScore;
                SaveManager.SaveScore(currentScore, currentWave);
                Debug.Log($"Wave {currentWave} started - saved score: {currentScore}");
            }

            // Calculate difficulty based on new wave
            int difficulty = GetWaveDifficulty();

            // Regenerate layout with increased difficulty (boxes will be in different positions)
            if (layoutGenerator != null && levelConfig != null && difficultyProfile != null)
            {
                layoutGenerator.Generate(levelConfig, difficultyProfile, difficulty, currentWave);
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
