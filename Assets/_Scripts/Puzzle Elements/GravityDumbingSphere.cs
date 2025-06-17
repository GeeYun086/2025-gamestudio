using System.Collections.Generic;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Creates a spherical area of effect that dampens the movement of dynamic Rigidbodies
    /// entering its trigger zone. Includes a visual representation of the effect radius.
    /// </summary>
    [RequireComponent(typeof(SphereCollider))]
    public class GravityDumbingSphere : MonoBehaviour
    {
        [SerializeField] float _effectRadius = 10f;
        [SerializeField] LayerMask _affectableLayers = ~0;
        [SerializeField] [Range(0f, 1f)] float _movementDampening = 0.1f;
        [SerializeField] Material _effectSphereMaterial;

        SphereCollider _triggerCollider;
        readonly List<Rigidbody> _affectedRigidbodies = new();
        Transform _visualEffectSphereTransform;

        const string VisualSphereName = nameof(GravityDumbingSphere);

        void Awake()
        {
            InitializeTriggerCollider();
            InitializeVisualSphere();
        }

        void FixedUpdate()
        {
            for (var i = _affectedRigidbodies.Count - 1; i >= 0; i--) {
                var rb = _affectedRigidbodies[i];
                if (!rb || rb.isKinematic) {
                    _affectedRigidbodies.RemoveAt(i);
                    continue;
                }

                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, _movementDampening);
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, _movementDampening);
            }
        }

        void OnTriggerEnter(Collider other)
        {
            if (!IsLayerAffectable(other.gameObject.layer)) return;
            var rb = other.GetComponentInParent<Rigidbody>();
            if (rb && !rb.isKinematic && !_affectedRigidbodies.Contains(rb)) _affectedRigidbodies.Add(rb);
        }

        void OnTriggerExit(Collider other)
        {
            if (!IsLayerAffectable(other.gameObject.layer)) return;
            var rb = other.GetComponentInParent<Rigidbody>();
            if (rb) _affectedRigidbodies.Remove(rb);
        }

        void OnValidate()
        {
            InitializeTriggerCollider();
            if (gameObject.scene.isLoaded || transform.Find(VisualSphereName)) InitializeVisualSphere();
        }

        void InitializeTriggerCollider()
        {
            if (!_triggerCollider) _triggerCollider = GetComponent<SphereCollider>();
            _triggerCollider.isTrigger = true;
            _triggerCollider.radius = _effectRadius;
        }

        void InitializeVisualSphere()
        {
            _visualEffectSphereTransform = transform.Find(VisualSphereName);
            
            if (!_visualEffectSphereTransform) {
                var sphereGo = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                sphereGo.name = VisualSphereName;
                sphereGo.transform.SetParent(transform);
                sphereGo.transform.localPosition = Vector3.zero;
                sphereGo.transform.localRotation = Quaternion.identity;
                _visualEffectSphereTransform = sphereGo.transform;
            }

            var visualCollider = _visualEffectSphereTransform.GetComponent<Collider>();
            if (visualCollider) visualCollider.enabled = false;

            var visualRenderer = _visualEffectSphereTransform.GetComponent<MeshRenderer>();
            if (visualRenderer) visualRenderer.sharedMaterial = _effectSphereMaterial;

            _visualEffectSphereTransform.localScale = Vector3.one * Mathf.Max(0f, _effectRadius * 2f);
        }

        bool IsLayerAffectable(int layer)
            => (_affectableLayers.value & 1 << layer) > 0;
    }
}