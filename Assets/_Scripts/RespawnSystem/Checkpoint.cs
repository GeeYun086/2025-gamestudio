using UnityEngine;

namespace GravityGame.RespawnSystem
{
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField]
        private Material activatedMaterial;
        [SerializeField]
        private ParticleSystem activationParticlesPrefab;
        [SerializeField]
        private AudioSource activationSound;
    }
}