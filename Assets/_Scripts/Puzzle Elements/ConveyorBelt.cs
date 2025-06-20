using System.Collections.Generic;
using System.Linq;
using GravityGame.Player;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    public class ConveyorBelt : MonoBehaviour
    {
        [SerializeField] float _speed;
        [SerializeField] float _nonPlayerSpeedModifier = 0.5f;
        readonly Dictionary<Rigidbody, RigidbodyConstraints> _onBelt = new();
        readonly Dictionary<Rigidbody, Quaternion> _settlingObjects = new();

        // Note FS: This is tested and looks the best
        const float SettleSpeed = 20f;
        const float TextureScrollMultiplier = 7f;

        Material _material;
        void Start() => _material = GetComponent<MeshRenderer>().material;

        void Update()
        {
            _material.mainTextureOffset += new Vector2(0, 1) * (_speed * TextureScrollMultiplier * Time.deltaTime);

            foreach (var rb in _settlingObjects.Keys.ToList().Where(rb => rb)) {
                rb.transform.rotation = Quaternion.Lerp(rb.transform.rotation, _settlingObjects[rb], SettleSpeed * Time.deltaTime);
                if (Quaternion.Angle(rb.transform.rotation, _settlingObjects[rb]) < 0.1f) {
                    rb.transform.rotation = _settlingObjects[rb];
                    _settlingObjects.Remove(rb);
                }
            }
        }

        void FixedUpdate()
        {
            foreach (var rb in _onBelt.Keys.Where(rb => rb)) {
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

                if (!rb.GetComponent<PlayerMovement>()) {
                    rb.constraints = RigidbodyConstraints.FreezeRotation;

                    var currentAngles = rb.transform.eulerAngles;
                    var targetAngles = new Vector3(
                        RoundToNearestAngle(currentAngles.x),
                        RoundToNearestAngle(currentAngles.y),
                        RoundToNearestAngle(currentAngles.z)
                    );
                    _settlingObjects.Add(rb, Quaternion.Euler(targetAngles));
                }
            }
        }

        void OnCollisionExit(Collision collision)
        {
            var rb = collision.rigidbody;
            if (rb) {
                if (_onBelt.TryGetValue(rb, out var originalConstraint)) {
                    rb.constraints = originalConstraint;
                    _onBelt.Remove(rb);
                }
                _settlingObjects.Remove(rb);
            }
        }

        static float RoundToNearestAngle(float angle) => Mathf.Round(angle / 90.0f) * 90.0f;
    }
}