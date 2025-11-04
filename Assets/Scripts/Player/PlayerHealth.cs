using System;
using System.Collections;
using UnityEngine;
using ITWaves.Systems;

namespace ITWaves.Player
{
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Health Settings")]
        [SerializeField, Tooltip("Maximum health.")]
        private int maxHealth = 3;
        
        [SerializeField, Tooltip("Invulnerability duration after taking damage.")]
        private float iFramesDuration = 1f;
        
        [Header("Visual Feedback")]
        [SerializeField, Tooltip("Sprite renderer for flashing.")]
        private SpriteRenderer spriteRenderer;
        
        [SerializeField, Tooltip("Flash colour during i-frames.")]
        private Color iFramesColour = new Color(1f, 1f, 1f, 0.5f);
        
        private int currentHealth;
        private bool isInvulnerable;
        private Color originalColour;
        private HitFlash hitFlash;
        
        public int CurrentHealth => currentHealth;
        public int MaxHealth => maxHealth;
        public bool IsAlive => currentHealth > 0;
        
        // Events
        public event Action<int, int> OnHealthChanged; // current, max
        public event Action OnDied;
        
        private void Awake()
        {
            currentHealth = maxHealth;
            
            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }
            
            if (spriteRenderer != null)
            {
                originalColour = spriteRenderer.color;
            }
            
            hitFlash = GetComponent<HitFlash>();
        }
        
        private void Start()
        {
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
        
        public void ApplyDamage(float amount, GameObject source = null)
        {
            if (isInvulnerable || !IsAlive)
            {
                return;
            }
            
            int damage = Mathf.RoundToInt(amount);
            currentHealth = Mathf.Max(0, currentHealth - damage);
            
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
            
            if (currentHealth <= 0)
            {
                HandleDeath();
            }
            else
            {
                StartCoroutine(InvulnerabilityRoutine());
                
                if (hitFlash != null)
                {
                    hitFlash.Flash();
                }
            }
        }
        
        private IEnumerator InvulnerabilityRoutine()
        {
            isInvulnerable = true;
            
            float elapsed = 0f;
            while (elapsed < iFramesDuration)
            {
                elapsed += Time.deltaTime;
                
                // Flash effect
                if (spriteRenderer != null)
                {
                    float t = Mathf.PingPong(elapsed * 10f, 1f);
                    spriteRenderer.color = Color.Lerp(originalColour, iFramesColour, t);
                }
                
                yield return null;
            }
            
            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalColour;
            }
            
            isInvulnerable = false;
        }
        
        private void HandleDeath()
        {
            OnDied?.Invoke();
            
            // Disable controls
            var controller = GetComponent<PlayerController>();
            if (controller != null)
            {
                controller.enabled = false;
            }
            
            var shooter = GetComponent<PlayerShooter>();
            if (shooter != null)
            {
                shooter.enabled = false;
            }
            
            // Optional: play death animation/effect
        }
        
        public void Heal(int amount)
        {
            if (!IsAlive)
            {
                return;
            }
            
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
            OnHealthChanged?.Invoke(currentHealth, maxHealth);
        }
    }
}

