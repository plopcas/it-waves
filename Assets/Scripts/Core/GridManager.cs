using UnityEngine;

namespace ITWaves.Core
{
    public class GridManager : MonoBehaviour
    {
        public static GridManager Instance { get; private set; }

        [Header("Grid Settings")]
        [SerializeField, Tooltip("Size of each grid cell (1x1 units).")]
        private float cellSize = 1f;

        [SerializeField, Tooltip("Margin around the grid (in world units).")]
        private float margin = 0.5f;

        [Header("Camera Reference")]
        [SerializeField, Tooltip("Main camera for calculating grid bounds.")]
        private Camera mainCamera;

        // Grid dimensions
        private int gridWidth;
        private int gridHeight;
        private Vector2 gridOrigin; // Bottom-left corner of the grid in world space
        private Vector2 gridCenter; // Center of the grid in world space

        public int GridWidth => gridWidth;
        public int GridHeight => gridHeight;
        public float CellSize => cellSize;
        public Vector2 GridOrigin => gridOrigin;
        public Vector2 GridCenter => gridCenter;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;

            if (mainCamera == null)
            {
                mainCamera = Camera.main;
            }

            CalculateGridDimensions();
        }

        private void CalculateGridDimensions()
        {
            if (mainCamera == null)
            {
                Debug.LogError("GridManager: No camera assigned!");
                return;
            }

            // Get camera bounds in world space
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            // Calculate available space after margins
            float availableWidth = cameraWidth - (margin * 2f);
            float availableHeight = cameraHeight - (margin * 2f);

            // Calculate grid dimensions (whole cells only)
            gridWidth = Mathf.FloorToInt(availableWidth / cellSize);
            gridHeight = Mathf.FloorToInt(availableHeight / cellSize);

            // Calculate actual grid size in world units
            float actualGridWidth = gridWidth * cellSize;
            float actualGridHeight = gridHeight * cellSize;

            // Calculate grid origin (bottom-left corner)
            gridOrigin = new Vector2(-actualGridWidth / 2f, -actualGridHeight / 2f);
            gridCenter = Vector2.zero;
        }

        public Vector2 GridToWorld(int gridX, int gridY)
        {
            float worldX = gridOrigin.x + (gridX * cellSize) + (cellSize / 2f);
            float worldY = gridOrigin.y + (gridY * cellSize) + (cellSize / 2f);
            return new Vector2(worldX, worldY);
        }

        public Vector2 GridToWorld(Vector2Int gridPos)
        {
            return GridToWorld(gridPos.x, gridPos.y);
        }

        public Vector2Int WorldToGrid(Vector2 worldPos)
        {
            int gridX = Mathf.FloorToInt((worldPos.x - gridOrigin.x) / cellSize);
            int gridY = Mathf.FloorToInt((worldPos.y - gridOrigin.y) / cellSize);
            return new Vector2Int(gridX, gridY);
        }

        public Vector2 SnapToGrid(Vector2 worldPos)
        {
            Vector2Int gridPos = WorldToGrid(worldPos);
            return GridToWorld(gridPos);
        }

        public bool IsValidGridPosition(int gridX, int gridY)
        {
            return gridX >= 0 && gridX < gridWidth && gridY >= 0 && gridY < gridHeight;
        }

        public bool IsValidGridPosition(Vector2Int gridPos)
        {
            return IsValidGridPosition(gridPos.x, gridPos.y);
        }

        public bool IsEdgePosition(int gridX, int gridY)
        {
            return gridX == 0 || gridX == gridWidth - 1 || gridY == 0 || gridY == gridHeight - 1;
        }

        public bool IsEdgePosition(Vector2Int gridPos)
        {
            return IsEdgePosition(gridPos.x, gridPos.y);
        }

        public Vector2Int GetRandomGridPosition()
        {
            return new Vector2Int(Random.Range(0, gridWidth), Random.Range(0, gridHeight));
        }

        public Vector2Int GetRandomEdgeGridPosition()
        {
            int edge = Random.Range(0, 4);
            
            switch (edge)
            {
                case 0: // Top edge
                    return new Vector2Int(Random.Range(0, gridWidth), gridHeight - 1);
                case 1: // Bottom edge
                    return new Vector2Int(Random.Range(0, gridWidth), 0);
                case 2: // Left edge
                    return new Vector2Int(0, Random.Range(0, gridHeight));
                case 3: // Right edge
                    return new Vector2Int(gridWidth - 1, Random.Range(0, gridHeight));
                default:
                    return new Vector2Int(0, 0);
            }
        }

        public Vector2 GetRandomEdgeWorldPosition()
        {
            Vector2Int gridPos = GetRandomEdgeGridPosition();
            return GridToWorld(gridPos);
        }

        public bool IsGridCellOccupied(int gridX, int gridY, LayerMask layerMask)
        {
            Vector2 worldPos = GridToWorld(gridX, gridY);
            Collider2D hit = Physics2D.OverlapCircle(worldPos, cellSize * 0.4f, layerMask);
            return hit != null;
        }

        public bool IsGridCellOccupied(Vector2Int gridPos, LayerMask layerMask)
        {
            return IsGridCellOccupied(gridPos.x, gridPos.y, layerMask);
        }

        public Vector2Int GetEmptyEdgeGridPosition(LayerMask obstacleMask, int maxAttempts = 20)
        {
            for (int i = 0; i < maxAttempts; i++)
            {
                Vector2Int gridPos = GetRandomEdgeGridPosition();
                if (!IsGridCellOccupied(gridPos, obstacleMask))
                {
                    return gridPos;
                }
            }

            // Fallback: return any edge position
            return GetRandomEdgeGridPosition();
        }

        public Vector2 GetEmptyEdgeWorldPosition(LayerMask obstacleMask, int maxAttempts = 20)
        {
            Vector2Int gridPos = GetEmptyEdgeGridPosition(obstacleMask, maxAttempts);
            return GridToWorld(gridPos);
        }

        private void OnDrawGizmos()
        {
            if (gridWidth == 0 || gridHeight == 0)
            {
                return;
            }

            Gizmos.color = Color.green;

            // Draw grid cells
            for (int x = 0; x < gridWidth; x++)
            {
                for (int y = 0; y < gridHeight; y++)
                {
                    Vector2 cellCenter = GridToWorld(x, y);
                    Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize * 0.9f);
                }
            }

            // Draw grid bounds
            Gizmos.color = Color.yellow;
            Vector2 gridSize = new Vector2(gridWidth * cellSize, gridHeight * cellSize);
            Gizmos.DrawWireCube(gridCenter, gridSize);
        }
    }
}

