using System;
using GravityGame.Puzzle_Elements;
using UnityEngine;

/// <summary>
///     Spawns a laser beam prefab at a specified local-space offset from this GameObject.
///     Now supports Redstone power: the laser is active when "powered" (unless InvertRedstoneSignal is enabled).
/// </summary>
[RequireComponent(typeof(Transform))]
public class LaserSpawner : RedstoneComponent
{
    [Header("Laser Prefab")]
    [Tooltip("Drag your LaserBeam prefab here.")]
    [SerializeField] GameObject _laserPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Offset from this GameObject’s position to spawn the beam origin.")]
    [SerializeField] Vector3 _spawnOffset = Vector3.zero;

    [Header("Redstone")]
    [Tooltip("If true, laser is ON by default and turns OFF when powered. If false, laser is OFF by default and turns ON when powered.")]
    public bool InvertRedstoneSignal;

    GameObject _spawnedLaser;
    bool _isPowered;

    public override bool IsPowered
    {
        get => _isPowered;
        set {
            if (_isPowered == value) return;
            _isPowered = value;
            UpdateLaserState();
        }
    }

    void OnValidate()
    {
        if (_laserPrefab == null)
            Debug.LogWarning($"{nameof(LaserSpawner)}: Laser Prefab is not assigned.", this);
        // DO NOT call UpdateLaserState here!
    }

    void Start()
    {
        UpdateLaserState();
    }


    /// <summary>
    ///     Spawns or destroys the laser based on redstone and inversion state.
    /// </summary>
    void UpdateLaserState()
    {
        bool laserShouldBeActive = InvertRedstoneSignal ? !_isPowered : _isPowered;
        if (laserShouldBeActive)
            SpawnLaser();
        else
            DestroyLaser();
    }

    /// <summary>
    ///     Instantiates (or replaces) the laser prefab at this transform’s position plus the configured offset.
    ///     Fires <see cref="OnLaserSpawned" /> after instantiation.
    /// </summary>
    void SpawnLaser()
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
    void DestroyLaser()
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