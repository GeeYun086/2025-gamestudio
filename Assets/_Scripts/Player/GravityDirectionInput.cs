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
        [SerializeField] private Axes _visualizationAxes;
        [SerializeField] private float _maxObjectRange = 30;
        [SerializeField] private float _sphereSelectionRadius = 0.5f;
        [SerializeField] private float _aimBufferDuration = 0.25f;
        [CanBeNull] private GravityModifier _aimedObject;
        [CanBeNull] private GravityModifier _lastAimedObject;
        private float _lastObjectAimedTime;
        [CanBeNull] private GameObject _lastSelectedObject;

        private Vector3 _selectedDirection;

        [CanBeNull] private GravityModifier _selectedObject;

        private static GravityDirectionRadialMenu GravityChangeMenu =>
            GameUI.Instance.Elements.GravityDirectionRadialMenu;

        private void Update()
        {
            var hitObject = RaycastForSelectableObject();

            if (hitObject)
            {
                _aimedObject = hitObject.GetComponent<GravityModifier>();
                if (hitObject != _lastSelectedObject)
                {
                    ToggleOutlineOnObject(hitObject, 1);
                    ToggleOutlineOnObject(_lastSelectedObject, 0);
                }
                else
                {
                    ToggleOutlineOnObject(hitObject, 1);
                }

                _lastSelectedObject = hitObject;
                _lastAimedObject = _aimedObject;
                _lastObjectAimedTime = Time.time;
            }
            else
            {
                ToggleOutlineOnObject(_lastSelectedObject, 0);
            }

            if (Input.GetMouseButtonDown(1))
                if (!_selectedObject)
                {
                    GravityModifier objectToSelect = null;
                    if (_aimedObject) objectToSelect = _aimedObject;
                    else if (_lastAimedObject && Time.time - _lastObjectAimedTime < _aimBufferDuration)
                        objectToSelect = _lastAimedObject;

                    if (objectToSelect) _selectedObject = objectToSelect;
                }

            var isInteracting = Input.GetMouseButton(1) && _selectedObject;

            switch (isInteracting)
            {
                case true when !GravityChangeMenu.visible:
                {
                    GravityChangeMenu.visible = true;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    if (_selectedObject) SetVisualizedDirection(_selectedObject.GravityDirection);
                    break;
                }
                case false when GravityChangeMenu.visible:
                {
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

            if (isInteracting)
            {
                if (!_selectedObject) return;
                // TODO TG: actually highlight object
                DebugDraw.DrawSphere(_selectedObject.transform.position, 1.0f, Color.white);

                SetVisualizedDirection(GetRadialMenuGravityDirection() ?? _selectedObject.GravityDirection);
            }
        }

        private void OnEnable()
        {
            SetVisualizedDirection(Vector3.zero);
        }

        [CanBeNull]
        private GameObject RaycastForSelectableObject()
        {
            var cam = Camera.main!;
            var ray = new Ray(cam.transform.position, cam.transform.forward);
            // Note TG: other objects may block the hit, maybe need to ignore more layers in the future
            var layerMask = ~LayerMask.GetMask("AxisGizmo", "Player");
            if (!Physics.SphereCast(ray, _sphereSelectionRadius, out var hitResult, _maxObjectRange, layerMask,
                    QueryTriggerInteraction.Ignore)) return null;
            return hitResult.transform.gameObject.TryGetComponent<GravityModifier>(out var selectable)
                ? hitResult.transform.gameObject
                : null;
        }

        private static Vector3? GetRadialMenuGravityDirection()
        {
            var cam = Camera.main!;
            var screenCenter = cam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var mouseOffset = Input.mousePosition - screenCenter;
            var distance = mouseOffset.magnitude;

            var up = Vector3.up;
            var forward = GetClosestCardinalDirection(Vector3.ProjectOnPlane(cam.transform.forward, up).normalized);
            var right = Vector3.Cross(up, forward).normalized;

            if (distance < GravityChangeMenu.DeadZoneRadius) return null;

            if (distance < GravityChangeMenu.InnerRadius) return mouseOffset.y > 0 ? forward : -forward;

            if (Mathf.Abs(mouseOffset.x) > Mathf.Abs(mouseOffset.y)) return mouseOffset.x > 0 ? right : -right;

            return mouseOffset.y > 0 ? up : -up;
        }

        private void SetVisualizedDirection(Vector3 direction)
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

        private static Vector3 GetClosestCardinalDirection(Vector3 input)
        {
            var abs = new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
            if (abs.x > abs.y && abs.x > abs.z) return input.x > 0 ? Vector3.right : Vector3.left;

            if (abs.y > abs.z) return input.y > 0 ? Vector3.up : Vector3.down;

            return input.z > 0 ? Vector3.forward : Vector3.back;
        }

        private void ToggleOutlineOnObject(GameObject go, int mode)
        {
            if (go)
            {
                var materialsCopy = go.GetComponent<Renderer>().materials;
                var shader = materialsCopy[1].shader;
                var keywords = shader.keywordSpace;
                foreach (var keyword in keywords.keywords)
                    if (keyword.name == "VISIBLE")
                    {
                        if (mode > 0)
                            materialsCopy[1].SetKeyword(keyword, true);
                        else
                            materialsCopy[1].SetKeyword(keyword, false);
                    }

                go.GetComponent<Renderer>().materials = materialsCopy;
            }
        }

        [Serializable]
        private struct Axes
        {
            public GameObject Up, Down, Left, Right, Forward, Back;
        }
    }
}