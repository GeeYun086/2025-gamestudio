using UnityEngine;

namespace GravityGame.RespawnSystem
{
    public struct CheckpointData
    {
        public readonly string ID;
        public readonly string Name;
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly bool HasBeenActivated;
        public readonly bool IsCurrentRespawnTarget;

        public CheckpointData(Checkpoint checkpoint, bool isCurrentRespawnTarget)
        {
            ID = checkpoint.CheckpointID;
            Name = checkpoint.gameObject.name;
            Position = checkpoint.transform.position;
            Rotation = checkpoint.transform.rotation;
            HasBeenActivated = checkpoint.HasBeenReached;
            IsCurrentRespawnTarget = isCurrentRespawnTarget;
        }
    }
}