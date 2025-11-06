using UnityEngine;
using System.Collections.Generic;

namespace ITWaves.Level
{
    /// <summary>
    /// Manages fog that progressively closes in from the edges as waves increase.
    /// Fog spawns at specific wave milestones and covers more edge cells each time.
    /// </summary>
    public class FogManager : MonoBehaviour
    {
        public static FogManager Instance { get; private set; }

        [Header("Fog Prefab")]
        [SerializeField, Tooltip("Fog prefab to spawn at edge cells.")]
        private GameObject fogPrefab;

        [Header("Fog Progression Settings")]
        [SerializeField, Tooltip("Wave milestones when fog increases (e.g., [3, 7, 12, 17]).")]
        private int[] fogWaveMilestones = new int[] { 3, 7, 12, 17 };

        [SerializeField, Tooltip("Fog depth (number of edge lines) at each milestone.")]
        private int[] fogDepthAtMilestone = new int[] { 1, 2, 3, 4 };

        [Header("Messages")]
        [SerializeField, Tooltip("Message to show when fog first appears.")]
        private string firstFogMessage = "THE AIR THICKENS";

        [SerializeField, Tooltip("Message to show when fog increases after first time.")]
        private string subsequentFogMessage = "THE FOG CLOSES IN";

        [Header("Rendering")]
        [SerializeField, Tooltip("Sorting layer name for fog (should be above everything except UI).")]
        private string fogSortingLayer = "Default";

        [SerializeField, Tooltip("Sorting order for fog (higher = on top).")]
        private int fogSortingOrder = 100;

        // State
        private int currentFogDepth = 0;
        private bool hasShownFirstFogMessage = false;
        private List<GameObject> spawnedFogObjects = new List<GameObject>();

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
            // Subscribe to wave started events
            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnWaveStarted += HandleWaveStarted;
            }
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            if (LevelManager.Instance != null)
            {
                LevelManager.Instance.OnWaveStarted -= HandleWaveStarted;
            }
        }

        private void HandleWaveStarted(int waveNumber)
        {
            // Check if this wave triggers a fog increase
            int newFogDepth = GetFogDepthForWave(waveNumber);

            if (newFogDepth > currentFogDepth)
            {
                // Fog is increasing!
                currentFogDepth = newFogDepth;
                SpawnFog();
                ShowFogMessage();

                Debug.Log($"[FogManager] Wave {waveNumber}: Fog depth increased to {currentFogDepth}");
            }
        }

        /// <summary>
        /// Determines the fog depth for a given wave based on milestones.
        /// </summary>
        private int GetFogDepthForWave(int waveNumber)
        {
            int depth = 0;

            for (int i = 0; i < fogWaveMilestones.Length; i++)
            {
                if (waveNumber >= fogWaveMilestones[i])
                {
                    // Use the corresponding depth, or default to i+1 if array is shorter
                    depth = (i < fogDepthAtMilestone.Length) ? fogDepthAtMilestone[i] : (i + 1);
                }
                else
                {
                    break;
                }
            }

            return depth;
        }

        /// <summary>
        /// Spawns fog at all edge cells up to the current fog depth.
        /// </summary>
        private void SpawnFog()
        {
            if (fogPrefab == null)
            {
                Debug.LogWarning("[FogManager] Fog prefab not assigned!");
                return;
            }

            if (Core.GridManager.Instance == null)
            {
                Debug.LogError("[FogManager] GridManager not found!");
                return;
            }

            // Clear existing fog
            ClearFog();

            var grid = Core.GridManager.Instance;
            int gridWidth = grid.GridWidth;
            int gridHeight = grid.GridHeight;

            // Spawn fog at edge cells based on current depth
            for (int depth = 0; depth < currentFogDepth; depth++)
            {
                // Top edge (moving inward)
                for (int x = depth; x < gridWidth - depth; x++)
                {
                    int y = gridHeight - 1 - depth;
                    if (y >= 0 && y < gridHeight)
                    {
                        SpawnFogAtGridPosition(x, y);
                    }
                }

                // Bottom edge (moving inward)
                for (int x = depth; x < gridWidth - depth; x++)
                {
                    int y = depth;
                    if (y >= 0 && y < gridHeight)
                    {
                        SpawnFogAtGridPosition(x, y);
                    }
                }

                // Left edge (moving inward, excluding corners already covered)
                for (int y = depth + 1; y < gridHeight - depth - 1; y++)
                {
                    int x = depth;
                    if (x >= 0 && x < gridWidth)
                    {
                        SpawnFogAtGridPosition(x, y);
                    }
                }

                // Right edge (moving inward, excluding corners already covered)
                for (int y = depth + 1; y < gridHeight - depth - 1; y++)
                {
                    int x = gridWidth - 1 - depth;
                    if (x >= 0 && x < gridWidth)
                    {
                        SpawnFogAtGridPosition(x, y);
                    }
                }
            }

            Debug.Log($"[FogManager] Spawned {spawnedFogObjects.Count} fog objects at depth {currentFogDepth}");
        }

        /// <summary>
        /// Spawns a single fog object at the specified grid position.
        /// </summary>
        private void SpawnFogAtGridPosition(int gridX, int gridY)
        {
            var grid = Core.GridManager.Instance;
            Vector2 worldPos = grid.GridToWorld(gridX, gridY);

            GameObject fogObject = Instantiate(fogPrefab, worldPos, Quaternion.identity, transform);

            // Set sorting layer and order to ensure fog is on top of everything except UI
            var spriteRenderer = fogObject.GetComponent<SpriteRenderer>();
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingLayerName = fogSortingLayer;
                spriteRenderer.sortingOrder = fogSortingOrder;
            }

            spawnedFogObjects.Add(fogObject);
        }

        /// <summary>
        /// Clears all spawned fog objects.
        /// </summary>
        private void ClearFog()
        {
            foreach (var fogObject in spawnedFogObjects)
            {
                if (fogObject != null)
                {
                    Destroy(fogObject);
                }
            }
            spawnedFogObjects.Clear();
        }

        /// <summary>
        /// Shows the appropriate fog message via HUD.
        /// </summary>
        private void ShowFogMessage()
        {
            var hud = FindFirstObjectByType<UI.HUDController>();
            if (hud != null)
            {
                string message = hasShownFirstFogMessage ? subsequentFogMessage : firstFogMessage;
                hud.ShowMessage(message);

                if (!hasShownFirstFogMessage)
                {
                    hasShownFirstFogMessage = true;
                }
            }
        }

        /// <summary>
        /// Public method to manually trigger fog update (useful for testing).
        /// </summary>
        public void UpdateFog(int waveNumber)
        {
            HandleWaveStarted(waveNumber);
        }

        /// <summary>
        /// Resets fog state (useful when restarting game).
        /// </summary>
        public void ResetFog()
        {
            currentFogDepth = 0;
            hasShownFirstFogMessage = false;
            ClearFog();
        }
    }
}

