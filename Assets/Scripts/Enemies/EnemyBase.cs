using UnityEngine;
using ITWaves.Level;

namespace ITWaves.Enemies
{
    /// <summary>
    /// Base class for enemy behaviour.
    /// Enemies always move toward the player (center of screen) and kill on contact.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
        [Header("Base Settings")]
        [SerializeField, Tooltip("Movement speed for smooth visual interpolation.")]
        protected float moveSpeed = 3f;

        protected Rigidbody2D rb;
        protected EnemyHealth health;
        protected bool isInitialised;
        
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0f;
            health = GetComponent<EnemyHealth>();
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
            // Kill player on contact (player has no health, dies instantly)
            if (collision.gameObject.CompareTag("Player"))
            {
                var playerHealth = collision.gameObject.GetComponent<ITWaves.Systems.IDamageable>();
                if (playerHealth != null)
                {
                    // Apply fatal damage (player dies on any contact)
                    playerHealth.ApplyDamage(999f, gameObject);
                }
            }
        }
    }
}

