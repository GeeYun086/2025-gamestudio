using System;
using GravityGame.Gravity;
using GravityGame.SaveAndLoadSystem;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Handles saving / loading of Player state
    /// </summary>
    public class PlayerSaveData : MonoBehaviour, ISaveData<PlayerSaveData.SaveData>
    {
        GravityModifier GravityModifier => GetComponent<GravityModifier>();
        Rigidbody Rigidbody => GetComponent<Rigidbody>();
        FirstPersonCameraController Camera => GetComponentInChildren<FirstPersonCameraController>();

        public (Vector3 position, Vector3 forward)? InjectedCheckpointPose { get; set; }

        void OnEnable() => GravityModifier.ShouldBeSaved = false;

        [Serializable]
        public struct SaveData
        {
            public Vector3 Position;
            public float LookRight;
            public Vector3 Up;
        }

        public SaveData Save()
        {
            var saveData = new SaveData();
            if (InjectedCheckpointPose is { } pose) {
                saveData.Position = pose.position;
                saveData.LookRight = Vector3.SignedAngle(transform.forward, pose.forward, transform.up);
            } else {
                saveData.Position = transform.position;
                saveData.LookRight = Camera.LookRightRotation;
            }
            saveData.Up = transform.up;
            return saveData;
        }

        public void Load(SaveData data)
        {
            Rigidbody.MovePosition(data.Position);
            Camera.LookRightRotation = data.LookRight;
            Camera.LookDownRotation = 0;
            transform.up = data.Up;
            Rigidbody.linearVelocity = Vector3.zero;
        }

        public int SaveDataID
        {
            get => 1;
            set { }
        }
    }
}