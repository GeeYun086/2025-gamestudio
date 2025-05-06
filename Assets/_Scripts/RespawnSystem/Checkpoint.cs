using GravityGame.Player;
using UnityEngine;

namespace GravityGame.RespawnSystem
{
    [RequireComponent(typeof(Collider))]
    public class Checkpoint : MonoBehaviour
    {
        const string DefaultCheckpointId = "Unique_Checkpoint_Id";

        [SerializeField]
        string checkpointID = DefaultCheckpointId;

        public bool HasBeenReached { get; set; }

        public string CheckpointID {
            get => checkpointID;
            set => checkpointID = value;
        }


        void Awake()
        {
            var tCollider = GetComponent<Collider>();
            if (!tCollider.isTrigger)
                tCollider.isTrigger = true;
        }

        void Start() => RespawnController.RegisterCheckpoint(this);

        void OnDestroy() => RespawnController.UnregisterCheckpoint(this);

        void OnTriggerEnter(Collider other)
        {
            if (!other.GetComponentInParent<FirstPersonCameraController>() &&
                !other.GetComponent<FirstPersonCameraController>())
                return;

            HasBeenReached = true;
            RespawnController.Instance.SetCurrentRespawnCheckpoint(this);
        }
    }
}