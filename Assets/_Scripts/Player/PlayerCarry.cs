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
        [SerializeField] Transform _playerCameraTransform;
        [SerializeField] Transform _carryPointTransform;
        [SerializeField] LayerMask _pickUpLayerMask;

        Carryable _carrying;

        void Update()
        {
            if (!Input.GetKeyDown(KeyCode.E))
                return;
            if (!_carrying) {
                float maxPickUpDistance = 4f;
                if (Physics.Raycast(
                        _playerCameraTransform.position, _playerCameraTransform.forward,
                        out var hit, maxPickUpDistance, _pickUpLayerMask
                    ) && hit.transform.TryGetComponent(out _carrying)) {
                    _carrying.PickUp(_carryPointTransform);
                }
            } else {
                _carrying.Release();
                _carrying = null;
            }
        }
    }
}