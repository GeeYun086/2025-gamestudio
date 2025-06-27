using System;
using GravityGame.Player;
using GravityGame.SaveAndLoadSystem;
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
    public class Checkpoint : MonoBehaviour, ISaveData<Checkpoint.SaveData>
    {
        [SerializeField] string _checkpointID;

        bool _hasBeenReached;
        public event Action<bool> OnHasBeenReachedChanged;

        public bool HasBeenReached
        {
            get => _hasBeenReached;
            set {
                if (_hasBeenReached == value) return;
                _hasBeenReached = value;
                OnHasBeenReachedChanged?.Invoke(_hasBeenReached);
            }
        }

        void Awake()
        {
            var tCollider = GetComponent<Collider>();
            if (tCollider && !tCollider.isTrigger) tCollider.isTrigger = true;
        }

        void OnTriggerEnter(Collider other)
        {
            if (HasBeenReached) return;
            if (!other.TryGetComponent<PlayerSaveData>(out var playerSaveData)) return;
            // Trigger Checkpoint
            HasBeenReached = true;

            const float checkpointRespawnHeightOffset = 0.2f;
            var pos = transform.position + playerSaveData.transform.up * checkpointRespawnHeightOffset;
            var forward = -transform.right; // prefab is rotated in weird way, so left is forward
            playerSaveData.InjectedCheckpointPose = (pos, forward);
            SaveAndLoad.Instance.Save();
            playerSaveData.InjectedCheckpointPose = null;
        }
        
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, transform.position + transform.forward * 3);
        }
        
    #region Save and Load

        [Serializable]
        public struct SaveData
        {
            public bool HasBeenReached;
        }

        public SaveData Save() => new() { HasBeenReached = HasBeenReached };

        public void Load(SaveData data) => HasBeenReached = data.HasBeenReached;

        [field: SerializeField] public int SaveDataID { get; set; }

    #endregion
    }
}