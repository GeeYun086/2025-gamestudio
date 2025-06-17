using System.Collections.Generic;
using System.Linq;
using GravityGame.Gravity;
using GravityGame.Player;
using GravityGame.Utils;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityGame.CheckpointSystem
{
    /// <summary>
    /// Manages all checkpoints in the scene and the player's respawn logic.
    /// This component should be present in each scene that requires checkpoint functionality.
    /// How to use:
    /// 1. Populate the 'Game Object Checkpoints' list in the Inspector with GameObjects
    ///    that represent checkpoints. These GameObjects should ideally have the
    ///    <see cref="Checkpoint"/> component already, or it will be added automatically.
    ///    Each checkpoint GameObject must have a Collider component (e.g., BoxCollider, SphereCollider)
    /// 2. If no checkpoints are active when a scene loads (or on initial game start),
    ///    an initial spawn point checkpoint will be created at the player's starting position.
    /// 3. To make the player respawn, call the <see cref="RespawnPlayer"/> method.
    ///
    /// The controller ensures that only one checkpoint is active at any time.
    /// When a player triggers a new, unreached checkpoint, that checkpoint becomes the active one.
    /// </summary>
    public class CheckpointController : SingletonMonoBehavior<CheckpointController>
    {
        [SerializeField] GameObject _playerObject;
        [SerializeField] List<GameObject> _gameObjectCheckpoints = new();
        [SerializeField] float _respawnHeightOffset = 2.0f;

        readonly List<Checkpoint> _checkpoints = new();
        PlayerMovement _playerMovementScript;
        FirstPersonCameraController _cameraController;

        void Awake()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnEnable()
        {
            _playerMovementScript = _playerObject.GetComponent<PlayerMovement>();
            _cameraController = _playerObject.GetComponentInChildren<FirstPersonCameraController>();
            PlayerHealth.Instance.OnPlayerDied.AddListener(RespawnPlayer);
        }

        void OnDisable() => PlayerHealth.Instance.OnPlayerDied.RemoveListener(RespawnPlayer);

        void OnDestroy()
        {
            if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetupCheckpointsFromGameObjects();
            if (!_checkpoints.FirstOrDefault(cp => cp.IsActiveCheckpoint))
                CreateAndSetInitialSpawnCheckpointAtPlayerPosition();
        }

        void DeactivateAllCheckpoints()
        {
            foreach (var cp in _checkpoints.Where(cp => cp)) cp.IsActiveCheckpoint = false;
        }

        void SetupCheckpointsFromGameObjects()
        {
            _checkpoints.Clear();
            _gameObjectCheckpoints ??= new List<GameObject>();

            foreach (var gObject in _gameObjectCheckpoints) {
                var checkpoint = gObject.GetComponent<Checkpoint>();
                if (!checkpoint) checkpoint = gObject.AddComponent<Checkpoint>();
                _checkpoints.Add(checkpoint);
            }
        }

        void CreateAndSetInitialSpawnCheckpointAtPlayerPosition()
        {
            var playerSpawnCheckpointObject = new GameObject("INITIAL_SPAWN_POINT") {
                transform = {
                    position = _playerObject.transform.position,
                    rotation = _playerObject.transform.rotation
                }
            };
            playerSpawnCheckpointObject.transform.SetParent(transform);

            var sphereCollider = playerSpawnCheckpointObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 1f;

            var initialCheckpoint = playerSpawnCheckpointObject.AddComponent<Checkpoint>();
            initialCheckpoint.HasBeenReached = true;
            initialCheckpoint.IsActiveCheckpoint = true;

            if (!_checkpoints.Contains(initialCheckpoint))
                _checkpoints.Add(initialCheckpoint);
        }

        public void TriggerCheckpointById(string checkpointID)
        {
            var targetCheckpoint = FindCheckpointByID(checkpointID);
            if (!targetCheckpoint || targetCheckpoint.HasBeenReached)
                return;

            DeactivateAllCheckpoints();
            targetCheckpoint.IsActiveCheckpoint = true;
            targetCheckpoint.HasBeenReached = true;
        }

        public void RespawnPlayer()
        {
            var activeCheckpoint = _checkpoints.FirstOrDefault(cp => cp.IsActiveCheckpoint);
            if (!activeCheckpoint) {
                Debug.LogError("No active checkpoint");
                return;
            }
            _playerMovementScript.enabled = false;
            
            _playerObject.transform.rotation = Quaternion.identity;
            var checkpointRotation = activeCheckpoint.transform.rotation.eulerAngles;
            _cameraController.LookRightRotation = checkpointRotation.y;
            _cameraController.LookDownRotation = Mathf.Clamp(checkpointRotation.x, -90f, 90f);
            
            var playerRb = _playerObject.GetComponent<Rigidbody>();
            playerRb.MovePosition(
                activeCheckpoint.transform.position + Vector3.up * _respawnHeightOffset
            );
            
            playerRb.linearVelocity = Vector3.zero;
            var playerGravity = _playerObject.GetComponent<GravityModifier>();
            playerGravity.GravityDirection = Vector3.down;

            _playerMovementScript.enabled = true;
            PlayerHealth.Instance.Heal(PlayerHealth.MaxHealth);
        }

        Checkpoint FindCheckpointByID(string id) =>
            string.IsNullOrEmpty(id)
                ? null
                : _checkpoints.FirstOrDefault(cp => cp != null && cp.CheckpointID == id);
    }
}