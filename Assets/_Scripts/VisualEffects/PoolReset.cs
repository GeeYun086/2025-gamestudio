using UnityEngine;

namespace GravityGame
{
    /// <summary>
    /// small scipt returning Host gameobject to defined Pool
    /// </summary>
    public class PoolReset : MonoBehaviour
    {
        public ImpactPool ImpactPool;

        void OnParticleSystemStopped()
        {
            ImpactPool.ReturnObject(gameObject);
        }
    }
}