using System.Collections.Generic;
using System.Linq;
using GravityGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityGame.RespawnSystem
{
    public class RespawnController : MonoBehaviour
    {
        public static RespawnController Instance { get; private set; }

        [SerializeField] GameObject playerObject;
        [SerializeField] List<GameObject> gameObjectCheckpoints = new List<GameObject>();
        
        readonly List<Checkpoint> _checkpoints = new List<Checkpoint>();

        PlayerMovement _playerMovementScript;
        [field: SerializeField]
        Checkpoint CurrentlyActiveRespawnPoint { get; set; }

        void Awake()
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;

            _playerMovementScript = playerObject.GetComponent<PlayerMovement>();
        }

        void OnDestroy()
        {
            if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SetupCheckpointsFromGameObjects();

            if (CurrentlyActiveRespawnPoint) {
                string previousCheckpointID = CurrentlyActiveRespawnPoint.CheckpointID;
                var newSceneInstance = _checkpoints.FirstOrDefault(cp => cp && cp.CheckpointID == previousCheckpointID);
                if (newSceneInstance && newSceneInstance.gameObject.scene == scene) {
                    CurrentlyActiveRespawnPoint = newSceneInstance;
                    CurrentlyActiveRespawnPoint.HasBeenReached = true;
                } else {
                    CurrentlyActiveRespawnPoint = null;
                }
            }
            if (!CurrentlyActiveRespawnPoint)
                CreateAndSetInitialSpawnCheckpointAtPlayerPosition();
        }

        void SetupCheckpointsFromGameObjects()
        {
            _checkpoints.Clear();
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
                _checkpoints.Add(cp);
            }
        }

        void CreateAndSetInitialSpawnCheckpointAtPlayerPosition()
        {
            var playerSpawnCheckpointObject = new GameObject("INITIAL_SPAWN_POINT") {
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

            if (!_checkpoints.Contains(initialCheckpoint))
                _checkpoints.Add(initialCheckpoint);

            CurrentlyActiveRespawnPoint = initialCheckpoint;
            Debug.Log($"RespawnController: Initial spawn point created and set as active. ID: {initialCheckpoint.CheckpointID}, Position: {initialCheckpoint.transform.position}", initialCheckpoint.gameObject);
        }

        public void TrySetCheckpointToActiveByID(string checkpointID)
        {
            var targetCheckpoint = FindCheckpointByID(checkpointID);
            if (!targetCheckpoint || targetCheckpoint.HasBeenReached)
                return;
            CurrentlyActiveRespawnPoint = targetCheckpoint;
            targetCheckpoint.HasBeenReached = true;
        }

        public void RespawnPlayer()
        {
            if (!CurrentlyActiveRespawnPoint) {
                Debug.LogError("RespawnController: No active respawn point. Cannot respawn player.");
                return; 
            }

            _playerMovementScript.enabled = false;
            playerObject.transform.position = CurrentlyActiveRespawnPoint.transform.position;
            _playerMovementScript.enabled = true;
        }

        Checkpoint FindCheckpointByID(string id) => string.IsNullOrEmpty(id)
            ? null
            : _checkpoints.FirstOrDefault(cp => cp != null && cp.CheckpointID == id);
    }
}