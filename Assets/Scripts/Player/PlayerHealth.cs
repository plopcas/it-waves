using System;
using UnityEngine;
using ITWaves.Systems;

namespace ITWaves.Player
{
    // Simple one-touch death system for the player.
    // Any damage instantly kills the player.
    public class PlayerHealth : MonoBehaviour, IDamageable
    {
        [Header("Visual Feedback")]
        [SerializeField, Tooltip("Hit flash component (optional).")]
        private HitFlash hitFlash;

        private bool isAlive = true;

        public bool IsAlive => isAlive;

        // Events
        public event Action OnDied;

        private void Awake()
        {
            if (hitFlash == null)
            {
                hitFlash = GetComponent<HitFlash>();
            }
        }

        public void ApplyDamage(float amount, GameObject source = null)
        {
            if (!isAlive)
            {
                return;
            }

            // One-touch death - any damage kills the player
            HandleDeath(source);
        }

        private void HandleDeath(GameObject source)
        {
            isAlive = false;

            // Flash effect on death
            if (hitFlash != null)
            {
                hitFlash.Flash();
            }

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

            // Notify listeners
            OnDied?.Invoke();

            // Add brief pause before triggering game over (similar to snake kill)
            // Check if killed by enemy (critter) vs snake
            bool killedByEnemy = source != null && source.GetComponent<Enemies.EnemyBase>() != null;
            float delay = killedByEnemy ? 1f : 0f; // 1 second pause for critter kills

            // Notify level manager after delay
            if (delay > 0f)
            {
                StartCoroutine(TriggerGameOverAfterDelay(delay));
            }
            else
            {
                TriggerGameOver();
            }
        }

        private System.Collections.IEnumerator TriggerGameOverAfterDelay(float delay)
        {
            yield return new UnityEngine.WaitForSeconds(delay);
            TriggerGameOver();
        }

        private void TriggerGameOver()
        {
            // Notify level manager
            var levelManager = ITWaves.LevelManager.Instance;
            if (levelManager != null)
            {
                levelManager.HandlePlayerDied();
            }
        }
    }
}

