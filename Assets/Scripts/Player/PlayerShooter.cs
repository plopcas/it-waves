using UnityEngine;
using UnityEngine.InputSystem;
using ITWaves.Systems;

namespace ITWaves.Player
{
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Shooting Settings")]
        [SerializeField, Tooltip("Base fire rate (shots per second).")]
        private float baseFireRate = 5f;

        [SerializeField, Tooltip("Enable automatic fire when holding button.")]
        private bool automaticFire = true;

        private float fireRate; // Current fire rate (can be boosted)

        [SerializeField, Tooltip("Bullet spawn offset from player.")]
        private float bulletSpawnOffset = 0.5f;

        [SerializeField, Tooltip("Bullet prefab.")]
        private GameObject bulletPrefab;
        
        [Header("Audio")]
        [SerializeField, Tooltip("Shoot sound effect.")]
        private AudioClip shootSound;

        [SerializeField, Range(0f, 1f), Tooltip("Volume of the shoot sound.")]
        private float shootSoundVolume = 1f;
        
        private PlayerController controller;
        private PlayerHealth playerHealth;
        private AudioSource audioSource;
        private float nextFireTime;
        private bool isFiring;
        private bool hasFiredThisPress;

        [Header("Snake Pause (Treasure 3)")]
        [SerializeField, Tooltip("Number of hits before triggering snake pause.")]
        private int hitsPerPause = 5;

        [SerializeField, Tooltip("Number of clicks/moves to pause snake for.")]
        private int pauseClickCount = 4;

        [SerializeField, Tooltip("Cooldown clicks after stunning before can stun again.")]
        private int stunCooldownClicks = 4;

        private int hitsSincePause = 0; // Track hits since last pause
        private int cooldownClicksRemaining = 0; // Track cooldown after stun
        
        private void Awake()
        {
            controller = GetComponent<PlayerController>();
            playerHealth = GetComponent<PlayerHealth>();
            audioSource = GetComponent<AudioSource>();
            fireRate = baseFireRate; // Initialize current fire rate

            // Apply saved fire rate boost if any
            float savedBoost = SaveManager.GetFireRateBoost();
            if (savedBoost > 0f)
            {
                fireRate += savedBoost;
                Debug.Log($"[PlayerShooter] Applied saved fire rate boost: +{savedBoost}. Current fire rate: {fireRate} shots/sec");
            }
        }
        
        public void OnAttack(InputValue value)
        {
            // Don't shoot if player is dead
            if (playerHealth != null && !playerHealth.IsAlive)
            {
                return;
            }

            // Check if button was just pressed
            if (value.isPressed)
            {
                isFiring = true;

                // Fire immediately on button press if ready
                if (Time.time >= nextFireTime)
                {
                    Fire();
                    nextFireTime = Time.time + (1f / fireRate);
                    hasFiredThisPress = true;
                }
            }
        }

        private void Update()
        {
            // Don't shoot if player is dead
            if (playerHealth != null && !playerHealth.IsAlive)
            {
                isFiring = false;
                hasFiredThisPress = false;
                return;
            }

            // Check if mouse button or touch is still held using the new Input System
            bool isMouseButtonHeld = UnityEngine.InputSystem.Mouse.current != null &&
                                     UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;
            bool isTouchHeld = UnityEngine.InputSystem.Touchscreen.current != null &&
                              UnityEngine.InputSystem.Touchscreen.current.primaryTouch.press.isPressed;

            // If button/touch is no longer held, stop firing
            if (isFiring && !isMouseButtonHeld && !isTouchHeld)
            {
                isFiring = false;
                hasFiredThisPress = false;
            }

            // Continue firing while button is held (only if automatic fire is enabled)
            if (automaticFire && isFiring && hasFiredThisPress && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }

        private void Fire()
        {
            // Don't shoot if player is dead
            if (playerHealth != null && !playerHealth.IsAlive)
            {
                return;
            }

            if (bulletPrefab == null)
            {
                Debug.LogWarning("No bullet prefab assigned!");
                return;
            }

            Vector2 aimDir = controller != null ? controller.AimDirection : Vector2.right;
            Vector3 spawnPos = transform.position + (Vector3)aimDir * bulletSpawnOffset;

            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

            if (bullet != null)
            {
                var bulletComponent = bullet.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    bulletComponent.Launch(aimDir);
                }
                else
                {
                    Debug.LogError("[PlayerShooter] Bullet component not found on instantiated bullet!");
                }
            }
            else
            {
                Debug.LogError("[PlayerShooter] Failed to instantiate bullet!");
            }

            // Play sound
            if (audioSource != null && shootSound != null)
            {
                float sfxVolume = SaveManager.GetSFXVolume();
                audioSource.PlayOneShot(shootSound, shootSoundVolume * sfxVolume);
            }
        }

        // Called by Bullet when it hits the snake (only if stun gun is enabled)
        public void OnSnakeHit()
        {
            if (!SaveManager.IsSnakePauseEnabled())
            {
                return;
            }

            // Decrement cooldown if active
            if (cooldownClicksRemaining > 0)
            {
                cooldownClicksRemaining--;
                Debug.Log($"[PlayerShooter] Stun cooldown: {cooldownClicksRemaining} clicks remaining");
                return; // Don't count hits during cooldown
            }

            hitsSincePause++;

            // Every 5 hits, pause the snake
            if (hitsSincePause >= hitsPerPause)
            {
                hitsSincePause = 0;
                cooldownClicksRemaining = stunCooldownClicks; // Start cooldown
                TriggerSnakePause();
            }
        }

        private void TriggerSnakePause()
        {
            // Find the snake and pause it
            var snake = FindFirstObjectByType<Snake.SnakeController>();
            if (snake != null)
            {
                snake.PauseForClicks(pauseClickCount);
                Debug.Log($"[PlayerShooter] Snake paused for {pauseClickCount} clicks! Cooldown: {stunCooldownClicks} clicks");
            }
            else
            {
                Debug.LogWarning("[PlayerShooter] Snake not found, cannot pause!");
            }
        }

        // Increases the fire rate by the specified amount.
        // Called by power-ups like treasure boxes.
        public void IncreaseFireRate(float amount)
        {
            float oldRate = fireRate;
            fireRate += amount;
            Debug.Log($"[PlayerShooter] Fire rate increased from {oldRate} to {fireRate} shots/sec (boost: +{amount})");
        }

        // Resets fire rate to base value.
        public void ResetFireRate()
        {
            fireRate = baseFireRate;
            Debug.Log($"[PlayerShooter] Fire rate reset to base: {fireRate} shots/sec");
        }

        // Get current fire rate.
        public float GetFireRate()
        {
            return fireRate;
        }
    }
}

