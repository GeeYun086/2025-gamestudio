using GravityGame.Puzzle_Elements;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    /// Manages the state of carrying a Carryable object.
    /// </summary>
    public class PlayerCarry : MonoBehaviour
    {
        [SerializeField] Transform _carryPointTransform;

        Carryable _currentlyCarrying;
        public bool IsCarrying() => _currentlyCarrying;

        public void AttemptPickUp(Carryable objectToCarry)
        {
            if (!IsCarrying() && objectToCarry) {
                _currentlyCarrying = objectToCarry;
                _currentlyCarrying.PickUp(_carryPointTransform);
            }
        }

        public void AttemptRelease()
        {
            if (IsCarrying()) {
                _currentlyCarrying.Release();
                _currentlyCarrying = null;
            }
        }
    }
}