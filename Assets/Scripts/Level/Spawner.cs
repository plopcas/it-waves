using System.Collections.Generic;
using UnityEngine;

namespace ITWaves.Level
{
    /// <summary>
    /// Generic spawner with rate limiting and caps.
    /// </summary>
    public class Spawner : MonoBehaviour
    {
        [Header("Spawn Settings")]
        [SerializeField, Tooltip("Prefabs to spawn.")]
        private List<GameObject> spawnPrefabs = new List<GameObject>();

        [SerializeField, Tooltip("Spawn rate (spawns per second).")]
        private float spawnRate = 1f;

        [SerializeField, Tooltip("Maximum active spawned objects.")]
        private int maxActive = 10;

        [SerializeField, Tooltip("Spawn on edges of arena.")]
        private bool spawnOnEdges = true;

        [Header("References")]
        [SerializeField, Tooltip("Layout generator for spawn positions.")]
        private ProceduralLayoutGenerator layoutGenerator;

        [SerializeField, Tooltip("Level config for arena bounds.")]
        private LevelConfig levelConfig;

        [SerializeField, Tooltip("Difficulty profile for enemy initialization.")]
        private LevelDifficultyProfile difficultyProfile;

        private List<GameObject> activeObjects = new List<GameObject>();
        private float nextSpawnTime;
        private bool isSpawning;
        private int currentLevel = 1;
        
        /// <summary>
        /// Start spawning.
        /// </summary>
        public void StartSpawning()
        {
            isSpawning = true;
            // Add delay before first spawn
            nextSpawnTime = Time.time + (1f / spawnRate);
        }

        /// <summary>
        /// Set the current level for enemy initialization.
        /// </summary>
        public void SetLevel(int level)
        {
            currentLevel = level;
        }
        
        /// <summary>
        /// Stop spawning.
        /// </summary>
        public void StopSpawning()
        {
            isSpawning = false;
        }
        
        /// <summary>
        /// Set spawn rate.
        /// </summary>
        public void SetSpawnRate(float rate)
        {
            spawnRate = rate;
        }
        
        /// <summary>
        /// Set maximum active objects.
        /// </summary>
        public void SetMaxActive(int max)
        {
            maxActive = max;
        }
        
        private void Update()
        {
            if (!isSpawning || spawnPrefabs.Count == 0)
            {
                return;
            }
            
            // Clean up null references
            activeObjects.RemoveAll(obj => obj == null);
            
            // Check if we can spawn
            if (Time.time >= nextSpawnTime && activeObjects.Count < maxActive)
            {
                Spawn();
                nextSpawnTime = Time.time + (1f / spawnRate);
            }
        }
        
        private void Spawn()
        {
            // Choose random prefab
            GameObject prefab = spawnPrefabs[Random.Range(0, spawnPrefabs.Count)];

            // Get spawn position
            Vector3 spawnPos;
            if (spawnOnEdges && layoutGenerator != null && levelConfig != null)
            {
                spawnPos = layoutGenerator.GetRandomEdgeSpawnPosition(levelConfig);
            }
            else
            {
                spawnPos = transform.position;
            }

            // Spawn object
            GameObject obj = Instantiate(prefab, spawnPos, Quaternion.identity);

            if (obj != null)
            {
                // Initialize enemy if it has an EnemyBase component
                var enemy = obj.GetComponent<Enemies.EnemyBase>();
                if (enemy != null && difficultyProfile != null)
                {
                    enemy.Initialise(difficultyProfile, currentLevel);
                    Debug.Log($"[Spawner] Spawned and initialized {obj.name} at {spawnPos}. Active count: {activeObjects.Count + 1}");
                }
                else
                {
                    Debug.Log($"[Spawner] Spawned {obj.name} at {spawnPos} (no EnemyBase component). Active count: {activeObjects.Count + 1}");
                }

                activeObjects.Add(obj);
            }
        }
        
        /// <summary>
        /// Clear all active spawned objects.
        /// </summary>
        public void ClearAll()
        {
            foreach (var obj in activeObjects)
            {
                if (obj != null)
                {
                    Destroy(obj);
                }
            }
            activeObjects.Clear();
        }
    }
}

