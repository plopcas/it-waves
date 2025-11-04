using System.Collections.Generic;
using UnityEngine;
using ITWaves.Core;
using ITWaves.Snake;

namespace ITWaves.Level
{
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
        
        public void StartSpawning()
        {
            isSpawning = true;
            // Add delay before first spawn
            nextSpawnTime = Time.time + (1f / spawnRate);
        }

        public void SetLevel(int level)
        {
            currentLevel = level;
        }
        
        public void StopSpawning()
        {
            isSpawning = false;
        }
        
        public void SetSpawnRate(float rate)
        {
            spawnRate = rate;
        }
        
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

            // Get spawn position (avoiding snake-occupied cells)
            Vector3 spawnPos;
            if (spawnOnEdges && GridManager.Instance != null)
            {
                spawnPos = GetSafeEdgeSpawnPosition();
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
                }

                activeObjects.Add(obj);
            }
        }

        private Vector2 GetSafeEdgeSpawnPosition()
        {
            if (GridManager.Instance == null)
            {
                return Vector2.zero;
            }

            // Find the snake controller to get occupied cells
            SnakeController snake = FindFirstObjectByType<SnakeController>();
            IReadOnlyList<Vector2> snakeOccupiedCells = snake != null ? snake.GetOccupiedCells() : null;

            int maxAttempts = 30;
            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2Int edgeGridPos = GridManager.Instance.GetRandomEdgeGridPosition();
                Vector2 worldPos = GridManager.Instance.GridToWorld(edgeGridPos);

                // Check if this position is occupied by the snake
                bool occupiedBySnake = false;
                if (snakeOccupiedCells != null)
                {
                    foreach (Vector2 snakeCell in snakeOccupiedCells)
                    {
                        Vector2Int snakeCellGrid = GridManager.Instance.WorldToGrid(snakeCell);
                        if (snakeCellGrid == edgeGridPos)
                        {
                            occupiedBySnake = true;
                            break;
                        }
                    }
                }

                // If not occupied by snake, check for other obstacles
                if (!occupiedBySnake)
                {
                    LayerMask obstacleMask = LayerMask.GetMask("Props", "Enemy");
                    Collider2D hit = Physics2D.OverlapCircle(worldPos, 0.4f, obstacleMask);
                    if (hit == null)
                    {
                        return worldPos;
                    }
                }
            }

            // Fallback: return any edge position
            return GridManager.Instance.GetRandomEdgeWorldPosition();
        }
        
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

