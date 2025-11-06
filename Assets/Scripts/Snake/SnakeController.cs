using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using ITWaves.Level;
using ITWaves.Systems;
using ITWaves.Core;

namespace ITWaves.Snake
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class SnakeController : MonoBehaviour, IDamageable
    {
        [Header("Configuration")]
        [SerializeField, Tooltip("Snake configuration.")]
        private SnakeConfig config;

        [Header("Grid Movement")]
        [SerializeField, Tooltip("Steps per second.")]
        private float stepsPerSecond = 3f;

        [Header("References")]
        [SerializeField, Tooltip("List of segments.")]
        private List<SnakeSegment> segments = new List<SnakeSegment>();

        private Rigidbody2D rb;
        private Transform player;
        private Vector2 currentGridPos;
        private Vector2 horizontalDirection; // Left or right (1, 0) or (-1, 0)
        private Vector2 verticalDirection; // Down or up (0, -1) or (0, 1)
        private float stepTimer;
        private int currentLevel = 1;
        private List<Vector2> positionHistory = new List<Vector2>(); // Track head positions for segments to follow
        private List<Vector2> pathHistory = new List<Vector2>(); // Track all cells the snake has occupied
        private int stepsSinceLastZag = 0;
        private int stepsUntilNextZag = 5;
        private bool isRetreating = false;
        private bool isVisible = false; // Track if snake has been spawned yet
        private int pauseClicksRemaining = 0; // Number of clicks/moves to pause for
        private bool isMegaHead = false; // Wave 20 mega head mode
        private Vector3 originalScale; // Store original scale for mega head transformation
        private int megaHeadHealth = 5; // Mega head requires 5 hits to defeat
        private const int MAX_MEGA_HEAD_HEALTH = 5;

        public bool IsAlive => segments.Count > 0 || isMegaHead || gameObject != null;
        public int SegmentCount => segments.Count;
        public bool IsVisible => isVisible;
        public bool IsMegaHead => isMegaHead;

        public IReadOnlyList<Vector2> GetOccupiedCells()
        {
            return pathHistory;
        }

        public Vector2 GetSegmentTargetPosition(int segmentIndex)
        {
            // Position history stores all positions the head has been at
            // Index 0 is the oldest, last index is most recent
            // We want segment 0 to be at the position the head was 1 step ago

            int historyIndex = positionHistory.Count - 1 - segmentIndex;

            if (historyIndex >= 0 && historyIndex < positionHistory.Count)
            {
                return positionHistory[historyIndex];
            }

            // Not enough history yet - stay at current position
            if (segmentIndex < segments.Count)
            {
                return segments[segmentIndex].transform.position;
            }

            return currentGridPos;
        }
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            // Store original scale
            originalScale = transform.localScale;

            if (GridManager.Instance != null)
            {
                // Snap to grid
                currentGridPos = GridManager.Instance.SnapToGrid(transform.position);
                transform.position = currentGridPos;
            }
            else
            {
                currentGridPos = transform.position;
            }
        }

        private void Start()
        {
            // Snake is visible immediately
            isVisible = true;

            // Add head's starting position to history
            positionHistory.Add(currentGridPos);
            pathHistory.Add(currentGridPos);

            // Calculate next random zag interval
            if (config != null)
            {
                stepsUntilNextZag = Random.Range(config.minStepsBeforeRandomZag, config.maxStepsBeforeRandomZag + 1);
            }
        }
        
        public void Initialise(SnakeConfig cfg, LevelDifficultyProfile difficulty, int levelIndex)
        {
            config = cfg;
            currentLevel = levelIndex;

            if (GridManager.Instance == null)
            {
                Debug.LogError("GridManager not found!");
                return;
            }

            float gridCellSize = GridManager.Instance.CellSize;

            if (difficulty != null)
            {
                stepsPerSecond = difficulty.GetSnakeSpeed(levelIndex) / gridCellSize;
            }
            else if (config != null)
            {
                stepsPerSecond = config.baseSpeed / gridCellSize;
            }

            // Find player and initialize direction NOW (before segments spawn)
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }

            InitializeStartDirection();
        }


        
        public void AddSegment(SnakeSegment segment)
        {
            if (segment != null)
            {
                segments.Add(segment);
                // Add the segment's starting position to history so it stays in place initially
                positionHistory.Add((Vector2)segment.transform.position);
            }
        }

        public Vector2 GetInitialDirection()
        {
            return horizontalDirection;
        }
        
        private void Update()
        {
            if (!IsAlive || !isVisible)
            {
                return;
            }

            // Update step timer
            float currentStepsPerSecond = isRetreating && config != null
                ? stepsPerSecond * config.retreatSpeedMultiplier
                : stepsPerSecond;

            float stepInterval = 1f / currentStepsPerSecond;
            stepTimer += Time.deltaTime;

            if (stepTimer >= stepInterval)
            {
                stepTimer -= stepInterval;
                TakeStep();
            }
        }

        private void FixedUpdate()
        {
            if (!IsAlive)
            {
                return;
            }

            // Snap directly to current grid position (no smooth interpolation)
            rb.MovePosition(currentGridPos);
            transform.position = currentGridPos;

            // Rotate to face the current movement direction
            if (horizontalDirection.sqrMagnitude > 0.01f)
            {
                float angle = Mathf.Atan2(horizontalDirection.y, horizontalDirection.x) * Mathf.Rad2Deg;
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }
        
        public void PauseForClicks(int clickCount)
        {
            // Mega head cannot be stunned/paused
            if (isMegaHead)
            {
                Debug.Log("[SnakeController] Mega head cannot be stunned!");
                return;
            }

            pauseClicksRemaining = clickCount;
            Debug.Log($"[SnakeController] Snake paused for {clickCount} clicks");
        }

        private void TakeStep()
        {
            if (GridManager.Instance == null) return;

            // Check if snake is paused
            if (pauseClicksRemaining > 0)
            {
                pauseClicksRemaining--;
                Debug.Log($"[SnakeController] Snake paused, {pauseClicksRemaining} clicks remaining");
                return; // Skip this step
            }

            // If in mega head mode, use special movement
            if (isMegaHead)
            {
                TakeMegaHeadStep();
                return;
            }

            // If retreating, walk back the path
            if (isRetreating)
            {
                TakeRetreatStep();
                return;
            }

            // Add current position to history BEFORE moving
            positionHistory.Add(currentGridPos);
            pathHistory.Add(currentGridPos);

            // Keep history size limited to number of segments + buffer
            int maxHistorySize = segments.Count + 5;
            if (positionHistory.Count > maxHistorySize)
            {
                positionHistory.RemoveAt(0); // Remove oldest
            }

            float cellSize = GridManager.Instance.CellSize;

            // Try to move horizontally first
            Vector2 nextHorizontalPos = currentGridPos + horizontalDirection * cellSize;

            // Check if we're at the edge of the grid
            Vector2Int nextGridPos = GridManager.Instance.WorldToGrid(nextHorizontalPos);
            bool atEdge = !GridManager.Instance.IsValidGridPosition(nextGridPos);

            // Check if there's a box in front (ignore snake segments)
            bool obstacleAhead = CheckForObstacle(nextHorizontalPos);

            // Check if it's time for a random zag
            stepsSinceLastZag++;
            bool shouldRandomZag = stepsSinceLastZag >= stepsUntilNextZag;

            Vector2 nextStep;

            if (atEdge || obstacleAhead || shouldRandomZag)
            {
                // Hit edge, obstacle, or time for random zag - move vertically and reverse horizontal direction
                nextStep = verticalDirection;
                horizontalDirection = -horizontalDirection;

                // Reset zag counter and pick new interval
                if (shouldRandomZag && config != null)
                {
                    stepsSinceLastZag = 0;
                    stepsUntilNextZag = Random.Range(config.minStepsBeforeRandomZag, config.maxStepsBeforeRandomZag + 1);
                }

                // Update directions to move towards player
                if (player != null)
                {
                    Vector2 toPlayer = (Vector2)player.position - currentGridPos;

                    // Update horizontal direction to move towards player
                    if (Mathf.Abs(toPlayer.x) > 0.5f)
                    {
                        horizontalDirection = toPlayer.x > 0 ? Vector2.right : Vector2.left;
                    }

                    // Update vertical direction to move towards player
                    if (Mathf.Abs(toPlayer.y) > 0.5f)
                    {
                        verticalDirection = toPlayer.y > 0 ? Vector2.up : Vector2.down;
                    }
                }
            }
            else
            {
                // Continue moving horizontally
                nextStep = horizontalDirection;
            }

            // Move to next position
            currentGridPos += nextStep * cellSize;
            currentGridPos = GridManager.Instance.SnapToGrid(currentGridPos);

            // Check if snake reached the player (game over) - delayed by one frame so visual catches up
            StartCoroutine(CheckPlayerProximityNextFrame());
        }

        private System.Collections.IEnumerator CheckPlayerProximityNextFrame()
        {
            // Wait for FixedUpdate to run so visual position catches up
            yield return new WaitForFixedUpdate();

            if (player == null || GridManager.Instance == null) yield break;

            // Get grid positions
            Vector2Int snakeGridPos = GridManager.Instance.WorldToGrid(currentGridPos);
            Vector2Int playerGridPos = GridManager.Instance.WorldToGrid(player.position);

            // Check if snake is in any of the 8 cells adjacent to the player (or on the player)
            int deltaX = Mathf.Abs(snakeGridPos.x - playerGridPos.x);
            int deltaY = Mathf.Abs(snakeGridPos.y - playerGridPos.y);

            // MUST be exactly 1 cell away or less (adjacent or on same cell)
            // deltaX and deltaY must BOTH be <= 1, and at least one must be <= 1
            // This means: same cell (0,0), orthogonally adjacent (1,0 or 0,1), or diagonally adjacent (1,1)
            bool isAdjacent = (deltaX <= 1 && deltaY <= 1) && !(deltaX == 0 && deltaY == 0);
            bool isSameCell = (deltaX == 0 && deltaY == 0);

            if (isAdjacent || isSameCell)
            {
                TriggerGameOver();
            }
        }

        private void TriggerGameOver()
        {
            // Stop all movement
            isVisible = false;
            enabled = false;

            // Notify LevelManager to handle game over
            // LevelManager will handle score saving (score from wave start is already saved)
            var levelManager = ITWaves.LevelManager.Instance;
            if (levelManager != null)
            {
                levelManager.HandlePlayerDied();
            }
        }

        private void TakeRetreatStep()
        {
            // Walk back the path in reverse
            if (pathHistory.Count > 1)
            {
                // Remove current position
                pathHistory.RemoveAt(pathHistory.Count - 1);

                // Move to previous position
                currentGridPos = pathHistory[pathHistory.Count - 1];
            }
            else
            {
                // Reached the start of the path - snake escapes!
                OnSnakeEscaped();
            }
        }

        private bool CheckForObstacle(Vector2 position)
        {
            if (GridManager.Instance == null) return false;

            // Check for boxes at this position (ignore snake segments)
            float checkRadius = GridManager.Instance.CellSize * 0.4f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, checkRadius);

            foreach (Collider2D hit in hits)
            {
                // Only count boxes as obstacles, not snake parts
                if (hit.CompareTag("Box") || hit.gameObject.layer == LayerMask.NameToLayer("Props"))
                {
                    return true;
                }
            }

            return false;
        }



        private void InitializeStartDirection()
        {
            // Choose which edge to start from (top, bottom, left, right)
            int edge = Random.Range(0, 4);

            // Determine horizontal direction based on player position
            Vector2 toPlayer = player != null ? ((Vector2)player.position - currentGridPos) : Vector2.right;
            Vector2 preferredHorizontal = toPlayer.x > 0 ? Vector2.right : Vector2.left;
            Vector2 preferredVertical = toPlayer.y > 0 ? Vector2.up : Vector2.down;

            switch (edge)
            {
                case 0: // Top edge - move down towards player
                    verticalDirection = Vector2.down;
                    horizontalDirection = preferredHorizontal;
                    break;

                case 1: // Bottom edge - move up towards player
                    verticalDirection = Vector2.up;
                    horizontalDirection = preferredHorizontal;
                    break;

                case 2: // Left edge - move right towards player
                    horizontalDirection = Vector2.right;
                    verticalDirection = preferredVertical;
                    break;

                case 3: // Right edge - move left towards player
                    horizontalDirection = Vector2.left;
                    verticalDirection = preferredVertical;
                    break;
            }
        }

        public void ApplyDamage(float amount, GameObject source = null)
        {
            if (!IsAlive || !isVisible)
            {
                return;
            }

            // If in mega head mode, reduce health
            if (isMegaHead)
            {
                megaHeadHealth--;
                Debug.Log($"[SnakeController] Mega head hit! Health remaining: {megaHeadHealth}/{MAX_MEGA_HEAD_HEALTH}");

                // Flash effect
                var hitFlash = GetComponent<HitFlash>();
                if (hitFlash != null)
                {
                    hitFlash.Flash();
                }

                // Check if mega head is defeated
                if (megaHeadHealth <= 0)
                {
                    OnMegaHeadDefeated();
                }
                return;
            }

            // Don't spawn boxes when hit - only when destroyed
            // (This fixes the random box spawning issue)

            // Destroy tail segment
            if (segments.Count > 0)
            {
                SnakeSegment tailSegment = segments[segments.Count - 1];
                segments.RemoveAt(segments.Count - 1);

                if (tailSegment != null)
                {
                    Destroy(tailSegment.gameObject);
                }
            }

            // Check if all segments are destroyed
            if (segments.Count == 0 && !isRetreating && !isMegaHead)
            {
                // Check if we should activate mega head mode (wave 20)
                var levelManager = ITWaves.LevelManager.Instance;
                if (levelManager != null && levelManager.CurrentWave == 20)
                {
                    ActivateMegaHeadMode();
                }
                else
                {
                    // Normal retreat behavior
                    isRetreating = true;
                }
            }
        }

        private void OnSnakeEscaped()
        {
            // Notify level manager that wave is complete
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.AddScore(500); // Bonus for completing wave
            }

            // Notify level manager to start next wave
            var levelManager = ITWaves.LevelManager.Instance;
            if (levelManager != null)
            {
                levelManager.HandleSnakeEscaped();
            }

            // Destroy the snake
            Destroy(gameObject);
        }

        private void TakeMegaHeadStep()
        {
            if (GridManager.Instance == null || player == null) return;

            float cellSize = GridManager.Instance.CellSize;

            // Calculate direction to player
            Vector2 toPlayer = (Vector2)player.position - currentGridPos;
            Vector2 moveDirection = Vector2.zero;

            // Move in the dominant direction (larger component)
            if (Mathf.Abs(toPlayer.x) > Mathf.Abs(toPlayer.y))
            {
                // Move horizontally
                moveDirection = toPlayer.x > 0 ? Vector2.right : Vector2.left;
            }
            else
            {
                // Move vertically
                moveDirection = toPlayer.y > 0 ? Vector2.up : Vector2.down;
            }

            // Calculate next position
            Vector2 nextPos = currentGridPos + moveDirection * cellSize;

            // Check for boxes and destroy them
            DestroyBoxesAtPosition(nextPos);

            // Move to next position
            currentGridPos = nextPos;
            currentGridPos = GridManager.Instance.SnapToGrid(currentGridPos);

            // Update direction for rotation
            horizontalDirection = moveDirection;

            // Check if mega head reached the player
            StartCoroutine(CheckPlayerProximityNextFrame());
        }

        private void DestroyBoxesAtPosition(Vector2 position)
        {
            if (GridManager.Instance == null) return;

            // Check for boxes at this position
            float checkRadius = GridManager.Instance.CellSize * 0.6f;
            Collider2D[] hits = Physics2D.OverlapCircleAll(position, checkRadius);

            foreach (Collider2D hit in hits)
            {
                // Destroy boxes in the way
                if (hit.CompareTag("Box") || hit.gameObject.layer == LayerMask.NameToLayer("Props"))
                {
                    var damageable = hit.GetComponent<IDamageable>();
                    if (damageable != null)
                    {
                        // Instantly destroy the box
                        damageable.ApplyDamage(999f, gameObject);
                    }
                }
            }
        }

        private void ActivateMegaHeadMode()
        {
            Debug.Log("[SnakeController] Activating Mega Head Mode!");
            isMegaHead = true;
            isRetreating = false;

            // Reset mega head health
            megaHeadHealth = MAX_MEGA_HEAD_HEALTH;

            // Scale up to 2x size
            transform.localScale = originalScale * 2f;

            // Increase speed
            stepsPerSecond *= 1.5f;

            // Clear path history (no longer retreating)
            pathHistory.Clear();
        }

        private void OnMegaHeadDefeated()
        {
            Debug.Log("[SnakeController] Mega Head defeated!");

            // Award bonus score
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.AddScore(1000); // Big bonus for defeating mega head
            }

            // Notify level manager that snake is defeated (triggers win)
            var levelManager = ITWaves.LevelManager.Instance;
            if (levelManager != null)
            {
                levelManager.HandleSnakeDefeated();
            }

            // Destroy the snake
            Destroy(gameObject);
        }
    }
}

