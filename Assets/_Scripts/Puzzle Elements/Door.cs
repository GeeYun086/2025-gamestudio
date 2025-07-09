using GravityGame.Audio;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Add to Door GameObject to toggle it open/closed and play the corresponding sound.
    /// Prevents playing sounds during initial scene setup.
    /// </summary>
    [RequireComponent(typeof(DoorSound))]
    public class Door : RedstoneComponent
    {
        private enum DoorState { Open, Closed }

        [SerializeField] private DoorState _whenPowered;
        [SerializeField] private bool _isPowered;

        private DoorSound _doorSound;
        private bool _suppressInitialSounds = true;

        void Awake()
        {
            _doorSound = GetComponent<DoorSound>();
            // initialize visuals without sound
            ToggleDoor(playSound: false);
        }

        void Start()
        {
            // allow sounds after initial setup
            _suppressInitialSounds = false;
        }

        public override bool IsPowered
        {
            get => _isPowered;
            set
            {
                bool changed = _isPowered != value;
                _isPowered = value;
                ToggleDoor(playSound: changed);
            }
        }

        /// <summary>
        /// Plays open/close sound first, then shows or hides the door.
        /// </summary>
        private void ToggleDoor(bool playSound = true)
        {
            bool isOpen = (_isPowered && _whenPowered == DoorState.Open)
                          || (!_isPowered && _whenPowered == DoorState.Closed);

            // play sound before disabling the object
            if (!_suppressInitialSounds && playSound && _doorSound != null)
            {
                if (isOpen)  _doorSound.PlayOpen();
                else         _doorSound.PlayClose();
            }

            // now show or hide the door
            gameObject.SetActive(!isOpen);
        }
    }
}