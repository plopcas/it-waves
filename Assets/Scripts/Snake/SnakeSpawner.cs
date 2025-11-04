using System.Collections.Generic;
using UnityEngine;
using ITWaves.Level;
using ITWaves.Core;

namespace ITWaves.Snake
{
    public class SnakeSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField, Tooltip("Snake head prefab.")]
        private GameObject snakeHeadPrefab;

        [SerializeField, Tooltip("Snake segment prefab.")]
        private GameObject snakeSegmentPrefab;

        [Header("Configuration")]
        [SerializeField, Tooltip("Snake configuration.")]
        private SnakeConfig snakeConfig;

        [SerializeField, Tooltip("Difficulty profile.")]
        private LevelDifficultyProfile difficultyProfile;

        [SerializeField, Tooltip("Level config for arena bounds.")]
        private LevelConfig levelConfig;

        public SnakeController Spawn(Vector2 position, int levelIndex)
        {
            if (difficultyProfile == null)
            {
                Debug.LogError("Difficulty profile not assigned!");
                return null;
            }

            int segmentCount = difficultyProfile.GetSnakeSegments(levelIndex);
            return SpawnWithSegmentCount(position, levelIndex, segmentCount);
        }

        public SnakeController SpawnWithSegmentCount(Vector2 position, int levelIndex, int segmentCount)
        {
            if (snakeHeadPrefab == null)
            {
                Debug.LogError("Snake head prefab not assigned!");
                return null;
            }

            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager not found! Cannot spawn snake.");
                return null;
            }

            // Find an empty edge position
            Vector2 spawnPosition = FindEmptyEdgePosition();
            if (spawnPosition == Vector2.zero)
            {
                Debug.LogWarning("Could not find empty edge position for snake!");
                spawnPosition = position;
            }

            // Snap position to grid
            Vector2 snappedPosition = GridManager.Instance.SnapToGrid(spawnPosition);

            // Spawn head
            GameObject headObj = Instantiate(snakeHeadPrefab, snappedPosition, Quaternion.identity);
            SnakeController controller = headObj.GetComponent<SnakeController>();

            if (controller == null)
            {
                Debug.LogError("Snake head prefab missing SnakeController component!");
                Destroy(headObj);
                return null;
            }

            // Initialize controller
            controller.Initialise(snakeConfig, difficultyProfile, levelIndex);

            // Determine which edge the head is on and spawn segments outward
            Vector2 outwardDirection = DetermineOutwardDirection(snappedPosition);
            SpawnSegments(controller, snappedPosition, segmentCount, outwardDirection);

            return controller;
        }

        private Vector2 FindEmptyEdgePosition()
        {
            if (GridManager.Instance == null) return Vector2.zero;

            // Try to find an empty edge position (not occupied by boxes or enemies)
            int maxAttempts = 20;
            LayerMask obstacleMask = LayerMask.GetMask("Props", "Enemy");
            float checkRadius = 0.4f;

            for (int attempt = 0; attempt < maxAttempts; attempt++)
            {
                Vector2Int edgeGridPos = GridManager.Instance.GetRandomEdgeGridPosition();
                Vector2 worldPos = GridManager.Instance.GridToWorld(edgeGridPos);

                // Check if position is empty using Physics2D
                Collider2D hit = Physics2D.OverlapCircle(worldPos, checkRadius, obstacleMask);
                if (hit == null)
                {
                    return worldPos;
                }
            }

            // Fallback: return any edge position
            return GridManager.Instance.GetRandomEdgeWorldPosition();
        }

        private Vector2 DetermineOutwardDirection(Vector2 position)
        {
            if (GridManager.Instance == null || levelConfig == null)
            {
                return Vector2.left; // Default fallback
            }

            // Get arena bounds
            float halfWidth = levelConfig.arenaWidth / 2f;
            float halfHeight = levelConfig.arenaHeight / 2f;

            // Determine which edge the position is closest to
            float distToTop = Mathf.Abs(position.y - halfHeight);
            float distToBottom = Mathf.Abs(position.y - (-halfHeight));
            float distToLeft = Mathf.Abs(position.x - (-halfWidth));
            float distToRight = Mathf.Abs(position.x - halfWidth);

            float minDist = Mathf.Min(distToTop, distToBottom, distToLeft, distToRight);

            if (minDist == distToTop)
                return Vector2.up;
            else if (minDist == distToBottom)
                return Vector2.down;
            else if (minDist == distToLeft)
                return Vector2.left;
            else
                return Vector2.right;
        }

        private void SpawnSegments(SnakeController controller, Vector2 startPosition, int count, Vector2 outwardDirection)
        {
            if (snakeSegmentPrefab == null)
            {
                Debug.LogWarning("Snake segment prefab not assigned!");
                return;
            }

            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager not found!");
                return;
            }

            float gridCellSize = GridManager.Instance.CellSize;

            for (int i = 0; i < count; i++)
            {
                // Spawn each segment one grid cell further in the outward direction
                Vector2 segmentPos = startPosition + outwardDirection * gridCellSize * (i + 1);

                // Snap to grid to ensure perfect alignment
                segmentPos = GridManager.Instance.SnapToGrid(segmentPos);

                GameObject segmentObj = Instantiate(snakeSegmentPrefab, segmentPos, Quaternion.identity);

                SnakeSegment segment = segmentObj.GetComponent<SnakeSegment>();
                if (segment != null)
                {
                    segment.SetSnakeController(controller, i);
                    controller.AddSegment(segment);
                }
            }
        }
    }
}

