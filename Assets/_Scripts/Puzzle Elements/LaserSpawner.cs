using System;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Spawns a LaserEmitter prefab at a local‐space offset. 
    /// Can auto‐spawn on Start, manually spawn/destroy, and fires an event when spawned.
    /// Also assigns the "Laser" layer to all spawned children.
    /// </summary>
    [RequireComponent(typeof(Transform))]
    public class LaserSpawner : MonoBehaviour
    {
        [Header("Laser Prefab")]
        [Tooltip("Drag your LaserEmitter.prefab here.")]
        [SerializeField] private GameObject _laserEmitterPrefab;

        [Header("Spawn Settings")]
        [Tooltip("Local‐space offset from this GameObject to spawn the emitter root.")]
        [SerializeField] private Vector3 _spawnOffset = Vector3.zero;
        [Tooltip("Automatically spawn on scene start?")]
        [SerializeField] private bool _spawnOnStart = true;

        private GameObject _spawnedEmitter;

        /// <summary>
        /// Fired immediately after a new LaserEmitter instance is spawned.
        /// </summary>
        public event Action<GameObject> OnLaserSpawned;

        void OnValidate()
        {
            if (_laserEmitterPrefab == null)
                Debug.LogWarning($"{nameof(LaserSpawner)}: no prefab assigned", this);
        }

        void Start()
        {
            if (_spawnOnStart)
                SpawnLaser();
        }

        /// <summary>
        /// Instantiates (or replaces) the LaserEmitter prefab under this transform.
        /// </summary>
        public void SpawnLaser()
        {
            if (_laserEmitterPrefab == null)
            {
                Debug.LogError($"{nameof(LaserSpawner)}: Cannot spawn, prefab is null", this);
                return;
            }

            DestroyLaser();
            Vector3 worldPos = transform.TransformPoint(_spawnOffset);
            _spawnedEmitter = Instantiate(_laserEmitterPrefab, worldPos, transform.rotation, transform);

            // Assign every child to the Laser layer
            int laserLayer = LayerMask.NameToLayer("Laser");
            SetLayerRecursively(_spawnedEmitter, laserLayer);

            OnLaserSpawned?.Invoke(_spawnedEmitter);
        }

        /// <summary>
        /// Destroys the last‐spawned LaserEmitter instance.
        /// </summary>
        public void DestroyLaser()
        {
            if (_spawnedEmitter != null)
            {
                Destroy(_spawnedEmitter);
                _spawnedEmitter = null;
            }
        }

        private void SetLayerRecursively(GameObject go, int layer)
        {
            go.layer = layer;
            foreach (Transform child in go.transform)
                SetLayerRecursively(child.gameObject, layer);
        }
    }
}
