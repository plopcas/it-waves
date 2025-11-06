using UnityEngine;
using ITWaves.Level;
using ITWaves.Core;
using ITWaves.Snake;
using System.Collections.Generic;

namespace ITWaves.Enemies
{
    public class Crawler : EnemyBase
    {
        [Header("Crawler Settings")]
        [SerializeField, Tooltip("Steps per burst.")]
        private int stepsPerBurst = 3;

        [SerializeField, Tooltip("Pause duration between bursts.")]
        private float pauseDuration = 1.5f;

        [SerializeField, Tooltip("Steps per second during slow burst.")]
        private float slowStepsPerSecond = 2f;

        [SerializeField, Tooltip("Steps per second during fast burst.")]
        private float fastStepsPerSecond = 6f;

        private enum CrawlerState
        {
            Burst,
            Pause
        }

        private enum MovementType
        {
            Diagonal,
            Lateral
        }

        private CrawlerState state = CrawlerState.Pause;
        private float stateTimer;
        private bool isFastBurst; // Track if current burst is fast or slow
        private MovementType currentMovementType;
        
        public override void Initialise(LevelDifficultyProfile difficulty, int levelIndex)
        {
            base.Initialise(difficulty, levelIndex);
            state = CrawlerState.Pause;
            stateTimer = pauseDuration;
        }

        private Vector2 currentGridPos;
        private float stepTimer;
        private int stepsRemaining;
        private Vector2Int currentDirection;
        private SnakeController snake;

        protected override void Awake()
        {
            base.Awake();
            currentGridPos = GridManager.Instance.SnapToGrid(transform.position);
            rb.position = currentGridPos;

            // Find snake reference
            snake = FindFirstObjectByType<SnakeController>();
        }

        protected override void UpdateBehaviour()
        {
            stateTimer -= Time.deltaTime;
            stepTimer -= Time.deltaTime;

            switch (state)
            {
                case CrawlerState.Burst:
                    if (stepTimer <= 0f && stepsRemaining > 0)
                    {
                        TakeStep();
                        float currentSpeed = isFastBurst ? fastStepsPerSecond : slowStepsPerSecond;
                        stepTimer = 1f / currentSpeed;
                        stepsRemaining--;

                        if (stepsRemaining <= 0)
                        {
                            StartPause();
                        }
                    }
                    break;

                case CrawlerState.Pause:
                    if (stateTimer <= 0f)
                    {
                        StartBurst();
                    }
                    break;
            }
        }

        private void TakeStep()
        {
            // Calculate next position
            Vector2 nextGridPos = currentGridPos + (Vector2)currentDirection;
            nextGridPos = GridManager.Instance.SnapToGrid(nextGridPos);

            // Check if next position is valid and not blocked
            if (IsValidMove(nextGridPos))
            {
                currentGridPos = nextGridPos;
            }
            // If blocked, stay in place (don't move into obstacles)
        }

        private void FixedUpdate()
        {
            // Snap directly to current grid position (no smooth interpolation - discrete grid movement like snake)
            rb.MovePosition(currentGridPos);
            transform.position = currentGridPos;
        }

        private void StartBurst()
        {
            state = CrawlerState.Burst;
            stepsRemaining = stepsPerBurst;
            stepTimer = 0f;

            // Randomly choose speed (50% chance of fast or slow)
            isFastBurst = Random.value > 0.5f;

            // Randomly choose movement type (50% chance of diagonal or lateral)
            currentMovementType = Random.value > 0.5f ? MovementType.Diagonal : MovementType.Lateral;

            // Always move toward center of screen (where player is)
            Vector2 dirToCenter = (Vector2.zero - currentGridPos).normalized;

            // Convert to grid direction based on movement type
            int x = 0;
            int y = 0;

            if (currentMovementType == MovementType.Diagonal)
            {
                // Diagonal movement: both x and y can be non-zero
                if (dirToCenter.x > 0.3f) x = 1;
                else if (dirToCenter.x < -0.3f) x = -1;

                if (dirToCenter.y > 0.3f) y = 1;
                else if (dirToCenter.y < -0.3f) y = -1;
            }
            else
            {
                // Lateral movement: only x OR y, choose the dominant direction
                if (Mathf.Abs(dirToCenter.x) > Mathf.Abs(dirToCenter.y))
                {
                    // Move horizontally
                    if (dirToCenter.x > 0.3f) x = 1;
                    else if (dirToCenter.x < -0.3f) x = -1;
                }
                else
                {
                    // Move vertically
                    if (dirToCenter.y > 0.3f) y = 1;
                    else if (dirToCenter.y < -0.3f) y = -1;
                }
            }

            currentDirection = new Vector2Int(x, y);
        }

        private void StartPause()
        {
            state = CrawlerState.Pause;
            stateTimer = pauseDuration;
        }

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
