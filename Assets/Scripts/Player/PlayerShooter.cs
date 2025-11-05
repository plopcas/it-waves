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
        
        private PlayerController controller;
        private AudioSource audioSource;
        private float nextFireTime;
        private bool isFiring;
        private bool hasFiredThisPress;
        
        private void Awake()
        {
            controller = GetComponent<PlayerController>();
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
            // Check if mouse button is still held using the new Input System
            bool isMouseButtonHeld = UnityEngine.InputSystem.Mouse.current != null &&
                                     UnityEngine.InputSystem.Mouse.current.leftButton.isPressed;

            // If button is no longer held, stop firing
            if (isFiring && !isMouseButtonHeld)
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
        
        public void HandleFire(bool pressed)
        {
            if (!pressed || Time.time < nextFireTime)
            {
                return;
            }
            
            Fire();
            nextFireTime = Time.time + (1f / fireRate);
        }
        
        private void Fire()
        {
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
                audioSource.PlayOneShot(shootSound);
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

