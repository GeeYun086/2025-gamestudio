using UnityEngine;
using UnityEngine.InputSystem;

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

        float _lookDownRotation;

        void Start()
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
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

            _lookDownRotation -= lookInputDelta.y;
            _lookDownRotation = Mathf.Clamp(_lookDownRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(_lookDownRotation, 0f, 0f);
            _playerBody.Rotate(Vector3.up * lookInputDelta.x);
        }
    }
}