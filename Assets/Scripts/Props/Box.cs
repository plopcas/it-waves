using UnityEngine;
using ITWaves.Systems;

namespace ITWaves.Props
{
    /// <summary>
    /// Destructible box obstacle (Centipede mushroom homage).
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class Box : MonoBehaviour, IDamageable
    {
        [Header("Box Settings")]
        [SerializeField, Tooltip("Hits required to destroy.")]
        private int maxHits = 3;
        
        [SerializeField, Tooltip("Score value on destroy.")]
        private int scoreValue = 5;
        
        [Header("Visual Feedback")]
        [SerializeField, Tooltip("Sprite renderer.")]
        private SpriteRenderer spriteRenderer;
        
        [SerializeField, Tooltip("Damage colours (by hit count).")]
        private Color[] damageColours = new Color[]
        {
            Color.white,
            new Color(0.8f, 0.8f, 0.8f),
            new Color(0.6f, 0.6f, 0.6f)
        };
        
        [Header("Destruction")]
        [SerializeField, Tooltip("Debris prefab (optional).")]
        private GameObject debrisPrefab;
        
        [SerializeField, Tooltip("Number of debris pieces.")]
        private int debrisCount = 3;
        
        private int currentHits;
        private HitFlash hitFlash;
        
        public bool IsAlive => currentHits < maxHits;
        
        private void Awake()
        {
            currentHits = 0;

            if (spriteRenderer == null)
            {
                spriteRenderer = GetComponent<SpriteRenderer>();
            }

            hitFlash = GetComponent<HitFlash>();
            UpdateVisual();
        }
        
        /// <summary>
        /// Apply damage to the box.
        /// </summary>
        public void ApplyDamage(float amount, GameObject source = null)
        {
            if (!IsAlive)
            {
                return;
            }
            
            int hits = Mathf.RoundToInt(amount);
            ApplyDamage(hits);
        }
        
        /// <summary>
        /// Apply damage by hit count.
        /// </summary>
        public void ApplyDamage(int hits = 1)
        {
            if (!IsAlive)
            {
                return;
            }
            
            currentHits += hits;
            
            if (hitFlash != null)
            {
                hitFlash.Flash();
            }
            
            if (currentHits >= maxHits)
            {
                HandleDestruction();
            }
            else
            {
                UpdateVisual();
            }
        }
        
        private void UpdateVisual()
        {
            if (spriteRenderer != null && damageColours.Length > 0)
            {
                int colourIndex = Mathf.Clamp(currentHits, 0, damageColours.Length - 1);
                spriteRenderer.color = damageColours[colourIndex];
            }
        }
        
        private void HandleDestruction()
        {
            // Add score
            if (Core.GameManager.Instance != null)
            {
                Core.GameManager.Instance.AddScore(scoreValue);
            }

            // Spawn debris
            if (debrisPrefab != null)
            {
                for (int i = 0; i < debrisCount; i++)
                {
                    Vector3 offset = Random.insideUnitCircle * 0.5f;
                    GameObject debris = Instantiate(debrisPrefab, transform.position + offset, Random.rotation);
                    
                    // Add random velocity to debris
                    Rigidbody2D debrisRb = debris.GetComponent<Rigidbody2D>();
                    if (debrisRb != null)
                    {
                        debrisRb.linearVelocity = Random.insideUnitCircle * 3f;
                        debrisRb.angularVelocity = Random.Range(-360f, 360f);
                    }
                    
                    // Auto-destroy debris after a delay
                    Destroy(debris, 2f);
                }
            }
            
            Destroy(gameObject);
        }
    }
}

