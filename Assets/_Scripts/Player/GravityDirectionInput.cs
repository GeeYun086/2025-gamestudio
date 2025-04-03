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
        [Serializable] struct Axes
        {
            public GameObject Up, Down, Left, Right, Forward, Back;
        }

        [SerializeField] Axes _visualizationAxes;
        [SerializeField] float _maxObjectRange = 30;

        [CanBeNull] GravityModifier _selectedObject;
        Vector3 _selectedDirection;
        static GravityDirectionRadialMenu GravityChangeMenu => GameUI.Instance.Elements.GravityDirectionRadialMenu;

        void OnEnable()
        {
            SetVisualizedDirection(Vector3.zero);
        }

        void Update()
        {
            bool hasSelectedObject = _selectedObject != null;

            if (hasSelectedObject) {
                // TODO TG: actually highlight object
                DebugDraw.DrawSphere(_selectedObject.transform.position, 1.0f, Color.white);
            }

            bool wasInteracting = GravityChangeMenu.visible;
            bool isInteracting = Input.GetMouseButton(1) && hasSelectedObject;

            if (isInteracting && !wasInteracting) {
                // Start interaction
                GravityChangeMenu.visible = true;
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            } else if (!isInteracting && wasInteracting) {
                // Stop interaction
                var direction = GetRadialMenuGravityDirection();
                if (_selectedObject && direction.HasValue) {
                    _selectedObject.GravityDirection = direction.Value;
                }
                SetVisualizedDirection(Vector3.zero);

                GravityChangeMenu.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            if (isInteracting) {
                var direction = GetRadialMenuGravityDirection();
                SetVisualizedDirection(direction ?? _selectedObject.GravityDirection);
            } else {
                _selectedObject = RaycastForSelectableObject();
            }
        }

        [CanBeNull] GravityModifier RaycastForSelectableObject()
        {
            var cam = Camera.main!;
            var screenCenter = cam.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
            var ray = cam.ScreenPointToRay(screenCenter);

            // Note TG: other objects may block the hit, maybe need to ignore more layers in the future
            int layerMask = ~LayerMask.GetMask("AxisGizmo");
            bool hit = Physics.Raycast(ray, out var hitResult, _maxObjectRange, layerMask);
            if (hit && hitResult.transform.gameObject.TryGetComponent<GravityModifier>(out var selectable))
                return selectable;
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

            if (distance < GravityChangeMenu.DeadZoneRadius) {
                return null;
            }
            if (distance < GravityChangeMenu.InnerRadius) {
                return mouseOffset.y > 0 ? forward : -forward;
            }
            if (Mathf.Abs(mouseOffset.x) > Mathf.Abs(mouseOffset.y)) {
                return mouseOffset.x > 0 ? right : -right;
            }
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
                if (direction.y != 0) {
                    return direction.y > 0 ? _visualizationAxes.Up : _visualizationAxes.Down;
                }
                if (direction.x != 0) {
                    return direction.x > 0 ? _visualizationAxes.Right : _visualizationAxes.Left;
                }
                if (direction.z != 0) {
                    return direction.z > 0 ? _visualizationAxes.Forward : _visualizationAxes.Back;
                }
                return null;
            }
        }

        static Vector3 GetClosestCardinalDirection(Vector3 input)
        {
            var abs = new Vector3(Mathf.Abs(input.x), Mathf.Abs(input.y), Mathf.Abs(input.z));
            if (abs.x > abs.y && abs.x > abs.z) {
                return input.x > 0 ? Vector3.right : Vector3.left;
            }
            if (abs.y > abs.z) {
                return input.y > 0 ? Vector3.up : Vector3.down;
            }
            return input.z > 0 ? Vector3.forward : Vector3.back;
        }
    }
}