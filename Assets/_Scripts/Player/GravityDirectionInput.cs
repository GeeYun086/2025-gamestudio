using System;
using GravityGame.Gravity;
using GravityGame.UI;
using GravityGame.Utils;
using JetBrains.Annotations;
using UnityEngine;
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
        [SerializeField] Axes _visualizationAxes;
        [SerializeField] float _maxObjectRange = 30;
        [SerializeField] float _aimBufferDuration = 0.25f;
        [CanBeNull] GravityModifier _aimedObject;
        [CanBeNull] GravityModifier _lastAimedObject;
        float _lastObjectAimedTime;
        [CanBeNull] GameObject _lastSelectedObject;

        Vector3 _selectedDirection;

        [CanBeNull] GravityModifier _selectedObject;

        static GravityDirectionRadialMenu GravityChangeMenu => GameUI.Instance.Elements.GravityDirectionRadialMenu;

        void Update()
        {
            var hitObject = RaycastForSelectableObject();

            if (hitObject) {
                _aimedObject = hitObject.GetComponent<GravityModifier>();
                if (!_selectedObject || hitObject != _selectedObject.gameObject) {
                    if (hitObject != _lastSelectedObject) {
                        ToggleOutlineOnObject(hitObject, 1);
                        ToggleOutlineOnObject(_lastSelectedObject, 0);
                    } else {
                        ToggleOutlineOnObject(hitObject, 1);
                    }
                }

                _lastSelectedObject = hitObject;
                _lastAimedObject = _aimedObject;
                _lastObjectAimedTime = Time.time;
            } else {
                _aimedObject = null;
                if (_lastSelectedObject &&
                    Time.time - _lastObjectAimedTime > _aimBufferDuration &&
                    (!_selectedObject || _lastSelectedObject != _selectedObject.gameObject)) {
                    ToggleOutlineOnObject(_lastSelectedObject, 0);
                    _lastSelectedObject = null;
                }
            }

            if (Input.GetMouseButtonDown(1))
                if (!_selectedObject) {
                    GravityModifier objectToSelect = null;
                    if (_aimedObject) objectToSelect = _aimedObject;
                    else if (_lastAimedObject && Time.time - _lastObjectAimedTime < _aimBufferDuration)
                        objectToSelect = _lastAimedObject;

                    if (objectToSelect) {
                        _selectedObject = objectToSelect;
                        if (_selectedObject.gameObject != _lastSelectedObject) {
                            ToggleOutlineOnObject(_lastSelectedObject, 0);
                            ToggleOutlineOnObject(_selectedObject.gameObject, 1);
                            _lastSelectedObject = _selectedObject.gameObject;
                        }
                    }
                }

            bool isInteracting = Input.GetMouseButton(1) && _selectedObject;

            switch (isInteracting) {
                case true when !GravityChangeMenu.visible: {
                    GravityChangeMenu.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    if (_selectedObject) SetVisualizedDirection(_selectedObject.GravityDirection);
                    if (_selectedObject) {
                        ToggleOutlineOnObject(_selectedObject.gameObject, 1);
                        _lastSelectedObject = _selectedObject.gameObject;
                    }
                    break;
                }
                case false when GravityChangeMenu.visible: {
                    var direction = GetRadialMenuGravityDirection();
                    if (_selectedObject && direction.HasValue) _selectedObject.GravityDirection = direction.Value;

                    SetVisualizedDirection(Vector3.zero);

                    GravityChangeMenu.visible = false;
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;

                    _selectedObject = null;
                    break;
                }
            }

            if (isInteracting) {
                if (!_selectedObject) return;
                ToggleOutlineOnObject(_selectedObject.gameObject, 1);
                
                // TODO TG: actually highlight object
                DebugDraw.DrawSphere(_selectedObject.transform.position, 1.0f, Color.white);

                SetVisualizedDirection(GetRadialMenuGravityDirection() ?? _selectedObject.GravityDirection);
            }
        }

        void OnEnable()
        {
            SetVisualizedDirection(Vector3.zero);
        }

        [CanBeNull]
        GameObject RaycastForSelectableObject()
        {
            var cam = Camera.main!;
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            // Note TG: other objects may block the hit, maybe need to ignore more layers in the future
            int layerMask = ~LayerMask.GetMask("AxisGizmo", "Player");

            if (Physics.Raycast(ray, out var hitInfo, _maxObjectRange, layerMask, QueryTriggerInteraction.Ignore)) {
                if (hitInfo.transform.gameObject.TryGetComponent<GravityModifier>(out _)) return hitInfo.transform.gameObject;
            }
            return null;
        }

        static Vector3? GetRadialMenuGravityDirection()
        {
            var cam = Camera.main!;
            var screenCenter = cam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var mouseOffset = Input.mousePosition - screenCenter;
            float distance = mouseOffset.magnitude;

            var up = Vector3.up;
            var forward = GetClosestCardinalDirection(Vector3.ProjectOnPlane(cam.transform.forward, up).normalized);
            var right = Vector3.Cross(up, forward).normalized;

            if (distance < GravityChangeMenu.DeadZoneRadius) return null;

            if (distance < GravityChangeMenu.InnerRadius) return mouseOffset.y > 0 ? forward : -forward;

            if (Mathf.Abs(mouseOffset.x) > Mathf.Abs(mouseOffset.y)) return mouseOffset.x > 0 ? right : -right;

            return mouseOffset.y > 0 ? up : -up;
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
                var shader = materialsCopy[1].shader;
                var keywords = shader.keywordSpace;
                foreach (var keyword in keywords.keywords)
                    if (keyword.name == "VISIBLE") {
                        if (mode > 0)
                            materialsCopy[1].SetKeyword(keyword, true);
                        else
                            materialsCopy[1].SetKeyword(keyword, false);
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