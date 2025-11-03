using UnityEngine;
using ITWaves.Level;

namespace ITWaves.Enemies
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class EnemyBase : MonoBehaviour
    {
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

