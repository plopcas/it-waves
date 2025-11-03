using UnityEngine;
using ITWaves.Level;

namespace ITWaves.Enemies
{
    /// <summary>
    /// Base class for enemy behaviour.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField, Tooltip("Movement speed.")]
        protected float moveSpeed = 3f;
        
        [SerializeField, Tooltip("Detection range for player.")]
        protected float detectionRange = 10f;
        
        [SerializeField, Tooltip("Damage dealt to player on contact.")]
        protected float contactDamage = 1f;
        
        protected Rigidbody2D rb;
        protected Transform player;
        protected EnemyHealth health;
        protected bool isInitialised;
        
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            health = GetComponent<EnemyHealth>();
            
            // Find player
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }
        
        protected virtual void OnEnable()
        {
            if (health != null)
            {
                health.ResetHealth();
            }
        }
        
        /// <summary>
        /// Initialize enemy with difficulty settings.
        /// </summary>
        public virtual void Initialise(LevelDifficultyProfile difficulty, int levelIndex)
        {
            isInitialised = true;
            // Override in derived classes to apply difficulty scaling
        }
        
        protected virtual void Update()
        {
            if (!isInitialised || health == null || !health.IsAlive)
            {
                return;
            }
            
            UpdateBehaviour();
        }
        
        /// <summary>
        /// Update enemy behaviour (override in derived classes).
        /// </summary>
        protected abstract void UpdateBehaviour();
        
        protected virtual void OnCollisionEnter2D(Collision2D collision)
        {
            // Damage player on contact
            if (collision.gameObject.CompareTag("Player"))
            {
                var playerHealth = collision.gameObject.GetComponent<ITWaves.Systems.IDamageable>();
                if (playerHealth != null)
                {
                    playerHealth.ApplyDamage(contactDamage, gameObject);
                }
            }
        }
        
        /// <summary>
        /// Get direction to player.
        /// </summary>
        protected Vector2 GetDirectionToPlayer()
        {
            if (player == null)
            {
                return Vector2.zero;
            }
            
            return ((Vector2)player.position - (Vector2)transform.position).normalized;
        }
        
        /// <summary>
        /// Get distance to player.
        /// </summary>
        protected float GetDistanceToPlayer()
        {
            if (player == null)
            {
                return float.MaxValue;
            }
            
            return Vector2.Distance(transform.position, player.position);
        }
        
        /// <summary>
        /// Check if player is in detection range.
        /// </summary>
        protected bool IsPlayerInRange()
        {
            return GetDistanceToPlayer() <= detectionRange;
        }
    }
}

