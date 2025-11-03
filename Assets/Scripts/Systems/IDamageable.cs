using UnityEngine;

namespace ITWaves.Systems
{
    public interface IDamageable
    {
        /// <param name="amount">Amount of damage to apply.</param>
        /// <param name="source">Source of the damage (optional).</param>
        void ApplyDamage(float amount, GameObject source = null);
        
        bool IsAlive { get; }
    }
}

