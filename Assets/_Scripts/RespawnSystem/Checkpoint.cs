using GravityGame.Player;
using UnityEngine;

namespace GravityGame.RespawnSystem
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField]
        string checkpointID;

        public bool HasBeenReached { get; set; }

        public string CheckpointID {
            get {
                if (string.IsNullOrEmpty(checkpointID)) {
                    checkpointID = gameObject.name + "_" + GetInstanceID();
                }
                return checkpointID;
            }
        }

        void Awake()
        {
            _ = CheckpointID;

            var tCollider = GetComponent<Collider>();

            if (tCollider != null && !tCollider.isTrigger) {
                tCollider.isTrigger = true;
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponent<FirstPersonCameraController>()) {
                return;
            }

            Debug.Log($"Player entered Checkpoint: {name} (ID: {CheckpointID})");
            switch (HasBeenReached) {
                case true:
                    return;
                case false:
                    RespawnController.Instance.SetCheckpointToActiveByID(CheckpointID);
                    break;
            }
            HasBeenReached = true;
        }
    }
}