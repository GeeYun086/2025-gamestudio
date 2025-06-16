using UnityEngine;

namespace GravityGame
{
    public class PoolReset : MonoBehaviour
    {
        public ImpactPool ImpactPool;

        void OnParticleSystemStopped()
        {
            ImpactPool.ReturnObject(gameObject);
        }
    }
}