using GravityGame.Player;
using UnityEngine;

namespace GravityGame.CheckpointSystem
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] string checkpointID;
        public bool HasBeenReached { get; set; }
        public bool IsActiveCheckpoint { get; set; }

        public string CheckpointID {
            get {
                if (string.IsNullOrEmpty(checkpointID)) checkpointID = gameObject.name + "_" + GetInstanceID();
                return checkpointID;
            }
        }

        void Awake()
        {
            _ = CheckpointID;
            IsActiveCheckpoint = false;

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