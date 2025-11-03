using UnityEngine;
using ITWaves.Level;
using ITWaves.Core;

namespace ITWaves.Enemies
{
    /// <summary>
    /// Skitterer enemy: zig-zag wander, periodic charges at player.
    /// </summary>
    public class Skitterer : EnemyBase
    {
        [Header("Skitterer Settings")]
        [SerializeField, Tooltip("Steps per second during wander.")]
        private float wanderStepsPerSecond = 2f;

        [SerializeField, Tooltip("Wander direction change interval.")]
        private float wanderChangeInterval = 1f;

        [SerializeField, Tooltip("Steps during charge.")]
        private int chargeSteps = 5;

        [SerializeField, Tooltip("Steps per second during charge.")]
        private float chargeStepsPerSecond = 6f;

        [SerializeField, Tooltip("Charge cooldown.")]
        private float chargeCooldown = 4f;

        private enum SkittererState
        {
            Wander,
            Charge
        }

        private SkittererState state = SkittererState.Wander;
        private float stateTimer;
        private float chargeTimer;
        private float stepTimer;
        private int stepsRemaining;
        private Vector2Int currentDirection;
        private Vector2 currentGridPos;
        
        protected override void Awake()
        {
            base.Awake();
            currentGridPos = GridManager.Instance.SnapToGrid(transform.position);
            rb.position = currentGridPos;
        }

        public override void Initialise(LevelDifficultyProfile difficulty, int levelIndex)
        {
            base.Initialise(difficulty, levelIndex);
            state = SkittererState.Wander;
            stateTimer = 0f;
            chargeTimer = chargeCooldown;
            PickNewWanderDirection();
        }

        protected override void UpdateBehaviour()
        {
            stateTimer -= Time.deltaTime;
            chargeTimer -= Time.deltaTime;
            stepTimer -= Time.deltaTime;

            switch (state)
            {
                case SkittererState.Wander:
                    if (stepTimer <= 0f)
                    {
                        TakeStep();
                        stepTimer = 1f / wanderStepsPerSecond;
                    }

                    if (stateTimer <= 0f)
                    {
                        PickNewWanderDirection();
                        stateTimer = wanderChangeInterval;
                    }

                    // Check if can charge
                    if (chargeTimer <= 0f && IsPlayerInRange())
                    {
                        StartCharge();
                    }
                    break;

                case SkittererState.Charge:
                    if (stepTimer <= 0f && stepsRemaining > 0)
                    {
                        TakeStep();
                        stepTimer = 1f / chargeStepsPerSecond;
                        stepsRemaining--;

                        if (stepsRemaining <= 0)
                        {
                            EndCharge();
                        }
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
        
        private void PickNewWanderDirection()
        {
            // Pick random grid direction (8-directional)
            int[] options = { -1, 0, 1 };
            int x = options[Random.Range(0, 3)];
            int y = options[Random.Range(0, 3)];

            // Avoid staying still
            if (x == 0 && y == 0)
            {
                x = Random.value > 0.5f ? 1 : -1;
            }

            currentDirection = new Vector2Int(x, y);

            // Rotate to face direction
            if (currentDirection != Vector2Int.zero)
            {
                float angle = Mathf.Atan2(currentDirection.y, currentDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
            }
        }
        
        private void StartCharge()
        {
            state = SkittererState.Charge;
            stepsRemaining = chargeSteps;
            stepTimer = 0f;
            chargeTimer = chargeCooldown;

            // Pick direction toward player (grid-aligned)
            Vector2 dirToPlayer = GetDirectionToPlayer();

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
        
        private void EndCharge()
        {
            state = SkittererState.Wander;
            PickNewWanderDirection();
            stateTimer = wanderChangeInterval;
        }
    }
}

