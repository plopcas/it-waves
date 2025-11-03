using UnityEngine;
using UnityEngine.InputSystem;
using ITWaves.Systems;

namespace ITWaves.Player
{
    /// <summary>
    /// Handles player shooting mechanics.
    /// </summary>
    public class PlayerShooter : MonoBehaviour
    {
        [Header("Shooting Settings")]
        [SerializeField, Tooltip("Fire rate (shots per second).")]
        private float fireRate = 5f;

        [SerializeField, Tooltip("Enable automatic fire when holding button.")]
        private bool automaticFire = true;

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
        }
        
        /// <summary>
        /// Handle fire input from Input System.
        /// </summary>
        public void OnAttack(InputValue value)
        {
            // Check if button was just pressed
            if (value.isPressed)
            {
                isFiring = true;
                Debug.Log("Attack button pressed - starting fire");

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
                Debug.Log("Attack button released - stopping fire");
            }

            // Continue firing while button is held (only if automatic fire is enabled)
            if (automaticFire && isFiring && hasFiredThisPress && Time.time >= nextFireTime)
            {
                Fire();
                nextFireTime = Time.time + (1f / fireRate);
            }
        }
        
        /// <summary>
        /// Handle firing logic.
        /// </summary>
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

            Debug.Log($"[PlayerShooter] Firing bullet at {spawnPos}, direction: {aimDir}");

            GameObject bullet = Instantiate(bulletPrefab, spawnPos, Quaternion.identity);

            if (bullet != null)
            {
                Debug.Log($"[PlayerShooter] Bullet instantiated: {bullet.name}, active: {bullet.activeSelf}");

                var bulletComponent = bullet.GetComponent<Bullet>();
                if (bulletComponent != null)
                {
                    Debug.Log($"[PlayerShooter] Launching bullet component");
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
    }
}

