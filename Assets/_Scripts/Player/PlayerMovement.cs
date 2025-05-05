using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

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

            var inputDirection = _moveInput.action.ReadValue<Vector2>();
            if (inputDirection != Vector2.zero)
                Move(inputDirection);
            if (_isGrounded && _jumpInput.action.IsPressed())
                Jump();
        }

        void Move(Vector2 direction)
        {
            if (!_isGrounded)
                return;
            var moveVector = direction.normalized * _moveSpeedMps;

            var velocity = new Vector3(moveVector.x, 0, moveVector.y);
            velocity = transform.TransformDirection(velocity);

            var velocityChange = velocity - new Vector3(_rigidbody.linearVelocity.x, 0, _rigidbody.linearVelocity.z);
            var maxVelocityChange = _isGrounded ? _maxAcceleration : _airMovementModifier;
            velocityChange = Vector3.ClampMagnitude(velocityChange, maxVelocityChange);

            _rigidbody.AddForce(velocityChange, ForceMode.VelocityChange);
        }

        void Jump()
        {
            _rigidbody.AddForce(transform.up * _jumpForce, ForceMode.VelocityChange);
        }
    }
}