using GravityGame.Puzzle_Elements;
using UnityEngine;

namespace GravityGame.Gravity
{
    /// <summary>
    ///     GameObjects with this component have custom gravity
    ///     that can be manipulated by editing <see cref="GravityDirection" /> and <see cref="GravityMagnitude" />
    ///     <see cref="GravityDirection" /> is shared between all objects with the same <see cref="Group" />
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GravityModifier : MonoBehaviour
    {
        public virtual Vector3 GravityDirection
        {
            get => _gravityDirection;
            set {
                if (value == _gravityDirection)
                    return;
                if(Group != GravityGroup.None && GravityGroupHandler.Instance != null) // Added null check for Instance
                    GravityGroupHandler.Instance.AlertGravityGroup(Group, value);
                _gravityDirection = value;
            }
        }
        
        Vector3 _gravityDirection = Vector3.down;
        public float GravityMagnitude = 9.81f;
        public GravityGroup Group = GravityGroup.None;

        public enum GravityGroup { None, Red, Blue, Green }

        Rigidbody _rigidbody;

        protected virtual void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            if(Group != GravityGroup.None)
                GravityGroupHandler.Instance.OnGravityGroupDirectionChange += SetGravityDirectionWithoutGroupAlert;
        }

        protected virtual void FixedUpdate()
        {
            _rigidbody.AddForce(GravityDirection.normalized * GravityMagnitude, ForceMode.Acceleration);
        }

        void SetGravityDirectionWithoutGroupAlert(GravityGroup gravityGroup, Vector3 gravityDirection)
        {
            if (gravityGroup == Group)
                _gravityDirection = gravityDirection;
        }

        protected virtual void OnDestroy()
        {
            GravityGroupHandler.Instance.OnGravityGroupDirectionChange -= SetGravityDirectionWithoutGroupAlert;
        }
    }
}