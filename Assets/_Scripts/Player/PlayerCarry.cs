using GravityGame.Puzzle_Elements;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Allows the player to pick up <see cref="Carryable" /> objects by
    ///     1. looking at them and
    ///     2. pressing the E key
    ///     Object is then carried in front of the Camera.
    ///     It can be released by pressing the E key again.
    /// </summary>
    public class PlayerCarry : MonoBehaviour
    {
        [SerializeField] Transform playerCameraTransform;
        [SerializeField] Transform carryPointTransform;
        [SerializeField] LayerMask pickUpLayerMask;

        Carryable _carrying;

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                if (!_carrying)
                {
                    float maxPickUpDistance = 4f;
                    if (!Physics.Raycast(playerCameraTransform.position, playerCameraTransform.forward,
                            out RaycastHit hit, maxPickUpDistance, pickUpLayerMask)) return;
                    if (hit.transform.TryGetComponent(out _carrying))
                        _carrying.PickUp(carryPointTransform);
                }
                else
                {
                    _carrying.Release();
                    _carrying = null;
                }
            }
        }
    }
}