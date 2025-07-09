using System;
using GravityGame.Gravity;
using GravityGame.SaveAndLoadSystem;
using GravityGame.Utils;
using UnityEngine;
using UnityEngine.Serialization;

namespace GravityGame.Player
{
    /// <summary>
    ///     Handles saving / loading of Player state
    /// </summary>
    public class PlayerSaveData : SingletonMonoBehavior<PlayerSaveData>, ISaveData<PlayerSaveData.SaveData>
    {
        GravityModifier GravityModifier => GetComponent<GravityModifier>();
        Rigidbody Rigidbody => GetComponent<Rigidbody>();
        FirstPersonCameraController Camera => GetComponentInChildren<FirstPersonCameraController>();

        public Func<SaveData, SaveData> InjectCheckpointData { get; set; }
        public int ActiveCheckpoint { get; set; }

        void OnEnable() => GravityModifier.ShouldBeSaved = false;

        [Serializable]
        public struct SaveData
        {
            public Vector3 Position;
            public float LookRight;
            public Vector3 Gravity;
            public int LastCheckpointID;
        }

        public SaveData Save()
        {
            var saveData = new SaveData {
                Position = transform.position,
                LookRight = Camera.LookRightRotation,
                Gravity = GravityModifier.GravityDirection,
                LastCheckpointID = 0
            };
            if (InjectCheckpointData != null) {
                saveData = InjectCheckpointData(saveData);
            } 
            return saveData;
        }

        public void Load(SaveData data)
        {
            Rigidbody.MovePosition(data.Position);
            Camera.LookRightRotation = data.LookRight;
            Camera.LookDownRotation = 0;
            GravityModifier.GravityDirection = data.Gravity;
            ActiveCheckpoint = data.LastCheckpointID;
            
            Rigidbody.linearVelocity = Vector3.zero;
            PlayerHealth.Instance.Heal(PlayerHealth.MaxHealth);
        }

        public int SaveDataID
        {
            get => 1;
            set { }
        }
    }
}