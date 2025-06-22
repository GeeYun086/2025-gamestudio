using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Destroys GameObjects entering its trigger if <see cref="_destroyOther"/> is true, or kills the player if <see cref="_killPlayer"/> is true.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class KillZone : MonoBehaviour
    {
        [SerializeField] bool _killPlayer = true;
        [SerializeField] bool _destroyOther = true;

        void OnTriggerEnter(Collider other)
        {
            var victim = other.gameObject;
            var playerMovement = victim.GetComponent<PlayerMovement>();

            if (playerMovement && _killPlayer) {
                PlayerHealth.Instance.TakeDamage(PlayerHealth.MaxHealth);
                return;
            }
            if (_destroyOther)
                Destroy(victim);
        }
    }
}