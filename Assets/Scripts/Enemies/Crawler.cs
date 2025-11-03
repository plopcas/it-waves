using UnityEngine;
using ITWaves.Level;
using ITWaves.Core;

namespace ITWaves.Enemies
{
    /// <summary>
    /// Crawler enemy: moves in bursts toward player on grid.
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
        
        [SerializeField, Tooltip("Puddle prefab (optional).")]
        private GameObject puddlePrefab;
        
        [SerializeField, Tooltip("Puddle spawn interval.")]
        private float puddleSpawnInterval = 2f;
        
        private enum CrawlerState
        {
            Idle,
            Burst,
            Pause
        }
        
        private CrawlerState state = CrawlerState.Idle;
        private float stateTimer;
        private float puddleTimer;
        private Vector2 burstDirection;
        
        public override void Initialise(LevelDifficultyProfile difficulty, int levelIndex)
        {
            base.Initialise(difficulty, levelIndex);
            state = CrawlerState.Idle;
            stateTimer = 0f;
        }
        
        private Vector2 currentGridPos;
        private float stepTimer;
        private int stepsRemaining;
        private Vector2Int currentDirection;

        protected override void Awake()
        {
            base.Awake();
            currentGridPos = GridManager.Instance.SnapToGrid(transform.position);
            rb.position = currentGridPos;
        }

        protected override void UpdateBehaviour()
        {
            stateTimer -= Time.deltaTime;
            puddleTimer -= Time.deltaTime;
            stepTimer -= Time.deltaTime;

            switch (state)
            {
                case CrawlerState.Idle:
                    if (IsPlayerInRange())
                    {
                        StartBurst();
                    }
                    break;

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
                    // Spawn puddle
                    if (puddleTimer <= 0f && puddlePrefab != null)
                    {
                        SpawnPuddle();
                        puddleTimer = puddleSpawnInterval;
                    }

                    if (stateTimer <= 0f)
                    {
                        state = CrawlerState.Idle;
                    }
                    break;
            }
        }

        private void TakeStep()
        {
            // Move one grid cell in current direction
            currentGridPos += (Vector2)currentDirection;
            currentGridPos = GridManager.Instance.SnapToGrid(currentGridPos);
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

            // Pick direction toward player (grid-aligned: up, down, left, right, or diagonal)
            Vector2 dirToPlayer = GetDirectionToPlayer();

            // Convert to grid direction (8-directional)
            int x = 0;
            int y = 0;

            if (dirToPlayer.x > 0.3f) x = 1;
            else if (dirToPlayer.x < -0.3f) x = -1;

            if (dirToPlayer.y > 0.3f) y = 1;
            else if (dirToPlayer.y < -0.3f) y = -1;

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
        
        private void SpawnPuddle()
        {
            if (puddlePrefab != null)
            {
                Instantiate(puddlePrefab, transform.position, Quaternion.identity);
            }
        }
    }
}

