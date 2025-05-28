using System.Collections.Generic;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Creates a conveyor belt effect, moving any Rigidbody objects that come into contact with it, and animates its material's texture.
    /// </summary>
    [RequireComponent(typeof(Collider), typeof(Renderer))]
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] float _moveSpeed = 2.0f;
        [SerializeField] Vector3 _direction = Vector3.forward;

        readonly List<Rigidbody> _rigidbodiesOnBelt = new();

        Material _material;
        Vector2 _textureOffset = Vector2.zero;
        const string BaseMap = "_BaseMap";

        void Awake()
        {
            var rendererComponent = GetComponent<Renderer>();
            if (!rendererComponent) {
                Debug.LogError("ConveyorBelt requires a renderer", this);
                enabled = false;
                return;
            }
            _material = rendererComponent.material;
        }


        void OnValidate() => _direction = _direction.normalized;

        void FixedUpdate()
        {
            _rigidbodiesOnBelt.RemoveAll(rb => !rb || !rb.gameObject.activeInHierarchy);
            var worldMoveDirection = transform.TransformDirection(_direction);

            foreach (var rb in _rigidbodiesOnBelt) {
                var velocityDifference = worldMoveDirection * _moveSpeed - Vector3.Project(rb.linearVelocity, worldMoveDirection);
                if (Vector3.Dot(velocityDifference, worldMoveDirection) > 0.01f) rb.AddForce(velocityDifference * 25f, ForceMode.Acceleration);
            }

            if (_material && _moveSpeed != 0) {
                float scrollSpeed = _moveSpeed * Time.fixedDeltaTime;
                var uvScroll = new Vector2(
                    _direction.x * scrollSpeed,
                    _direction.z * scrollSpeed
                );

                _textureOffset += uvScroll;
                _textureOffset.x %= 1.0f;
                _textureOffset.y %= 1.0f;
                _material.SetTextureOffset(Shader.PropertyToID(BaseMap), _textureOffset);
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

        void OnDestroy()
        {
            if (_material) {
                Destroy(_material);
                _material = null;
            }
        }
    }
}