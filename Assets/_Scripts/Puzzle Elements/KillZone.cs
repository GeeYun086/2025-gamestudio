using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        [SerializeField] bool _killPlayer = true;

        void OnTriggerEnter(Collider other)
        {
            var victim = other.gameObject;
            var playerMovement = victim.GetComponent<PlayerMovement>();

            if (playerMovement) {
                if (_killPlayer) PlayerHealth.Instance.TakeDamage(PlayerHealth.MaxHealth);
                return;
            }
            Destroy(victim);
        }
    }
}