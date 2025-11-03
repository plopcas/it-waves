using UnityEngine;
using ITWaves.Level;
using ITWaves.Core;
using ITWaves.Snake;
using System.Collections.Generic;

namespace ITWaves.Enemies
{
    /// <summary>
    /// Skitterer enemy: moves at constant medium speed with zig-zag pattern toward player (center of screen).
    /// Avoids snake-occupied cells and boxes.
    /// </summary>
    public class Skitterer : EnemyBase
    {
        [Header("Skitterer Settings")]
        [SerializeField, Tooltip("Steps per second (medium speed, between Crawler's slow and fast).")]
        private float stepsPerSecond = 3.5f;

        [SerializeField, Tooltip("Steps before changing direction (zig-zag pattern).")]
        private int stepsBeforeZag = 2;

        private float stepTimer;
        private int stepsSinceLastZag;
        private Vector2Int currentDirection;
        private Vector2Int primaryDirection; // Main direction toward player
        private Vector2Int alternateDirection; // Perpendicular direction for zig-zag
        private bool movingPrimary; // True if moving in primary direction, false if zagging
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
            stepsSinceLastZag = 0;
            movingPrimary = true;
            CalculateZigZagDirections();
        }

        protected override void UpdateBehaviour()
        {
            stepTimer -= Time.deltaTime;

            // Take steps at constant rate
            if (stepTimer <= 0f)
            {
                TakeStep();
                stepTimer = 1f / stepsPerSecond;

                // Check if it's time to zag
                stepsSinceLastZag++;
                if (stepsSinceLastZag >= stepsBeforeZag)
                {
                    ToggleZigZag();
                    stepsSinceLastZag = 0;
                }
            }
        }

        private void TakeStep()
        {
            // Calculate next position based on current direction
            Vector2 nextGridPos = currentGridPos + (Vector2)currentDirection;
            nextGridPos = GridManager.Instance.SnapToGrid(nextGridPos);

            // Check if next position is valid
            if (IsValidMove(nextGridPos))
            {
                currentGridPos = nextGridPos;
            }
            else
            {
                // If blocked, recalculate directions and try to continue
                CalculateZigZagDirections();
            }
        }

        private void FixedUpdate()
        {
            // Snap directly to current grid position (no smooth interpolation - discrete grid movement like snake)
            rb.MovePosition(currentGridPos);
            transform.position = currentGridPos;
        }

        /// <summary>
        /// Calculate primary and alternate directions for zig-zag pattern toward player.
        /// </summary>
        private void CalculateZigZagDirections()
        {
            // Always move toward center of screen (where player is)
            Vector2 dirToCenter = (Vector2.zero - currentGridPos).normalized;

            // Determine primary direction (dominant axis toward player)
            int x = 0;
            int y = 0;

            if (Mathf.Abs(dirToCenter.x) > Mathf.Abs(dirToCenter.y))
            {
                // Horizontal is dominant
                if (dirToCenter.x > 0.3f) x = 1;
                else if (dirToCenter.x < -0.3f) x = -1;

                primaryDirection = new Vector2Int(x, 0);

                // Alternate direction is vertical
                if (dirToCenter.y > 0.3f) y = 1;
                else if (dirToCenter.y < -0.3f) y = -1;
                alternateDirection = new Vector2Int(0, y);
            }
            else
            {
                // Vertical is dominant
                if (dirToCenter.y > 0.3f) y = 1;
                else if (dirToCenter.y < -0.3f) y = -1;

                primaryDirection = new Vector2Int(0, y);

                // Alternate direction is horizontal
                if (dirToCenter.x > 0.3f) x = 1;
                else if (dirToCenter.x < -0.3f) x = -1;
                alternateDirection = new Vector2Int(x, 0);
            }

            // Set current direction based on zig-zag state
            currentDirection = movingPrimary ? primaryDirection : alternateDirection;
        }

        /// <summary>
        /// Toggle between primary and alternate direction for zig-zag movement.
        /// </summary>
        private void ToggleZigZag()
        {
            movingPrimary = !movingPrimary;
            currentDirection = movingPrimary ? primaryDirection : alternateDirection;
        }

        /// <summary>
        /// Check if a move to the given position is valid (not blocked by snake, boxes, or out of bounds).
        /// </summary>
        private bool IsValidMove(Vector2 worldPos)
        {
            if (GridManager.Instance == null)
            {
                return false;
            }

            // Check if position is within grid bounds
            Vector2Int gridPos = GridManager.Instance.WorldToGrid(worldPos);
            if (!GridManager.Instance.IsValidGridPosition(gridPos))
            {
                return false;
            }

            // Check if occupied by snake
            if (IsCellOccupiedBySnake(worldPos))
            {
                return false;
            }

            // Check if occupied by boxes or other obstacles
            if (IsCellOccupiedByObstacle(worldPos))
            {
                return false;
            }

            return true;
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

        /// <summary>
        /// Check if a grid cell is occupied by boxes or other obstacles.
        /// </summary>
        private bool IsCellOccupiedByObstacle(Vector2 worldPos)
        {
            if (GridManager.Instance == null)
            {
                return false;
            }

            // Check for boxes and props at this position
            float checkRadius = GridManager.Instance.CellSize * 0.4f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(worldPos, checkRadius);

            foreach (Collider2D hit in hits)
            {
                // Check for boxes or props layer
                if (hit.CompareTag("Box") || hit.gameObject.layer == LayerMask.NameToLayer("Props"))
                {
                    return true;
                }
            }

            return false;
        }
    }
}

