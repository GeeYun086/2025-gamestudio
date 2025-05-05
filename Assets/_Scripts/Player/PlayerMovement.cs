using UnityEngine;
using UnityEngine.InputSystem;

namespace GravityGame.Player
{
    /// <summary>
    ///     Basic player ground movement including walking and jumping
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float _moveSpeedMps = 2.5f;
        [SerializeField] float _maxAcceleration = 2.5f;
        [SerializeField] float _jumpSpeed = 3.0f;
        [SerializeField] float _airMovementModifier = 0.5f;

        [SerializeField] InputActionReference _moveInput;
        [SerializeField] InputActionReference _jumpInput;
        Rigidbody _rigidbody;
        bool _jumping;
        bool _isGrounded;
        CapsuleCollider _collider;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            // ReSharper disable twice BitwiseOperatorOnEnumWithoutFlags
            _rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
        }

        void FixedUpdate()
        {
            const float Margin = -0.05f;
            const float GroundDistance = 0.2f;
            var feetPosition = transform.position - (_collider.height * 0.5f + Margin) * transform.up;
            _isGrounded = Physics.Raycast(feetPosition, -transform.up, GroundDistance);
            Debug.DrawRay(feetPosition, -transform.up * GroundDistance, Color.red);
            Move();
        }

        void Move()
        {
            var moveDirection = _moveInput.action.ReadValue<Vector2>();
            moveDirection = moveDirection.normalized * _moveSpeedMps;
            if (!_isGrounded) {
                moveDirection *= _airMovementModifier;
            }
            var velocity = new Vector3(moveDirection.x, _rigidbody.linearVelocity.y, moveDirection.y);
            velocity = transform.TransformDirection(velocity);

            var velocityChange = velocity - _rigidbody.linearVelocity;
            velocityChange = Vector3.ClampMagnitude(velocityChange, _maxAcceleration);

            _jumping = _jumpInput.action.IsPressed();
            if (_isGrounded && _jumping) {
                velocityChange.y = _jumpSpeed;
            }

            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }
    }
}