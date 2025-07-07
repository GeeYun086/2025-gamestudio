using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

namespace GravityGame.Player
{
    /// <summary>
    ///     Camera controller for a first-person camera
    ///     Rotates the attached GameObject camera to look up and down and the <see cref="_playerBody" /> left and right
    /// </summary>
    public class FirstPersonCameraController : MonoBehaviour
    {
        [SerializeField] float _mouseSensitivity = 20f;

        [SerializeField] Transform _playerBody;
        [SerializeField] InputActionReference _lookAction;

        public float LookDownRotation;
        public float LookRightRotation;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            // Don't render player
            // Note TG: may need to change in the future
            GetComponent<Camera>().cullingMask &= ~LayerMask.GetMask("Player"); 
        }

        void Update()
        {
            // Note TG: Debug functionality to lock / unlock mouse when pressing escape to tab out of the Unity game window.
            // We might need to do this differently in the future or disable this in the build.
            if (Input.GetButtonDown("Cancel")) {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            if (Input.GetMouseButtonDown(0)) {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
            if (Cursor.lockState != CursorLockMode.Locked)
                return;

            var lookInput = _lookAction.action.ReadValue<Vector2>();
            var lookInputDelta = lookInput * (_mouseSensitivity * Time.deltaTime);
            LookRightRotation += lookInputDelta.x;
            LookDownRotation += lookInputDelta.y;
            LookDownRotation = Mathf.Clamp(LookDownRotation, -90f, 90f);

            if (_playerBody == null || _playerBody == transform) {
                transform.localRotation = Quaternion.Euler(LookDownRotation, LookRightRotation, 0f);
            } else {
                transform.localRotation = Quaternion.Euler(LookDownRotation, 0f, 0f);
                _playerBody.localRotation = Quaternion.Euler(0f, LookRightRotation, 0f);
            }
        }
    }
}