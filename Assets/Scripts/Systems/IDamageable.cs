using UnityEngine;

namespace ITWaves.Systems
{
    public interface IDamageable
    {
        void ApplyDamage(float amount, GameObject source = null);
        
        bool IsAlive { get; }
    }
}

