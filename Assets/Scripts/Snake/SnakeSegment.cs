using UnityEngine;
using ITWaves.Systems;

namespace ITWaves.Snake
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class SnakeSegment : MonoBehaviour, IDamageable
    {
        [Header("References")]
        [SerializeField, Tooltip("Snake controller (head).")]
        private SnakeController snakeController;

        private Rigidbody2D rb;
        private HitFlash hitFlash;
        private int segmentIndex; // Position in the snake chain

        public bool IsAlive => snakeController != null;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;
            hitFlash = GetComponent<HitFlash>();
        }

        public void SetSnakeController(SnakeController controller, int index)
        {
            snakeController = controller;
            segmentIndex = index;
        }

        private void LateUpdate()
        {
            if (snakeController == null || !IsAlive)
            {
                return;
            }

            // Get target position from controller's position history
            Vector2 targetPosition = snakeController.GetSegmentTargetPosition(segmentIndex);
            Vector2 currentPos = transform.position;

            // Only update if position has changed
            if (Vector2.Distance(currentPos, targetPosition) > 0.01f)
            {
                // Snap directly to target position (no smooth interpolation)
                rb.MovePosition(targetPosition);
                transform.position = targetPosition;

                // Rotate to face direction of movement
                Vector2 moveDir = (targetPosition - currentPos).normalized;
                if (moveDir.sqrMagnitude > 0.01f)
                {
                    float angle = Mathf.Atan2(moveDir.y, moveDir.x) * Mathf.Rad2Deg;
                    transform.rotation = Quaternion.Euler(0f, 0f, angle);
                }
            }
        }
        
        public void ApplyDamage(float amount, GameObject source = null)
        {
            if (snakeController != null)
            {
                // Flash this segment
                if (hitFlash != null)
                {
                    hitFlash.Flash();
                }

                // Pass damage to controller (which will destroy tail)
                snakeController.ApplyDamage(amount, source);
            }
        }
    }
}

