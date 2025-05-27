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

            foreach (var rb in _rigidbodiesOnBelt) rb.linearVelocity = _direction * _moveSpeed;
            ;

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