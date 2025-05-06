using System;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.CheckpointSystem
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] string checkpointID;

        bool _hasBeenReached;
        bool _isActiveCheckpoint;
        public event Action<bool> OnHasBeenReachedChanged;
        public event Action<bool> OnIsActiveCheckpointChanged;

        public bool HasBeenReached {
            get => _hasBeenReached;
            set {
                if (_hasBeenReached == value) return;
                _hasBeenReached = value;
                OnHasBeenReachedChanged?.Invoke(_hasBeenReached);
            }
        }

        public bool IsActiveCheckpoint {
            get => _isActiveCheckpoint;
            set {
                if (_isActiveCheckpoint == value) return;
                _isActiveCheckpoint = value;
                OnIsActiveCheckpointChanged?.Invoke(_isActiveCheckpoint);
            }
        }

        public string CheckpointID {
            get {
                if (string.IsNullOrEmpty(checkpointID)) checkpointID = gameObject.name + "_" + GetInstanceID();
                return checkpointID;
            }
        }

        void Awake()
        {
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