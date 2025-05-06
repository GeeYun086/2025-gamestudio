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
            if (RespawnController.Instance) RespawnController.Instance.TrySetCheckpointToActiveByID(CheckpointID);
        }
    }
}