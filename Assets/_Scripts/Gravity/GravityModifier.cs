using System;
using GravityGame.Player;
using GravityGame.Puzzle_Elements;
using GravityGame.SaveAndLoadSystem;
using UnityEngine;

namespace GravityGame.Gravity
{
    /// <summary>
    ///     GameObjects with this component have custom gravity
    ///     that can be manipulated by editing <see cref="GravityDirection" /> and <see cref="GravityMagnitude" />
    ///     <see cref="GravityDirection" /> is shared between all objects with the same <see cref="Group" />
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GravityModifier : MonoBehaviour, ISaveData<GravityModifier.SaveData>
    {
        [SerializeField] Vector3 _gravityDirection = Vector3.down;
        public Vector3 GravityDirection
        {
            get => _gravityDirection.normalized;
            set {
                if (value == _gravityDirection)
                    return;
                if (Group != GravityGroup.None)
                    GravityGroupHandler.AlertGravityGroup(Group, value);
                _gravityDirection = value;
            }
        }

        public float GravityMagnitude = 9.81f;

        public Vector3 Gravity => GravityDirection * GravityMagnitude;

        public enum GravityGroup { None, Player, Red, Blue, Green }

        public GravityGroup Group = GravityGroup.None;

        Rigidbody _rigidbody;

        void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            GravityGroupHandler.OnGravityGroupDirectionChange += SetGravityDirectionWithoutGroupAlert;
        }

        void OnDisable()
        {
            GravityGroupHandler.OnGravityGroupDirectionChange -= SetGravityDirectionWithoutGroupAlert;
        }

        void FixedUpdate()
        {
            if (GetComponent<PlayerMovement>() != null) return;
            _rigidbody.AddForce(GravityDirection.normalized * GravityMagnitude, ForceMode.Acceleration);
        }

        void SetGravityDirectionWithoutGroupAlert(GravityGroup gravityGroup, Vector3 gravityDirection)
        {
            if (gravityGroup == Group)
                _gravityDirection = gravityDirection;
        }

    #region Save and Load
        
        [Serializable]
        public struct SaveData
        {
            public Vector3 Gravity;
            public Vector3 Position;
            public Quaternion Rotation;
        }
        
        public SaveData Save() =>
            new() {
                Gravity = Gravity,
                Position = transform.position,
                Rotation = transform.rotation,
            };

        public void Load(SaveData data)
        {
            _rigidbody = GetComponent<Rigidbody>();
            GravityMagnitude = data.Gravity.magnitude;
            GravityDirection = data.Gravity.normalized;
            _rigidbody.MovePosition(data.Position);
            _rigidbody.MoveRotation(data.Rotation);
            if (!_rigidbody.isKinematic) {
                _rigidbody.linearVelocity = Vector3.zero;
                _rigidbody.angularVelocity = Vector3.zero;    
            }
        }
        
        [field: SerializeField] public int SaveDataID { get; set; }

        public bool ShouldBeSaved { get; set; } = true;

    #endregion
    }
}