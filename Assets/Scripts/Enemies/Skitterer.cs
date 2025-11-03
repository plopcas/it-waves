using UnityEngine;
using ITWaves.Level;
using ITWaves.Core;
using ITWaves.Snake;
using System.Collections.Generic;

namespace ITWaves.Enemies
{
    /// <summary>
    /// Skitterer enemy: constantly moves toward player (center of screen), avoiding snake-occupied cells.
    /// Faster than Crawler.
    /// </summary>
    public class Skitterer : EnemyBase
    {
        [Header("Skitterer Settings")]
        [SerializeField, Tooltip("Steps per second.")]
        private float stepsPerSecond = 3f;

        [SerializeField, Tooltip("Direction change interval.")]
        private float directionChangeInterval = 0.5f;

        private float stepTimer;
        private float directionTimer;
        private Vector2Int currentDirection;
        private Vector2 currentGridPos;
        private SnakeController snake;
        
        protected override void Awake()
        {
            base.Awake();
            currentGridPos = GridManager.Instance.SnapToGrid(transform.position);
            rb.position = currentGridPos;

            // Find snake reference
            snake = FindFirstObjectByType<SnakeController>();
        }

        public override void Initialise(LevelDifficultyProfile difficulty, int levelIndex)
        {
            base.Initialise(difficulty, levelIndex);
            directionTimer = 0f;
            UpdateDirectionTowardPlayer();
        }

        protected override void UpdateBehaviour()
        {
            stepTimer -= Time.deltaTime;
            directionTimer -= Time.deltaTime;

            // Update direction periodically
            if (directionTimer <= 0f)
            {
                UpdateDirectionTowardPlayer();
                directionTimer = directionChangeInterval;
            }

            // Take steps
            if (stepTimer <= 0f)
            {
                TakeStep();
                stepTimer = 1f / stepsPerSecond;
            }
        }

        private void TakeStep()
        {
            // Calculate next position
            Vector2 nextGridPos = currentGridPos + (Vector2)currentDirection;
            nextGridPos = GridManager.Instance.SnapToGrid(nextGridPos);

            // Check if next position is occupied by snake
            if (!IsCellOccupiedBySnake(nextGridPos))
            {
                currentGridPos = nextGridPos;
            }
            // If blocked by snake, stay in place (don't move into snake)
        }

        private void FixedUpdate()
        {
            // Smoothly move visual position to grid position
            rb.MovePosition(Vector2.MoveTowards(rb.position, currentGridPos, moveSpeed * Time.fixedDeltaTime));
        }

        /// <summary>
        /// Update direction to move toward center of screen (where player is).
        /// </summary>
        private void UpdateDirectionTowardPlayer()
        {
            // Always move toward center of screen (where player is)
            Vector2 dirToCenter = (Vector2.zero - currentGridPos).normalized;

            // Convert to grid direction (8-directional)
            int x = 0;
            int y = 0;

            if (dirToCenter.x > 0.3f) x = 1;
            else if (dirToCenter.x < -0.3f) x = -1;

            if (dirToCenter.y > 0.3f) y = 1;
            else if (dirToCenter.y < -0.3f) y = -1;

            currentDirection = new Vector2Int(x, y);

            // Rotate to face direction
            if (currentDirection != Vector2Int.zero)
            {
                float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }
        }

        /// <summary>
        /// Check if a grid cell is occupied by the snake.
        /// </summary>
        private bool IsCellOccupiedBySnake(Vector2 worldPos)
        {
            if (snake == null || GridManager.Instance == null)
            {
                return false;
            }

            IReadOnlyList<Vector2> snakeOccupiedCells = snake.GetOccupiedCells();
            if (snakeOccupiedCells == null || snakeOccupiedCells.Count == 0)
            {
                return false;
            }

            Vector2Int checkGridPos = GridManager.Instance.WorldToGrid(worldPos);

            foreach (Vector2 snakeCell in snakeOccupiedCells)
            {
                Vector2Int snakeCellGrid = GridManager.Instance.WorldToGrid(snakeCell);
                if (snakeCellGrid == checkGridPos)
                {
                    return true;
                }
            }

            return false;
        }
    }
}

