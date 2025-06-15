using System.Collections.Generic;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] float _speed;
        [SerializeField] float _nonPlayerSpeedModifier = 0.5f;
        readonly Dictionary<Rigidbody, RigidbodyConstraints> _onBelt = new();

        Material _material;
        void Start() => _material = GetComponent<MeshRenderer>().material;

        // Note FS: This is tested and looks the best
        void Update() => _material.mainTextureOffset += new Vector2(0, 1) * (_speed * 7f * Time.deltaTime);

        void FixedUpdate()
        {
            foreach (var rb in _onBelt.Keys) {
                if (rb.GetComponent<PlayerMovement>()) {
                    rb.AddForce(_speed * transform.forward, ForceMode.VelocityChange);
                } else {
                    rb.AddForce(_nonPlayerSpeedModifier * _speed * transform.forward, ForceMode.VelocityChange);
                }
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            var rb = collision.rigidbody;
            if (rb && !_onBelt.ContainsKey(rb)) {
                _onBelt.Add(rb, rb.constraints);
                if (!rb.GetComponent<PlayerMovement>()) rb.constraints = RigidbodyConstraints.FreezeRotation;
            }
        }

        void OnCollisionExit(Collision collision)
        {
            var rb = collision.rigidbody;
            if (rb && _onBelt.TryGetValue(rb, out var originalConstraints)) {
                rb.constraints = originalConstraints;
                _onBelt.Remove(rb);
            }
        }
    }
}