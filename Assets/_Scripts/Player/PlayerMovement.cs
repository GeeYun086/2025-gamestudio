using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Basic player ground movement including walking and jumping
    /// </summary>
    public class PlayerMovement : MonoBehaviour
    {
        [SerializeField] float _moveSpeedMps = 2.5f;
        [SerializeField] float _gravity = 20.0f;
        [SerializeField] float _jumpSpeed = 3.0f;
        [SerializeField] float _airMovementModifier = 0.5f;
        [SerializeField] float _friction = 5f;

        CharacterController _controller;
        float _horizontalMoveInput;
        float _verticalMoveInput;
        bool _jumping;
        bool _isGrounded;

        void Awake()
        {
            _controller = GetComponent<CharacterController>();
        }

        void Update()
        {
            var velocity = _controller.velocity;
            _horizontalMoveInput = Input.GetAxisRaw("Horizontal");
            _verticalMoveInput = Input.GetAxisRaw("Vertical");
            _jumping = Input.GetButton("Jump");
            _isGrounded = _controller.isGrounded;

            var groundMoveDirection = new Vector3(_horizontalMoveInput, 0, _verticalMoveInput);
            groundMoveDirection.Normalize();
            groundMoveDirection = transform.TransformDirection(groundMoveDirection);

            groundMoveDirection *= _moveSpeedMps * Time.deltaTime;

            var velocityXZ = new Vector3(velocity.x, 0, velocity.z);

            if (_isGrounded) {
                velocityXZ += -velocityXZ * (Time.deltaTime * _friction);
                velocityXZ += groundMoveDirection;

                velocity.x = velocityXZ.x;
                velocity.z = velocityXZ.z;

                if (_jumping) {
                    velocity.y = Mathf.Sqrt(_jumpSpeed * _gravity);
                } else {
                    velocity.y = -2f;
                }
            } else {
                float airFriction = _friction * 0.5f;
                velocityXZ += -velocityXZ * (Time.deltaTime * airFriction);
                velocityXZ += groundMoveDirection * _airMovementModifier;
                // var dirChangeFactor = Vector3.Angle(groundMoveDirection, velocityXZ) / 180f;
                // var currentMag = velocityXZ.magnitude;
                // var newMag = groundMoveDirection.magnitude;
                // velocityXZ = groundMoveDirection * Math.Max(currentMag, newMag);

                velocity.y -= _gravity * Time.deltaTime;

                velocity.x = velocityXZ.x;
                velocity.z = velocityXZ.z;
            }

            _controller.Move(velocity * Time.deltaTime);
        }
    }
}