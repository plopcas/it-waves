using System.Collections.Generic;
using UnityEngine;
using ITWaves.Core;

namespace ITWaves.Level
{
    /// <summary>
    /// Generates procedural arena layouts with boxes and spawn points.
    /// Uses GridManager for precise grid-based placement.
    /// </summary>
    public class ProceduralLayoutGenerator : MonoBehaviour
    {
        [Header("References")]
        [SerializeField, Tooltip("Box prefab to spawn.")]
        private GameObject boxPrefab;

        [Header("Generation Settings")]
        [SerializeField, Tooltip("Minimum distance between boxes (in grid cells).")]
        private int minBoxDistanceInCells = 2;

        private List<GameObject> spawnedBoxes = new List<GameObject>();
        private System.Random rng;
        
        /// <summary>
        /// Generate a new layout for the given level.
        /// </summary>
        public void Generate(LevelConfig config, LevelDifficultyProfile difficulty, int levelIndex)
        {
            // Clear previous layout
            ClearLayout();

            // Initialize RNG with seed
            int seed = config.seed != 0 ? config.seed : levelIndex;
            rng = new System.Random(seed);

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
            float density = difficulty.GetBoxDensity(levelIndex);
            int boxCount = Mathf.RoundToInt((gridArea / 100f) * density);

            // Generate boxes
            GenerateBoxes(config, boxCount);
        }
        
        private void GenerateBoxes(LevelConfig config, int count)
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
            foreach (var gridPos in gridPositions)
            {
                Vector2 worldPos = grid.GridToWorld(gridPos);
                GameObject box = Instantiate(boxPrefab, worldPos, Quaternion.identity, transform);
                spawnedBoxes.Add(box);
            }
        }
        
        /// <summary>
        /// Clear all spawned objects.
        /// </summary>
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
        
        /// <summary>
        /// Get a random spawn position on the edge of the arena (grid-based).
        /// </summary>
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

