using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityGame.RespawnSystem
{
    public class RespawnController : MonoBehaviour
    {
        public static RespawnController Instance { get; private set; }

        [SerializeField] GameObject playerObject;
        [SerializeField] List<GameObject> gameObjectCheckpoints = new List<GameObject>();

        readonly List<Checkpoint> _managedCheckpoints = new List<Checkpoint>();

        CharacterController _playerCharacterController;
        public Checkpoint CurrentlyActiveRespawnPoint { get; private set; }
        public IReadOnlyList<Checkpoint> Checkpoints => _managedCheckpoints.AsReadOnly();

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            _playerCharacterController = playerObject.GetComponent<CharacterController>();
        }

        void OnDestroy()
        {
            if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetupCheckpointsFromGameObjects();

            if (CurrentlyActiveRespawnPoint) {
                var newSceneInstance =
                    _managedCheckpoints.FirstOrDefault(cp
                        => cp && cp.CheckpointID == CurrentlyActiveRespawnPoint.CheckpointID);
                if (newSceneInstance && newSceneInstance.gameObject.scene == scene) {
                    CurrentlyActiveRespawnPoint = newSceneInstance;
                } else {
                    CurrentlyActiveRespawnPoint = null;
                }
            }
            Debug.Log("Scene loaded with checkpoints, but no active respawn point. Player must reach a checkpoint.");
            if (!CurrentlyActiveRespawnPoint && _managedCheckpoints.Count == 0)
                CreateAndSetInitialSpawnCheckpointAtPlayerPosition();
        }

        void SetupCheckpointsFromGameObjects()
        {
            _managedCheckpoints.Clear();
            gameObjectCheckpoints ??= new List<GameObject>();

            foreach (var go in gameObjectCheckpoints) {
                if (!go) {
                    Debug.LogWarning("RespawnController: A null GameObject was found in the checkpointDesignators list.",
                        this);
                    continue;
                }

                var col = go.GetComponent<Collider>();
                if (!col) {
                    col = go.AddComponent<SphereCollider>();
                }
                if (!col.isTrigger) {
                    col.isTrigger = true;
                }

                var cp = go.GetComponent<Checkpoint>();
                if (!cp) {
                    cp = go.AddComponent<Checkpoint>();
                }
                _managedCheckpoints.Add(cp);
            }
        }

        void CreateAndSetInitialSpawnCheckpointAtPlayerPosition()
        {
            var playerSpawnCheckpointObject = new GameObject("INITIAL_SPAWN_POINT_DYNAMIC") {
                transform = {
                    position = playerObject.transform.position,
                    rotation = playerObject.transform.rotation
                }
            };
            playerSpawnCheckpointObject.transform.SetParent(transform);

            var sphereCollider = playerSpawnCheckpointObject.AddComponent<SphereCollider>();
            sphereCollider.isTrigger = true;
            sphereCollider.radius = 1f;

            var initialCheckpoint = playerSpawnCheckpointObject.AddComponent<Checkpoint>();
            initialCheckpoint.HasBeenReached = true;

            if (!_managedCheckpoints.Contains(initialCheckpoint))
                _managedCheckpoints.Add(initialCheckpoint);

            SetCheckpointToActiveByID(initialCheckpoint.CheckpointID);
        }

        public void SetCheckpointToActiveByID(string checkpointID)
        {
            var targetCheckpoint = FindCheckpointByID(checkpointID);
            if (targetCheckpoint && !targetCheckpoint.HasBeenReached) {
                CurrentlyActiveRespawnPoint = targetCheckpoint;
            } else {
                Debug.LogWarning($"Checkpoint with ID '{checkpointID}' not found in checkpoints list.");
            }
        }

        public void RespawnPlayer()
        {
            if (!CurrentlyActiveRespawnPoint)
                Debug.LogError("No active respawn point");

            _playerCharacterController.enabled = false;
            playerObject.transform.position = CurrentlyActiveRespawnPoint.transform.position;
            playerObject.transform.rotation = CurrentlyActiveRespawnPoint.transform.rotation;
            _playerCharacterController.enabled = true;
        }

        Checkpoint FindCheckpointByID(string id) => string.IsNullOrEmpty(id)
            ? null
            : _managedCheckpoints.FirstOrDefault(cp => cp != null && cp.CheckpointID == id);
    }
}