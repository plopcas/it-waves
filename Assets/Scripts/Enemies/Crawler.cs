using UnityEngine;
using ITWaves.Level;
using ITWaves.Core;
using ITWaves.Snake;
using System.Collections.Generic;

namespace ITWaves.Enemies
{
    /// <summary>
    /// Crawler enemy: moves in bursts toward player (center of screen) on grid, avoiding snake-occupied cells.
    /// </summary>
    public class Crawler : EnemyBase
    {
        [Header("Crawler Settings")]
        [SerializeField, Tooltip("Steps per burst.")]
        private int stepsPerBurst = 3;

        [SerializeField, Tooltip("Pause duration between bursts.")]
        private float pauseDuration = 2f;

        [SerializeField, Tooltip("Steps per second during burst.")]
        private float burstStepsPerSecond = 4f;

        private enum CrawlerState
        {
            Burst,
            Pause
        }

        private CrawlerState state = CrawlerState.Pause;
        private float stateTimer;
        
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
                        stepTimer = 1f / burstStepsPerSecond;
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

        private void StartBurst()
        {
            state = CrawlerState.Burst;
            stepsRemaining = stepsPerBurst;
            stepTimer = 0f;

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

        private void StartPause()
        {
            state = CrawlerState.Pause;
            stateTimer = pauseDuration;
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

