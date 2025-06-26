using System;
using System.Collections;
using System.Collections.Generic;
using GravityGame.Gravity;
using GravityGame.UI;
using GravityGame.Utils;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using Cursor = UnityEngine.Cursor;

namespace GravityGame.Player
{
    /// <summary>
    ///     Allows the player to change the gravity of <see cref="GravityModifier" /> objects by
    ///     1. looking at them and
    ///     2. selecting a direction from a radial menu
    /// </summary>
    public class GravityDirectionInput : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] Transform _playerTransform;
        [SerializeField] Camera _camera;
        [SerializeField] Axes _visualizationAxes;
        
        [Header("Input")]
        [SerializeField] InputActionReference _startGravityInput;
        [SerializeField] InputActionReference _cancelGravityInput;
        
        [Header("Parameters")]
        [SerializeField] float _maxObjectRange = 30;
        [SerializeField] Timer _aimBuffer = new(0.25f);

        [Header("Preview")]
        [SerializeField] Material _previewMaterial;
        [SerializeField] float _previewDistance = 4f;
        [SerializeField] float _previewCycleDuration = 1.5f;

        static GravityDirectionRadialMenu GravityChangeMenu => GameUI.Instance.Elements.GravityDirectionRadialMenu;
        
        GravityModifier _target;
        bool _tryChangingGravity;

        Coroutine _previewCoroutine;
        [CanBeNull] GameObject _previewCloneInstance;
        Vector3 _currentPreviewDirection;

        void Update()
        {
            bool wasChangingGravity = GravityChangeMenu.visible;

            // Switch Target
            bool canSwitchTarget = !wasChangingGravity || !_target;
            if (canSwitchTarget) {
                var hit = RaycastForSelectableObject();
                var last = _target;
                if (hit) {
                    _target = hit;
                    _aimBuffer.Start();
                } else if (!_aimBuffer.IsActive) {
                    _target = null;
                }
                if (_target != last) {
                    if (_target) ToggleOutlineOnObject(_target.gameObject, 1);
                    if (last) ToggleOutlineOnObject(last.gameObject, 0);
                }
            }

            // Update Input
            var start = _startGravityInput.action.WasPressedThisFrame();
            var stopped = !_startGravityInput.action.IsPressed();
            var canceled = _cancelGravityInput.action.WasPressedThisFrame();
            if (start) {
                _tryChangingGravity = true;
            }
            if (stopped || canceled) {
                _tryChangingGravity = false;
            }

            // Start / Stop changing gravity
            if (_tryChangingGravity) {
                if (_target && !wasChangingGravity) {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    GravityChangeMenu.visible = true;
                }
            } else {
                if (wasChangingGravity) {
                    var direction = GetRadialMenuGravityDirection();
                    if (_target && direction.HasValue && !canceled) {
                        _target.GravityDirection = direction.Value;
                    }
                    
                    _aimBuffer.Stop();
                    StopGravityPreview();
                    
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    GravityChangeMenu.visible = false;
                }
            }

            // Update while changing gravity
            bool isChangingGravity = GravityChangeMenu.visible && _target;
            if (isChangingGravity) {
                ToggleOutlineOnObject(_target.gameObject, 1);

                var direction = GetRadialMenuGravityDirection() ?? _target.GravityDirection;
                SetVisualizedDirection(direction);

                if (!_previewCloneInstance || _currentPreviewDirection != direction) {
                    StartGravityPreview(_target, direction);
                    _currentPreviewDirection = direction;
                }
            } 
        }

        void OnEnable() => SetVisualizedDirection(Vector3.zero);
        void OnDisable() => StopGravityPreview();
        void OnDestroy() => StopGravityPreview();

        void StartGravityPreview(GravityModifier originalObjectToPreview, Vector3 direction)
        {
            SetVisualizedDirection(direction);
            StopGravityPreview();
            _previewCloneInstance = Instantiate(
                originalObjectToPreview.gameObject,
                originalObjectToPreview.transform.position,
                originalObjectToPreview.transform.rotation
            );
            _currentPreviewDirection = direction;

            if (_previewCloneInstance) {
                _previewCloneInstance.GetComponent<Rigidbody>().isKinematic = true;
                _previewCloneInstance.GetComponent<GravityModifier>().enabled = false;
                _previewCloneInstance.GetComponent<Renderer>().SetMaterials(new List<Material> { _previewMaterial });
                foreach (var renderer in _previewCloneInstance.GetComponentsInChildren<Renderer>()) {
                    renderer.SetMaterials(new List<Material> { _previewMaterial });
                }
                _previewCloneInstance.transform.localScale *= .999f;
                foreach (var component in _previewCloneInstance.GetComponentsInChildren<Collider>()) {
                    component.enabled = false;
                }
                _previewCoroutine = StartCoroutine(
                    PreviewGravityMovementRoutine(_previewCloneInstance.transform, originalObjectToPreview.transform, direction)
                );
            }
        }

        void StopGravityPreview()
        {
            SetVisualizedDirection(Vector3.zero);
            
            if (_previewCoroutine != null) {
                StopCoroutine(_previewCoroutine);
                _previewCoroutine = null;
            }

            if (_previewCloneInstance) {
                Destroy(_previewCloneInstance);
                _previewCloneInstance = null;
            }
        }

        IEnumerator PreviewGravityMovementRoutine(Transform clone, Transform original, Vector3 gravityDirection)
        {
            while (true) {
                if (!clone || !original) yield break;

                clone.position = original.position;
                clone.rotation = original.rotation;

                float timer = 0f;
                while (timer < _previewCycleDuration) {
                    clone.position = Vector3.Lerp(
                        original.position,
                        original.position + gravityDirection.normalized * _previewDistance,
                        timer / _previewCycleDuration
                    );
                    timer += Time.deltaTime;
                    yield return null;
                }
                clone.position = original.position + gravityDirection.normalized * _previewDistance;
            }
        }

        [CanBeNull]
        GravityModifier RaycastForSelectableObject()
        {
            var ray = new Ray(_camera.transform.position, _camera.transform.forward);
            // Note TG: other objects may block the hit, maybe need to ignore more layers in the future
            int layerMask = ~LayerMask.GetMask("AxisGizmo", "Player");

            if (Physics.Raycast(ray, out var hitInfo, _maxObjectRange, layerMask, QueryTriggerInteraction.Ignore)) {
                if (hitInfo.transform.gameObject.TryGetComponent<GravityModifier>(out var g)) 
                    return g;
            }
            return null;
        }

        Vector3? GetRadialMenuGravityDirection()
        {
            var screenCenter = _camera.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var mouseOffset = Input.mousePosition - screenCenter;
            var dir = GravityChangeMenu.GetDirection(mouseOffset);
            
            var up = GetClosestCardinalDirection(_playerTransform.up);
            var right = GetClosestCardinalDirection(_camera.transform.right);
            var forward = Vector3.Cross(right, up).normalized;

            return dir switch {
                GravityDirectionRadialMenu.Zone.None => null,
                GravityDirectionRadialMenu.Zone.Left => -right,
                GravityDirectionRadialMenu.Zone.Right => right,
                GravityDirectionRadialMenu.Zone.Up => up,
                GravityDirectionRadialMenu.Zone.Down => -up,
                GravityDirectionRadialMenu.Zone.OuterUp => forward,
                GravityDirectionRadialMenu.Zone.OuterDown => -forward,
                _ => throw new ArgumentOutOfRangeException()
            };
        }

        void SetVisualizedDirection(Vector3 direction)
        {
            _visualizationAxes.Up.SetActive(false);
            _visualizationAxes.Down.SetActive(false);
            _visualizationAxes.Left.SetActive(false);
            _visualizationAxes.Right.SetActive(false);
            _visualizationAxes.Forward.SetActive(false);
            _visualizationAxes.Back.SetActive(false);
            var displayedAxis = GetDisplayedAxis();
            displayedAxis?.SetActive(true);
            return;

            GameObject GetDisplayedAxis()
            {
                if (direction.y != 0) return direction.y > 0 ? _visualizationAxes.Up : _visualizationAxes.Down;
                if (direction.x != 0) return direction.x > 0 ? _visualizationAxes.Right : _visualizationAxes.Left;
                if (direction.z != 0) return direction.z > 0 ? _visualizationAxes.Forward : _visualizationAxes.Back;
                return null;
            }
        }

        static Vector3 GetClosestCardinalDirection(Vector3 input)
        {
            var abs = new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
            if (abs.x > abs.y && abs.x > abs.z) return input.x > 0 ? Vector3.right : Vector3.left;

            if (abs.y > abs.z) return input.y > 0 ? Vector3.up : Vector3.down;

            return input.z > 0 ? Vector3.forward : Vector3.back;
        }

        void ToggleOutlineOnObject(GameObject go, int mode)
        {
            if (go) {
                var materialsCopy = go.GetComponent<Renderer>().materials;
                if (materialsCopy.Length < 2) return;
                var shader = materialsCopy[0].shader;
                var keywords = shader.keywordSpace;
                foreach (var keyword in keywords.keywords)
                    if (keyword.name == "VISIBLE") {
                        if (mode > 0)
                            materialsCopy[0].SetKeyword(keyword, true);
                        else
                            materialsCopy[0].SetKeyword(keyword, false);
                    }

                go.GetComponent<Renderer>().materials = materialsCopy;
            }
        }

        [Serializable]
        struct Axes
        {
            public GameObject Up, Down, Left, Right, Forward, Back;
        }
    }
}