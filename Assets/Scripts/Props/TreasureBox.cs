using UnityEngine;
using System.Collections;
using ITWaves.Systems;

namespace ITWaves.Props
{
    // Special treasure box that grants fire rate boost when destroyed.
    [RequireComponent(typeof(Collider2D))]
    public class TreasureBox : MonoBehaviour, IDamageable
    {
        [Header("Box Settings")]
        [SerializeField, Tooltip("Hits required to destroy.")]
        private int maxHits = 3;
        
        [SerializeField, Tooltip("Score value on destroy.")]
        private int scoreValue = 50;
        
        [Header("Power-up Settings")]
        [SerializeField, Tooltip("Fire rate increase amount (shots per second).")]
        private float fireRateBoost = 1f;
        
        [Header("Visual Feedback")]
        [SerializeField, Tooltip("Sprite renderer.")]
        private SpriteRenderer spriteRenderer;
        
        [SerializeField, Tooltip("Damage colours (by hit count).")]
        private Color[] damageColours = new Color[]
        {
            Color.yellow,
            new Color(0.9f, 0.9f, 0f),
            new Color(0.8f, 0.8f, 0f)
        };
        
        [Header("Destruction")]
        [SerializeField, Tooltip("Debris prefab (optional).")]
        private GameObject debrisPrefab;

        [SerializeField, Tooltip("Number of debris pieces.")]
        private int debrisCount = 5;

        [Header("Audio")]
        [SerializeField, Tooltip("Sound to play when collected.")]
        private AudioClip collectSound;
        
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
        
        public void ApplyDamage(float amount, GameObject source = null)
        {
            if (!IsAlive)
            {
                return;
            }
            
            int hits = Mathf.RoundToInt(amount);
            ApplyDamage(hits);
        }
        
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

            // Start the power-up sequence (this will handle everything including destruction)
            StartCoroutine(PowerUpSequence());
        }

        private IEnumerator PowerUpSequence()
        {
            // Find the player
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player == null)
            {
                Debug.LogWarning("TreasureBox: Player not found!");
                yield break;
            }

            // Get the PlayerShooter component
            var shooter = player.GetComponent<Player.PlayerShooter>();
            if (shooter == null)
            {
                Debug.LogWarning("TreasureBox: PlayerShooter component not found on player!");
                yield break;
            }

            // Mark treasure as collected in save data and save the fire rate boost
            SaveManager.SetTreasureCollected(true);
            SaveManager.AddFireRateBoost(fireRateBoost);

            // Pause the game
            float originalTimeScale = Time.timeScale;
            Time.timeScale = 0f;

            // Play sound using AudioSource.PlayClipAtPoint (creates temporary GameObject, unaffected by time scale)
            if (collectSound != null)
            {
                // Create a temporary audio source that won't be destroyed with this object
                GameObject tempAudio = new GameObject("TreasureCollectSound");
                AudioSource audioSource = tempAudio.AddComponent<AudioSource>();
                audioSource.clip = collectSound;
                audioSource.Play();
                Destroy(tempAudio, collectSound.length);
                Debug.Log($"[TreasureBox] Playing collect sound: {collectSound.name}");
            }

            // Show message on HUD
            var hud = FindFirstObjectByType<UI.HUDController>();
            if (hud != null)
            {
                hud.ShowMessage("FIRE RATE INCREASED");
            }

            // Wait for 1 second (real time, not affected by time scale)
            yield return new WaitForSecondsRealtime(1f);

            // Increase fire rate
            shooter.IncreaseFireRate(fireRateBoost);
            Debug.Log($"[TreasureBox] Treasure collected! New fire rate: {shooter.GetFireRate()} shots/sec");

            // Resume the game
            Time.timeScale = originalTimeScale;

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

            // Finally, destroy the treasure box
            Destroy(gameObject);
        }
    }
}

