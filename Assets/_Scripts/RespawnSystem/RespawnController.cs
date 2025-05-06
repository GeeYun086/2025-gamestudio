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

        [SerializeField] string defaultStartingCheckpointID = "InitialSpawn";

        CharacterController _playerCharacterController;
        Checkpoint _currentRespawnTargetCheckpoint;

        private readonly static List<Checkpoint> s_registeredCheckpoints = new List<Checkpoint>();
        public Checkpoint CurrentlyActiveRespawnPoint => _currentRespawnTargetCheckpoint;

        public List<CheckpointData> GetAllCheckpointData() => s_registeredCheckpoints
            .Select(cp => new CheckpointData(cp, cp == _currentRespawnTargetCheckpoint)).ToList();

        void Awake()
        {
            if (!Instance) {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                SceneManager.sceneLoaded += OnSceneLoaded;
            } else if (Instance != this) {
                Destroy(gameObject);
                return;
            }
            TryFindPlayer();
        }

        void OnDestroy()
        {
            if (Instance == this) SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            TryFindPlayer();

            if (_currentRespawnTargetCheckpoint)
                return;
            var defaultCheckpoint = FindCheckpointByID(defaultStartingCheckpointID);
            if (defaultCheckpoint != null && defaultCheckpoint.HasBeenReached ||
                defaultCheckpoint != null && string.IsNullOrEmpty(defaultStartingCheckpointID)) {
                SetCurrentRespawnCheckpoint(defaultCheckpoint);
            }
        }

        void TryFindPlayer()
        {
            if (!playerObject) {
                var cameraController = FindFirstObjectByType<FirstPersonCameraController>();
                if (cameraController) playerObject = cameraController.gameObject;
            }
            _playerCharacterController = playerObject.GetComponent<CharacterController>();
        }

        public void SetCurrentRespawnCheckpoint(Checkpoint newRespawnTarget)
            => _currentRespawnTargetCheckpoint = newRespawnTarget;

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
            playerObject.transform.position = _currentRespawnTargetCheckpoint.transform.position;
            playerObject.transform.rotation = _currentRespawnTargetCheckpoint.transform.rotation;
            _playerCharacterController.enabled = true;
        }

        public static void RegisterCheckpoint(Checkpoint checkpoint)
        {
            if (s_registeredCheckpoints.Contains(checkpoint)) return;
            s_registeredCheckpoints.Add(checkpoint);
        }

        public static void UnregisterCheckpoint(Checkpoint checkpoint)
        {
            if (!s_registeredCheckpoints.Remove(checkpoint)) return;
        }

        static Checkpoint FindCheckpointByID(string id) => string.IsNullOrEmpty(id)
            ? null
            : s_registeredCheckpoints.FirstOrDefault(cp => cp.CheckpointID == id);
    }
}