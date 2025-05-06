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
        CharacterController _playerCharacterController;
        public Checkpoint CurrentlyActiveRespawnPoint { get; private set; }

        readonly static List<Checkpoint> RegisteredCheckpoints = new();

        void Awake()
        {
            if (!Instance) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            } else if (Instance != this) {
                Destroy(gameObject);
            }
        }

        void CreateAndSetInitialSpawnCheckpoint()
        {
            var playerSpawnCheckpointObject = new GameObject("INITIAL_SPAWN_POINT") {
                transform = {
                    position = playerObject.transform.position,
                    rotation = playerObject.transform.rotation
                }
            };
            playerSpawnCheckpointObject.transform.SetParent(transform);
            playerSpawnCheckpointObject.AddComponent<SphereCollider>();

            var initialCheckpoint = playerSpawnCheckpointObject.AddComponent<Checkpoint>();
            initialCheckpoint.HasBeenReached = true;
            initialCheckpoint.CheckpointID = "InitialSpawnCheckpoint_" + playerSpawnCheckpointObject.GetInstanceID();

            RegisterCheckpoint(initialCheckpoint);
            SetCurrentRespawnCheckpoint(initialCheckpoint);
        }

        void OnDestroy()
        {
            if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (RegisteredCheckpoints.Count == 0 || !CurrentlyActiveRespawnPoint) CreateAndSetInitialSpawnCheckpoint();
        }

        public void SetCurrentRespawnCheckpoint(Checkpoint newRespawnTarget)
            => CurrentlyActiveRespawnPoint = newRespawnTarget;

        public void SetActiveRespawnPointByID(string checkpointID)
        {
            var targetCheckpoint = FindCheckpointByID(checkpointID);
            if (targetCheckpoint.HasBeenReached) {
                SetCurrentRespawnCheckpoint(targetCheckpoint);
            }
        }

        public void RespawnPlayer()
        {
            _playerCharacterController = playerObject.GetComponent<CharacterController>();

            _playerCharacterController.enabled = false;
            playerObject.transform.position = CurrentlyActiveRespawnPoint.transform.position;
            playerObject.transform.rotation = CurrentlyActiveRespawnPoint.transform.rotation;
            _playerCharacterController.enabled = true;
        }

        public static void RegisterCheckpoint(Checkpoint checkpoint)
        {
            if (RegisteredCheckpoints.Contains(checkpoint)) return;
            RegisteredCheckpoints.Add(checkpoint);
        }

        public static void UnregisterCheckpoint(Checkpoint checkpoint)
        {
            if (!RegisteredCheckpoints.Remove(checkpoint)) return;
        }

        static Checkpoint FindCheckpointByID(string id) => string.IsNullOrEmpty(id)
            ? null
            : RegisteredCheckpoints.FirstOrDefault(cp => cp.CheckpointID == id);
    }
}