using System.Collections.Generic;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Creates a conveyor belt effect, moving any Rigidbody objects that come into contact with it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] float _moveSpeed = 2.0f;
        [SerializeField] Vector3 _localMoveDirection = Vector3.forward;

        readonly List<Rigidbody> _rigidbodiesOnBelt = new();

        void OnValidate() => _localMoveDirection = _localMoveDirection.normalized;

        void FixedUpdate()
        {
            var worldMoveDirection = transform.TransformDirection(_localMoveDirection.normalized);
            _rigidbodiesOnBelt.RemoveAll(item => !item);
            foreach (var rb in _rigidbodiesOnBelt) {
                var movementDelta = worldMoveDirection * (_moveSpeed * Time.fixedDeltaTime);
                rb.MovePosition(rb.position + movementDelta);
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            var rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb && !_rigidbodiesOnBelt.Contains(rb)) _rigidbodiesOnBelt.Add(rb);
        }

        void OnCollisionExit(Collision collision)
        {
            var rb = collision.gameObject.GetComponent<Rigidbody>();
            if (rb) _rigidbodiesOnBelt.Remove(rb);
        }
    }
}