using GravityGame.Player;
using GravityGame.Utils;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Drops the current carry in front of the trigger when passing through.
    ///     Used to prevent the player from carrying a cube through a player-only force field (in "backpack mode"
    /// </summary>
    public class DropCarryOnContact : MonoBehaviour
    {
        void OnTriggerEnter(Collider collision)
        {
            if (!collision.gameObject.TryGetComponent<PlayerCarry>(out var carry)) return;
            var rb = carry.GetComponent<Rigidbody>();
            var point = carry.transform.position + 1.0f * carry.transform.up;
            var direction = -Vector3.Project(rb.linearVelocity, transform.forward).normalized;
            var dropPoint = point + direction * 1.0f;
            carry.ForceDrop(dropPoint);
            
            Debug.DrawRay(point, direction, Color.red, 1f);
            DebugDraw.DrawCube(point + direction * 1.0f, 1.0f, Color.red, 1f);
        }
    }
}