using GravityGame.Puzzle_Elements;
using UnityEngine;

namespace GravityGame.Gravity
{
    /// <summary>
    ///     GameObjects with this component have custom gravity
    ///     that can be manipulated by editing <see cref="GravityDirection" /> and <see cref="GravityMagnitude" />
    ///     <see cref="GravityDirection" /> is shared between all objects with the same <see cref="Color" />
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class GravityModifier : MonoBehaviour
    {
        public Vector3 GravityDirection
        {
            get => _gravityDirection;
            set {
                if (value == _gravityDirection)
                    return;
                ColorGroupHandler.Instance.AlertColorGroup(Color, value);
                _gravityDirection = value;
            }
        }
        Vector3 _gravityDirection = Vector3.down;
        public float GravityMagnitude = 9.81f;
        public ColorGroup Color = ColorGroup.None;

        public enum ColorGroup { None, Red, Blue, Green }

        Rigidbody _rigidbody;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            SubscribeToCubeGroup();
        }

        void FixedUpdate()
        {
            _rigidbody.AddForce(GravityDirection.normalized * GravityMagnitude, ForceMode.Acceleration);
        }

        void SetGravityDirectionWithoutGroupAlert(Vector3 gravityDirection)
        {
            _gravityDirection = gravityDirection;
        }

        void SubscribeToCubeGroup()
        {
            switch (Color) {
                case ColorGroup.Red:
                    ColorGroupHandler.Instance.OnRedGroupGravityDirectionChange += SetGravityDirectionWithoutGroupAlert;
                    break;
                case ColorGroup.Blue:
                    ColorGroupHandler.Instance.OnBlueGroupGravityDirectionChange += SetGravityDirectionWithoutGroupAlert;
                    break;
                case ColorGroup.Green:
                    ColorGroupHandler.Instance.OnGreenGroupGravityDirectionChange += SetGravityDirectionWithoutGroupAlert;
                    break;
            }
        }

        void OnDestroy()
        {
            UnsubscribeFromCubeGroups();
        }

        void UnsubscribeFromCubeGroups()
        {
            ColorGroupHandler.Instance.OnRedGroupGravityDirectionChange -= SetGravityDirectionWithoutGroupAlert;
            ColorGroupHandler.Instance.OnBlueGroupGravityDirectionChange -= SetGravityDirectionWithoutGroupAlert;
            ColorGroupHandler.Instance.OnGreenGroupGravityDirectionChange -= SetGravityDirectionWithoutGroupAlert;
        }
    }
}