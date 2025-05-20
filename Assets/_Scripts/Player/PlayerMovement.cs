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
        [SerializeField] AnimationCurve _accelerationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] float _jumpBufferTime = 0.15f;

        [SerializeField] InputActionReference _moveInput;
        [SerializeField] InputActionReference _jumpInput;

        Rigidbody _rigidbody;
        CapsuleCollider _collider;
        bool _isGrounded;
        float _jumpBufferTimer;

        void Awake()
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collider = GetComponent<CapsuleCollider>();

            _rigidbody.constraints |= RigidbodyConstraints.FreezeRotation;
        }

        void FixedUpdate()
        {
            const float margin = -0.05f;
            const float groundDistance = 0.15f;
            var feetPosition = transform.position - (_collider.height * 0.5f + margin) * transform.up;
            _isGrounded = Physics.Raycast(feetPosition, -transform.up, groundDistance);
            Debug.DrawRay(feetPosition, -transform.up * groundDistance, Color.red);

            var inputDirection = _moveInput.action.ReadValue<Vector2>();
            if (inputDirection != Vector2.zero)
                Move(inputDirection);
            if (_jumpBufferTimer < _jumpBufferTime)
                _jumpBufferTimer += Time.fixedDeltaTime;
            if (_jumpInput.action.IsPressed())
                _jumpBufferTimer = 0;
            if (_isGrounded && _jumpBufferTimer < _jumpBufferTime)
                Jump();
        }

        void Move(Vector2 direction)
        {
            var inputDir = direction.normalized;
            var velocity = new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);

            var desiredVelocity = new Vector3(inputDir.x, 0, inputDir.y) * _moveSpeedMps;
            desiredVelocity = transform.TransformDirection(desiredVelocity);
            
            // Determine angle of the ground, and project the desired velocity onto the ground plane (For movement on slopes)
            const int groundLayerMask = 1 << 0;
            RaycastHit hit;
            Vector3 groundNormal = Vector3.up;
            if (Physics.Raycast(transform.position, -transform.up, out hit, _collider.height * 0.5f + 0.3f, groundLayerMask))
            {
                groundNormal = hit.normal;
            }
            desiredVelocity = Vector3.ProjectOnPlane(desiredVelocity, groundNormal);

            var velocityChange = desiredVelocity - velocity;

            if (_isGrounded) {
                float diff = velocityChange.magnitude / _moveSpeedMps;
                float curveValue = _accelerationCurve.Evaluate(diff);
                float clampedAccel = _maxAcceleration * curveValue;
                velocityChange = Vector3.ClampMagnitude(velocityChange, clampedAccel);
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