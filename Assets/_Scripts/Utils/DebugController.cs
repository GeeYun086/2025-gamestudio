using GravityGame.CheckpointSystem;
using GravityGame.Player;
using GravityGame.SaveAndLoadSystem;
using UnityEngine;

namespace GravityGame.Utils
{
    public class DebugController : MonoBehaviour
    {
        const KeyCode NoclipToggleKey = KeyCode.F1;
        const KeyCode SpawnGravityObjectKey = KeyCode.F2;
        const KeyCode TeleportToCheckpointKey = KeyCode.F3;
        const KeyCode HealPlayerKey = KeyCode.F4;
        const KeyCode DamagePlayerKey = KeyCode.F5;
        const KeyCode SaveKey = KeyCode.C;
        const KeyCode LoadKey = KeyCode.V;

        const float NoclipSpeed = 15.0f;

        [SerializeField]
        GameObject _playerObject;

        [SerializeField]
        GameObject _debugGravityObject;

        Rigidbody _playerCharacterController;
        PlayerMovement _playerMovementScript;
        Camera _mainCamera;

        bool IsNoclipActive { get; set; }

        void Awake()
        {
            _playerCharacterController = _playerObject.GetComponent<Rigidbody>();
            _playerMovementScript = _playerObject.GetComponent<PlayerMovement>();
            _mainCamera = Camera.main;
        }

        void Update()
        {
            HandleInput();
            if (IsNoclipActive)
                HandleNoclipMovement();
        }

        void HandleInput()
        {
            if (Input.GetKeyDown(NoclipToggleKey) && _playerObject && _playerCharacterController)
                ToggleNoclip();

            if (Input.GetKeyDown(SpawnGravityObjectKey) && _debugGravityObject && _mainCamera)
                SpawnGravityObject();

            if (Input.GetKeyDown(TeleportToCheckpointKey) && _playerObject) {
                TeleportPlayerToActiveCheckpoint();
            }

            if (Input.GetKeyDown(HealPlayerKey)) {
                PlayerHealth.Instance.Heal(20f);
            }

            if (Input.GetKeyDown(DamagePlayerKey)) {
                PlayerHealth.Instance.TakeDamage(20f);
            }
            
            if(Input.GetKeyDown(SaveKey)) SaveAndLoad.Instance.Save();
            if(Input.GetKeyDown(LoadKey)) SaveAndLoad.Instance.Load();
        }

        void ToggleNoclip()
        {
            IsNoclipActive = !IsNoclipActive;

            if (IsNoclipActive) {
                _playerMovementScript.enabled = false;
                _playerCharacterController.detectCollisions = false;
                _playerCharacterController.isKinematic = true;
            } else {
                _playerMovementScript.enabled = true;
                _playerCharacterController.detectCollisions = true;
                _playerCharacterController.isKinematic = false;
            }
        }

        void HandleNoclipMovement()
        {
            float forwardInput = (Input.GetKey(KeyCode.W) ? 1f : 0f) - (Input.GetKey(KeyCode.S) ? 1f : 0f);
            float rightInput = (Input.GetKey(KeyCode.D) ? 1f : 0f) - (Input.GetKey(KeyCode.A) ? 1f : 0f);
            float verticalInput = (Input.GetKey(KeyCode.Space) || Input.GetKey(KeyCode.E) ? 1f : 0f)
                                  - (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.Q) ? 1f : 0f);

            var moveDirection = _mainCamera.transform.forward * forwardInput
                                + _mainCamera.transform.right * rightInput + Vector3.up * verticalInput;
            _playerObject.transform.Translate(moveDirection.normalized * (NoclipSpeed * Time.deltaTime), Space.World);
        }

        void SpawnGravityObject()
        {
            var rayOrigin = _mainCamera.transform.position;
            var rayDirection = _mainCamera.transform.forward;
            Vector3 spawnPos;

            if (Physics.Raycast(rayOrigin, rayDirection, out var hitInfo, 50.0f, ~0)) {
                spawnPos = hitInfo.point;
            } else {
                spawnPos = rayOrigin + rayDirection * 5.0f;
            }
            Instantiate(_debugGravityObject, spawnPos, Quaternion.identity);
        }

        static void TeleportPlayerToActiveCheckpoint() => CheckpointController.RespawnPlayer();
    }
}