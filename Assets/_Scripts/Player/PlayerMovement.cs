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
        [SerializeField] float _jumpForce = 5.0f;
        [SerializeField] float _airMovementModifier = 0.5f;

        [SerializeField] InputActionReference _moveInput;
        [SerializeField] InputActionReference _jumpInput;

        Rigidbody _rigidbody;
        CapsuleCollider _collider;
        bool _isGrounded;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            _rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
        }

        void FixedUpdate()
        {
            const float Margin = -0.05f;
            const float GroundDistance = 0.15f;
            var feetPosition = transform.position - (_collider.height * 0.5f + Margin) * transform.up;
            _isGrounded = Physics.Raycast(feetPosition, -transform.up, GroundDistance);
            Debug.DrawRay(feetPosition, -transform.up * GroundDistance, Color.red);

            var inputDirection = _moveInput.action.ReadValue<Vector2>();
            if (inputDirection != Vector2.zero)
                Move(inputDirection);
            if (_isGrounded && _jumpInput.action.IsPressed())
                Jump();
        }

        void Move(Vector2 direction)
        {
            var inputDir = direction.normalized;
            var velocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);

            var desiredVelocity = new Vector3(inputDir.x, 0, inputDir.y) * _moveSpeedMps;
            desiredVelocity = transform.TransformDirection(desiredVelocity);

            var velocityChange = desiredVelocity - velocity;

            if (_isGrounded) {
                velocityChange = Vector3.ClampMagnitude(velocityChange, _maxAcceleration);
            } else {
                // In air: only apply acceleration if trying to steer in a new direction
                if (Vector3.Dot(desiredVelocity.normalized, velocity.normalized) > 0.9f) {
                    if (desiredVelocity.magnitude < velocity.magnitude) {
                        velocityChange = Vector3.zero;
                    }
                }

                velocityChange = Vector3.ClampMagnitude(velocityChange, _maxAcceleration * _airMovementModifier);
            }
            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        void Jump()
        {
            _rigidbody.AddForce(transform.up * _jumpForce, ForceMode.VelocityChange);
        }
    }
}