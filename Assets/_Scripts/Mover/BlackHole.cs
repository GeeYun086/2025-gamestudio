using System.Collections.Generic;
using UnityEngine;

namespace GravityGame.Mover
{
    [RequireComponent(typeof(SphereCollider))]
    public class BlackHole : MonoBehaviour
    {
        [SerializeField] float effectRadius = 10f;
        [SerializeField] LayerMask affectableLayers = ~0;
        [SerializeField] [Range(0f, 1f)] float movementDampening = 0.1f;
        [SerializeField] Material effectSphereMaterial;

        SphereCollider _triggerCollider;
        readonly List<Rigidbody> _affectedRigidbodies = new();
        Transform _visualEffectSphereTransform;

        const string VisualSphereName = "BlackHoleEffectSphere";
        static int _ignoreRaycastLayer = -1;

        void Awake()
        {
            InitializeTriggerCollider();
            InitializeVisualSphere();
        }

        void FixedUpdate()
        {
            for (int i = _affectedRigidbodies.Count - 1; i >= 0; i--) {
                var rb = _affectedRigidbodies[i];
                if (!rb || rb.isKinematic) {
                    _affectedRigidbodies.RemoveAt(i);
                    continue;
                }
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, Vector3.zero, movementDampening);
                rb.angularVelocity = Vector3.Lerp(rb.angularVelocity, Vector3.zero, movementDampening);
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
            if (!_triggerCollider) {
                _triggerCollider = GetComponent<SphereCollider>();
                // _triggerCollider = gameObject.AddComponent<SphereCollider>();
            }
            _triggerCollider.isTrigger = true;
            _triggerCollider.radius = effectRadius;
        }

        void InitializeVisualSphere()
        {
            if (_ignoreRaycastLayer == -1) _ignoreRaycastLayer = LayerMask.NameToLayer("Ignore Raycast");

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
            if (_visualEffectSphereTransform) _visualEffectSphereTransform.gameObject.layer = _ignoreRaycastLayer;

            var visualRenderer = _visualEffectSphereTransform.GetComponent<MeshRenderer>();
            if (visualRenderer) visualRenderer.sharedMaterial = effectSphereMaterial;

            _visualEffectSphereTransform.localScale = Vector3.one * Mathf.Max(0f, effectRadius * 2f);
        }

        bool IsLayerAffectable(int layer) => (affectableLayers.value & 1 << layer) > 0;
    }
}