using System.Collections.Generic;
using System.Linq;
using GravityGame.Player;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace GravityGame.CheckpointSystem
{
    public class CheckpointController : MonoBehaviour
    {
        public static CheckpointController Instance { get; private set; }

        [SerializeField] GameObject playerObject;
        [SerializeField] List<GameObject> gameObjectCheckpoints = new();

        readonly List<Checkpoint> _checkpoints = new();
        PlayerMovement _playerMovementScript;

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
            gameObjectCheckpoints ??= new List<GameObject>();

            foreach (var gObject in gameObjectCheckpoints) {
                var checkpoint = gObject.GetComponent<Checkpoint>();
                if (!checkpoint) checkpoint = gObject.AddComponent<Checkpoint>();
                _checkpoints.Add(checkpoint);
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
            _playerMovementScript.enabled = false;
            playerObject.transform.position = _checkpoints.First(cp => cp.IsActiveCheckpoint).transform.position;
            _playerMovementScript.enabled = true;
        }

        Checkpoint FindCheckpointByID(string id) => string.IsNullOrEmpty(id)
            ? null
            : _checkpoints.FirstOrDefault(cp => cp != null && cp.CheckpointID == id);
    }
}