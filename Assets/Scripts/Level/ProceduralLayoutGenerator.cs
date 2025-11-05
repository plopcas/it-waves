using System.Collections.Generic;
using UnityEngine;
using ITWaves.Core;

namespace ITWaves.Level
{
    public class ProceduralLayoutGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Box prefab to spawn.")]
        private GameObject boxPrefab;

        [SerializeField, Tooltip("Treasure box prefab to spawn (optional).")]
        private GameObject treasureBoxPrefab;

        [Header("Generation Settings")]
        [SerializeField, Tooltip("Minimum distance between boxes (in grid cells).")]
        private int minBoxDistanceInCells = 2;

        [SerializeField, Tooltip("Wave number when treasure boxes start appearing.")]
        private int treasureBoxStartWave = 5;

        private List<GameObject> spawnedBoxes = new List<GameObject>();
        private System.Random rng;
        private int generationCounter = 0; // Track how many times we've generated to vary seed

        public void Generate(LevelConfig config, LevelDifficultyProfile difficulty, int difficultyLevel, int currentWave)
        {
            // Clear previous layout
            ClearLayout();

            // Initialize RNG with a seed that changes each wave
            // Use Time.frameCount and generationCounter to ensure different layouts each time
            int seed = config.seed != 0 ? config.seed + generationCounter : (Time.frameCount + generationCounter);
            rng = new System.Random(seed);
            generationCounter++;

            if (!config.useProcedural)
            {
                return;
            }

            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager not found! Cannot generate layout.");
                return;
            }

            // Calculate box count based on density and grid area
            int gridArea = GridManager.Instance.GridWidth * GridManager.Instance.GridHeight;
            float density = difficulty.GetBoxDensity(difficultyLevel);
            int boxCount = Mathf.RoundToInt((gridArea / 100f) * density);

            // Check if we should spawn a treasure box (only if not already collected)
            bool spawnTreasure = currentWave >= treasureBoxStartWave &&
                                 treasureBoxPrefab != null &&
                                 !Systems.SaveManager.IsTreasureCollected();

            // Generate boxes
            GenerateBoxes(config, boxCount, spawnTreasure);
        }
        
        private void GenerateBoxes(LevelConfig config, int count, bool spawnTreasure)
        {
            if (boxPrefab == null)
            {
                Debug.LogWarning("Box prefab not assigned to ProceduralLayoutGenerator.");
                return;
            }

            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager not found!");
                return;
            }

            GridManager grid = GridManager.Instance;
            Vector2Int playerGridPos = grid.WorldToGrid(Vector2.zero); // Player spawns at center
            int safeRadiusInCells = Mathf.CeilToInt(config.playerSafeRadius / config.gridCellSize);

            List<Vector2Int> gridPositions = new List<Vector2Int>();
            int attempts = 0;
            int maxAttempts = count * 10;

            while (gridPositions.Count < count && attempts < maxAttempts)
            {
                attempts++;

                // Generate random grid position
                int gridX = rng.Next(0, grid.GridWidth);
                int gridY = rng.Next(0, grid.GridHeight);
                Vector2Int gridPos = new Vector2Int(gridX, gridY);

                // Check if too close to player (in grid cells)
                int distToPlayer = Mathf.Max(Mathf.Abs(gridPos.x - playerGridPos.x), Mathf.Abs(gridPos.y - playerGridPos.y));
                if (distToPlayer < safeRadiusInCells)
                {
                    continue;
                }

                // Check if too close to other boxes (in grid cells)
                bool tooClose = false;
                foreach (var existingGridPos in gridPositions)
                {
                    int dist = Mathf.Max(Mathf.Abs(gridPos.x - existingGridPos.x), Mathf.Abs(gridPos.y - existingGridPos.y));
                    if (dist < minBoxDistanceInCells)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    gridPositions.Add(gridPos);
                }
            }

            // Spawn boxes at grid positions
            int treasureIndex = spawnTreasure && gridPositions.Count > 0 ? rng.Next(0, gridPositions.Count) : -1;

            for (int i = 0; i < gridPositions.Count; i++)
            {
                Vector2Int gridPos = gridPositions[i];
                Vector2 worldPos = grid.GridToWorld(gridPos);

                // Spawn treasure box at random position, regular box otherwise
                GameObject prefabToSpawn = (i == treasureIndex) ? treasureBoxPrefab : boxPrefab;
                GameObject box = Instantiate(prefabToSpawn, worldPos, Quaternion.identity, transform);
                spawnedBoxes.Add(box);

                if (i == treasureIndex)
                {
                    Debug.Log($"Treasure box spawned at {worldPos}!");
                }
            }
        }
        
        public void ClearLayout()
        {
            foreach (var box in spawnedBoxes)
            {
                if (box != null)
                {
                    Destroy(box);
                }
            }
            spawnedBoxes.Clear();
        }
        
        public Vector2 GetRandomEdgeSpawnPosition(LevelConfig config)
        {
            if (rng == null)
            {
                rng = new System.Random();
            }

            if (GridManager.Instance == null)
            {
                Debug.LogWarning("GridManager not found! Using fallback position.");
                return Vector2.zero;
            }

            // Use GridManager to get a random edge position
            Vector2Int gridPos = GridManager.Instance.GetRandomEdgeGridPosition();
            return GridManager.Instance.GridToWorld(gridPos);
        }
    }
}

