using System;
using GravityGame.Puzzle_Elements;
using UnityEngine;

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
    ///     Redstone power flag: whenever this flips, we update the laser.
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

    void Awake()
    {
        // Make sure our default (possibly inverted) state is applied
        UpdateLaserState();
    }

    void OnValidate()
    {
        // Only warn in the editor—don't spawn/destroy here!
        if (_laserPrefab == null)
            Debug.LogWarning($"{nameof(LaserSpawner)}: Laser Prefab is not assigned.", this);
    }

    /// <summary>
    ///     Spawns or destroys the laser based on current power & invert settings.
    /// </summary>
    void UpdateLaserState()
    {
        // Never spawn/destroy in edit mode
        if (!Application.isPlaying)
            return;

        bool shouldBeOn = InvertRedstoneSignal ? !_isPowered : _isPowered;
        if (shouldBeOn)
            SpawnLaser();
        else
            DestroyLaser();
    }

    /// <summary>
    ///     Instantiates (or replaces) the laser prefab at this transform + offset.
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
    ///     Destroys the last spawned laser instance, if any.
    /// </summary>
    public void DestroyLaser()
    {
        if (_spawnedLaser != null) {
            Destroy(_spawnedLaser);
            _spawnedLaser = null;
        }
    }

    /// <summary>
    ///     Fired immediately after a new laser is spawned.
    /// </summary>
    public event Action<GameObject> OnLaserSpawned;
}