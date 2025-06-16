using System;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.CheckpointSystem
{
    /// <summary>
    ///     Represents an individual checkpoint in the game.
    ///     Attach this component to a GameObject with a Collider (set to trigger) to define a checkpoint.
    ///     When a Player (with a <see cref="PlayerMovement" /> component) enters its trigger,
    ///     it notifies the <see cref="CheckpointController" />.
    ///     It tracks whether it has been reached and if it's currently the active respawn point.
    ///     A unique <see cref="CheckpointID" /> is automatically generated or can be pre-set for identification.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] string _checkpointID;

        bool _hasBeenReached;
        bool _isActiveCheckpoint;
        public event Action<bool> OnHasBeenReachedChanged;
        public event Action<bool> OnIsActiveCheckpointChanged;

        public bool HasBeenReached
        {
            get => _hasBeenReached;
            set {
                if (_hasBeenReached == value) return;
                _hasBeenReached = value;
                OnHasBeenReachedChanged?.Invoke(_hasBeenReached);
            }
        }

        public bool IsActiveCheckpoint
        {
            get => _isActiveCheckpoint;
            set {
                if (_isActiveCheckpoint == value) return;
                _isActiveCheckpoint = value;
                OnIsActiveCheckpointChanged?.Invoke(_isActiveCheckpoint);
            }
        }

        public string CheckpointID
        {
            get {
                if (string.IsNullOrEmpty(_checkpointID)) _checkpointID = gameObject.name + "_" + GetInstanceID();
                return _checkpointID;
            }
        }

        void Awake()
        {
            // Accessing the CheckpointID property here ensures it gets auto-generated if it's null or empty.
            // The '_' discards the result, as we only care about the side effect of generation.
            _ = CheckpointID;
            var tCollider = GetComponent<Collider>();
            if (tCollider && !tCollider.isTrigger) tCollider.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<PlayerMovement>()) return;
            if (CheckpointController.Instance) CheckpointController.Instance.TriggerCheckpointById(CheckpointID);
        }
    }
}