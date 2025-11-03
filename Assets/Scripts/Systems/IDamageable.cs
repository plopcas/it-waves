using UnityEngine;

namespace ITWaves.Systems
{
    /// <summary>
    /// Interface for entities that can take damage.
    /// </summary>
    public interface IDamageable
    {
        /// <summary>
        /// Apply damage to this entity.
        /// </summary>
        /// <param name="amount">Amount of damage to apply.</param>
        /// <param name="source">Source of the damage (optional).</param>
        void ApplyDamage(float amount, GameObject source = null);
        
        /// <summary>
        /// Check if this entity is currently alive.
        /// </summary>
        bool IsAlive { get; }
    }
}

