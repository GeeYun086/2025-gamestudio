using System.Collections.Generic;
using UnityEngine;

namespace GravityGame.Mover
{
    [RequireComponent(typeof(SphereCollider))]
    public class BlackHole : MonoBehaviour
    {
        [SerializeField] float effectRadius = 10f;
        [SerializeField] LayerMask affectableLayers;
        [SerializeField] [Range(0f, 1f)] float movementDampening = 0.1f;
        [SerializeField] Material effectSphereMaterial;

        SphereCollider _triggerCollider;
        readonly List<Rigidbody> _affectedRigidbodies = new();
        Transform _visualEffectSphereTransform;

        void Awake()
        {
            _triggerCollider = GetComponent<SphereCollider>();
            if (!_triggerCollider) _triggerCollider = gameObject.AddComponent<SphereCollider>();
            _triggerCollider.isTrigger = true;
            _triggerCollider.radius = effectRadius;

            SetupInGameVisualSphere();
        }

        void FixedUpdate()
        {
            for (int i = _affectedRigidbodies.Count - 1; i >= 0; i--) {
                var rb = _affectedRigidbodies[i];

                if (!rb || rb.isKinematic) {
                    _affectedRigidbodies.RemoveAt(i);
                    continue;
                }

                if (!(movementDampening > 0f)) continue;
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, movementDampening);
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, movementDampening);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if ((affectableLayers.value & (1 << other.gameObject.layer)) <= 0) return;

            var rb = other.GetComponentInParent<Rigidbody>();
            if (rb && !rb.isKinematic && !_affectedRigidbodies.Contains(rb)) _affectedRigidbodies.Add(rb);
        }

        void OnTriggerExit(Collider other)
        {
            var rb = other.GetComponentInParent<Rigidbody>();
            if (rb && _affectedRigidbodies.Contains(rb)) _affectedRigidbodies.Remove(rb);
        }

        void SetupInGameVisualSphere()
        {
            var existingManagedVisual = transform.Find("EffectSphereVisual_ManagedByBlackHole");
            if (existingManagedVisual) {
                _visualEffectSphereTransform = existingManagedVisual;
            } else {
                var sphereGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereGo.name = "EffectSphereVisual_ManagedByBlackHole";
                sphereGo.transform.SetParent(transform);
                sphereGo.transform.localPosition = Vector3.zero;
                sphereGo.transform.localRotation = Quaternion.identity;
                var primitiveCollider = sphereGo.GetComponent<Collider>();
                if (primitiveCollider) {
                    if (Application.isPlaying) Destroy(primitiveCollider);
                    else DestroyImmediate(primitiveCollider);
                }
                _visualEffectSphereTransform = sphereGo.transform;
            }

            var visualRenderer = _visualEffectSphereTransform.GetComponent<MeshRenderer>();
            visualRenderer.sharedMaterial = effectSphereMaterial;

            UpdateVisualSphereScale();
        }

        void UpdateVisualSphereScale()
        {
            if (!_visualEffectSphereTransform) return;
            float targetScale = Mathf.Max(0, effectRadius * 2f);
            _visualEffectSphereTransform.localScale = Vector3.one * targetScale;
        }

        void OnValidate()
        {
            if (!_triggerCollider) _triggerCollider = GetComponent<SphereCollider>();
            if (_triggerCollider) {
                _triggerCollider.isTrigger = true;
                _triggerCollider.radius = Mathf.Max(0, effectRadius);
            }
            if (effectRadius < 0) effectRadius = 0;
        }
    }
}