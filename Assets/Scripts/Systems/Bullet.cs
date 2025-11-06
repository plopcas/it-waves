using UnityEngine;

namespace ITWaves.Systems
{
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Bullet : MonoBehaviour
    {
        [Header("Bullet Properties")]
        [SerializeField, Tooltip("Damage dealt on hit.")]
        private float damage = 1f;

        [SerializeField, Tooltip("Movement speed.")]
        private float speed = 15f;

        [SerializeField, Tooltip("Lifetime before auto-destroy.")]
        private float lifetime = 3f;

        private Rigidbody2D rb;
        private float spawnTime;

        public float Damage => damage;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.gravityScale = 0f;
            }
            else
            {
                Debug.LogError($"[Bullet] Failed to get Rigidbody2D component!");
            }
        }
        
        private void OnEnable()
        {
            spawnTime = Time.time;
            // Reset velocity when enabled from pool
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }

        private void OnDisable()
        {
            // Reset velocity when disabled/returned to pool
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
            }
        }
        
        private void Update()
        {
            // Auto-destroy after lifetime expires
            if (Time.time - spawnTime >= lifetime)
            {
                ReturnToPool();
            }
        }
        
        public void Launch(Vector2 direction)
        {
            // Ensure rb is initialized (in case Launch is called before Awake)
            if (rb == null)
            {
                Debug.LogWarning($"[Bullet] rb was null in Launch, attempting to get component");
                rb = GetComponent<Rigidbody2D>();
                if (rb != null)
                {
                    rb.gravityScale = 0f;
                }
            }

            if (rb == null)
            {
                Debug.LogError("[Bullet] Rigidbody2D is null! Cannot launch.");
                return;
            }

            rb.linearVelocity = direction.normalized * speed;

            // Rotate to face direction
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            // Don't hit the player who shot it
            if (other.CompareTag("Player"))
            {
                return;
            }

            // Check if we hit the snake (for stun gun tracking)
            var snakeController = other.GetComponent<Snake.SnakeController>();
            var snakeSegment = other.GetComponent<Snake.SnakeSegment>();
            bool hitSnake = snakeController != null || snakeSegment != null;

            // Try to damage
            var damageable = other.GetComponent<IDamageable>();
            if (damageable != null)
            {
                damageable.ApplyDamage(damage, gameObject);

                // Notify player shooter if we hit the snake (for stun gun)
                // But NOT if it's the mega head (mega head cannot be stunned)
                if (hitSnake)
                {
                    bool isMegaHead = snakeController != null && snakeController.IsMegaHead;

                    if (!isMegaHead)
                    {
                        var player = GameObject.FindGameObjectWithTag("Player");
                        if (player != null)
                        {
                            var shooter = player.GetComponent<Player.PlayerShooter>();
                            if (shooter != null)
                            {
                                shooter.OnSnakeHit();
                            }
                        }
                    }
                }

                ReturnToPool();
                return;
            }

            // Hit props or walls
            if (other.gameObject.layer == LayerMask.NameToLayer("Props"))
            {
                ReturnToPool();
            }
        }
        
        private void ReturnToPool()
        {
            Destroy(gameObject);
        }
    }
}

