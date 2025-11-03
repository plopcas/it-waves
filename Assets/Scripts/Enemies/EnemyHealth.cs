using System;
using UnityEngine;
using ITWaves.Systems;

namespace ITWaves.Enemies
{
    /// <summary>
    /// Manages enemy health and death.
    /// </summary>
    public class EnemyHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField, Tooltip("Maximum health.")]
        private float maxHealth = 3f;
        
        [SerializeField, Tooltip("Score value on death.")]
        private int scoreValue = 10;
        
        private float currentHealth;
        private HitFlash hitFlash;
        
        public float CurrentHealth => currentHealth;
        public float MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
        
        // Events
        public event Action<float, float> OnHealthChanged;
        public event Action OnDied;
        
        private void Awake()
        {
            currentHealth = maxHealth;
            hitFlash = GetComponent<HitFlash>();
        }
        
        /// <summary>
        /// Apply damage to the enemy.
        /// </summary>
        public void ApplyDamage(float amount, GameObject source = null)
        {
            if (!IsAlive)
            {
                return;
            }
            
            currentHealth = Mathf.Max(0f, currentHealth - amount);
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (hitFlash != null)
            {
                hitFlash.Flash();
            }
            
            if (currentHealth <= 0f)
            {
                HandleDeath();
            }
        }
        
        private void HandleDeath()
        {
            OnDied?.Invoke();

            // Add score
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.AddScore(scoreValue);
            }

            // Destroy enemy
            Destroy(gameObject);
        }
        
        /// <summary>
        /// Reset health (for pooling).
        /// </summary>
        public void ResetHealth()
        {
            currentHealth = maxHealth;
        }
    }
}

