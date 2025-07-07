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
        public event Action OnHasBeenReachedChanged;

        public bool HasBeenReached { get; private set; }

        public bool IsActiveCheckpoint => PlayerSaveData.Instance.ActiveCheckpoint == SaveDataID;

        void OnEnable()
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
            playerSaveData.ActiveCheckpoint = SaveDataID;

            playerSaveData.InjectCheckpointData = InjectCheckpointData;
            SaveAndLoad.Instance.Save();
            playerSaveData.InjectCheckpointData = null;
            
            OnHasBeenReachedChanged?.Invoke();
            return;

            PlayerSaveData.SaveData InjectCheckpointData(PlayerSaveData.SaveData saveData)
            {
                const float checkpointRespawnHeightOffset = 0.2f;
                var pos = transform.position + transform.up * checkpointRespawnHeightOffset;
                var forward = -transform.right; // prefab is rotated in weird way, so left is forward
                saveData.Position = pos;
                saveData.LookRight = Vector3.SignedAngle(playerSaveData.transform.forward, forward, playerSaveData.transform.up);
                saveData.LastCheckpointID = SaveDataID;
                return saveData;
            }
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

        public SaveData Save() => new() { HasBeenReached = HasBeenReached};

        public void Load(SaveData data) => HasBeenReached = data.HasBeenReached;

        public void OnAfterLoad() => OnHasBeenReachedChanged?.Invoke();

        [field: SerializeField] public int SaveDataID { get; set; }

    #endregion
    }
}