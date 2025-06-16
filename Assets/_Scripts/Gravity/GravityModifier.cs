using GravityGame.Player;
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
        public Vector3 Gravity => GravityDirection * GravityMagnitude;

        public Vector3 GravityDirection
        {
            get => _gravityDirection.normalized;
            set {
                if (value == _gravityDirection)
                    return;
                if (Group != GravityGroup.None)
                    GravityGroupHandler.Instance.AlertGravityGroup(Group, value);
                _gravityDirection = value;
            }
        }

        [SerializeField] Vector3 _gravityDirection = Vector3.down;
        public float GravityMagnitude = 9.81f;
        public GravityGroup Group = GravityGroup.None;

        public enum GravityGroup { None, Player, Red, Blue, Green }

        Rigidbody _rigidbody;

        void OnEnable()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            GravityGroupHandler.Instance.OnGravityGroupDirectionChange += SetGravityDirectionWithoutGroupAlert;
        }

        void OnDisable()
        {
            GravityGroupHandler.Instance.OnGravityGroupDirectionChange -= SetGravityDirectionWithoutGroupAlert;
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
    }
}