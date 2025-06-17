using System;
using UnityEngine;

/// <summary>
///     Spawns a laser beam prefab at a specified local-space offset from this GameObject.
///     Optionally auto-spawns on Start, allows manual spawn/destruction, and fires an event when a laser is spawned.
/// </summary>
[RequireComponent(typeof(Transform))]
public class LaserSpawner : MonoBehaviour
{
    [Header("Laser Prefab")]
    [Tooltip("Drag your LaserBeam prefab here.")]
    [SerializeField]
    GameObject _laserPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Offset from this GameObject’s position to spawn the beam origin.")]
    [SerializeField]
    Vector3 _spawnOffset = Vector3.zero;

    [Tooltip("Automatically spawn the laser when the scene starts?")]
    [SerializeField]
    bool _spawnOnStart = true;

    GameObject _spawnedLaser;

    /// <summary>
    ///     Called in the Editor when values change—warns if required references are missing.
    /// </summary>
    void OnValidate()
    {
        if (_laserPrefab == null)
            Debug.LogWarning($"{nameof(LaserSpawner)}: Laser Prefab is not assigned.", this);
    }

    void Start()
    {
        if (_spawnOnStart)
            SpawnLaser();
    }

    /// <summary>
    ///     Instantiates (or replaces) the laser prefab at this transform’s position plus the configured offset.
    ///     Fires <see cref="OnLaserSpawned" /> after instantiation.
    /// </summary>
    public void SpawnLaser()
    {
        if (_laserPrefab == null) {
            Debug.LogError($"{nameof(LaserSpawner)}: Cannot spawn – no prefab assigned.", this);
            return;
        }

        DestroyLaser();

        var worldPos = transform.TransformPoint(_spawnOffset);
        _spawnedLaser = Instantiate(_laserPrefab, worldPos, transform.rotation);
        _spawnedLaser.transform.SetParent(transform, worldPositionStays: true);

        OnLaserSpawned?.Invoke(_spawnedLaser);
    }

    /// <summary>
    ///     Destroys the last spawned laser instance, if one exists.
    /// </summary>
    public void DestroyLaser()
    {
        if (_spawnedLaser != null) {
            Destroy(_spawnedLaser);
            _spawnedLaser = null;
        }
    }

    /// <summary>
    ///     Invoked immediately after a new laser instance is spawned.
    /// </summary>
    public event Action<GameObject> OnLaserSpawned;
}