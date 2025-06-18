using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Add to Door GameObject
    /// </summary>
    public class Door : RedstoneComponent
    {
        enum DoorState { Open, Closed }

        [SerializeField] DoorState _whenPowered;
        bool _isPowered;

        public override bool IsPowered
        {
            get => _isPowered;
            set {
                _isPowered = value;
                ToggleDoor();
            }
        }

        void ToggleDoor()
        {
            bool isOpen = IsPowered == (_whenPowered == DoorState.Open);
            // currently just toggles the GameObject. Should have an animation in the future
            gameObject.SetActive(!isOpen);
        }
    }
}