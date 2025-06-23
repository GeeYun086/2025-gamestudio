using System;
using GravityGame.Puzzle_Elements;
using UnityEngine;

/// <summary>
///     Spawns a laser beam prefab at a specified local-space offset from this GameObject.
///     Supports Redstone power: the laser is active when “powered” (unless InvertRedstoneSignal is enabled),
///     and initializes correctly even with no redstone signal connected.
/// </summary>
[RequireComponent(typeof(Transform))]
public class LaserSpawner : RedstoneComponent
{
    [Header("Laser Prefab")]
    [Tooltip("Drag your LaserBeam prefab here.")]
    [SerializeField]
    GameObject _laserPrefab;

    [Header("Spawn Settings")]
    [Tooltip("Offset from this GameObject’s position to spawn the beam origin.")]
    [SerializeField]
    Vector3 _spawnOffset = Vector3.zero;

    [Header("Redstone")]
    [Tooltip("If true, laser is ON by default and turns OFF when powered. If false, laser is OFF by default and turns ON when powered.")]
    public bool InvertRedstoneSignal;

    GameObject _spawnedLaser;
    bool _isPowered;

    /// <summary>
    ///     Redstone power control. Whenever this is set, updates the laser state.
    /// </summary>
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

        // Reflect inspector changes immediately
        UpdateLaserState();
    }

    void Start()
    {
        // Ensure laser is in the correct default state even if no redstone signal is ever applied
        UpdateLaserState();
    }

    /// <summary>
    ///     Spawns or destroys the laser based on current power & invert settings.
    /// </summary>
    void UpdateLaserState()
    {
        bool shouldBeOn = InvertRedstoneSignal ? !_isPowered : _isPowered;
        if (shouldBeOn)
            SpawnLaser();
        else
            DestroyLaser();
    }

    /// <summary>
    ///     Instantiates (or replaces) the laser prefab at this transform’s position plus the configured offset.
    ///     Fires OnLaserSpawned after instantiation.
    /// </summary>
    public void SpawnLaser()
    {
        if (_laserPrefab == null) {
            Debug.LogError($"{nameof(LaserSpawner)}: Cannot spawn – no prefab assigned.", this);
            return;
        }

        DestroyLaser();

        var worldPos = transform.TransformPoint(_spawnOffset);
        _spawnedLaser = Instantiate(_laserPrefab, worldPos, transform.rotation, transform);
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