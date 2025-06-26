using UnityEngine;

namespace GravityGame
{
    public interface ILaserConfig
    {
        float MaxDistance { get; }
        float FlatDamage { get; }
        float KnockbackForce { get; }
        float BeamRadius { get; }
        float DamageCooldown { get; }
        LayerMask ObstacleMask { get; }
    }
}