using System.Collections.Generic;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] float _speed = 1f;
        [SerializeField] float _acceleration = 10f;
        [SerializeField] float _friction = 5.0f;
        [SerializeField] float _angularFriction = 5.0f;
        [SerializeField] float _textureScrollMultiplier = 1.3f; // Note FS: This is tested and looks the best
        Material _material;

        public Vector3 Velocity => _speed * transform.forward;
        void Start() => _material = GetComponent<MeshRenderer>().material;

        void Update()
        {
            _material.mainTextureOffset += new Vector2(0, 1) * (_speed * _textureScrollMultiplier * Time.deltaTime);
        }

        void OnCollisionStay(Collision collision)
        {
            var rb = collision.rigidbody;
            if (rb.GetComponent<PlayerMovement>()) {
                // handled in PlayerMovement
            } else {
                var currentVelocity = rb.linearVelocity;

                // friction
                float velocityInBeltDirection = Vector3.Dot(currentVelocity, transform.forward);
                var velocityAffectedByFriction = Vector3.ProjectOnPlane(
                    currentVelocity - Mathf.Clamp(velocityInBeltDirection, 0, _speed) * transform.forward, transform.up
                );
                var newVelocityAffectedByFriction = Vector3.MoveTowards(velocityAffectedByFriction, Vector3.zero, _friction * Time.fixedDeltaTime);
                
                rb.AddForce(newVelocityAffectedByFriction - velocityAffectedByFriction, ForceMode.VelocityChange);

                // acceleration
                if (velocityInBeltDirection < _speed) {
                    float newVelocityInBeltDir = Mathf.MoveTowards(velocityInBeltDirection, _speed, _acceleration * Time.fixedDeltaTime);
                    rb.AddForce(transform.forward * (newVelocityInBeltDir - velocityInBeltDirection), ForceMode.VelocityChange);
                }

                // angular friction
                var angularVelocity = rb.angularVelocity;
                angularVelocity = Vector3.MoveTowards(angularVelocity, Vector3.zero, _angularFriction * Time.fixedDeltaTime);
                rb.AddTorque(angularVelocity - rb.angularVelocity, ForceMode.VelocityChange);
            }
        }
    }
}