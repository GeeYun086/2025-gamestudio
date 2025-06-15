using UnityEngine;
using UnityEngine.Serialization;

public class LaserSpawner : MonoBehaviour
{
    [FormerlySerializedAs("laserPrefab")]
    [Header("Laser Prefab")]
    [Tooltip("Drag your LaserBeam prefab here.")]
    public GameObject LaserPrefab;

    [FormerlySerializedAs("spawnOffset")]
    [Header("Spawn Settings")]
    [Tooltip("Offset from this GameObject’s position to spawn the beam origin.")]
    public Vector3 SpawnOffset = Vector3.zero;

    [FormerlySerializedAs("spawnOnStart")] [Tooltip("Automatically spawn the laser when the scene starts?")]
    public bool SpawnOnStart = true;

    GameObject _spawnedLaser;

    void Start()
    {
        if (SpawnOnStart)
            SpawnLaser();
    }

    /// <summary>
    ///     Instantiates the laser prefab at this transform’s position + offset, with same rotation.
    /// </summary>
    public void SpawnLaser()
    {
        if (LaserPrefab == null) {
            Debug.LogError("LaserSpawner: laserPrefab is not assigned!", this);
            return;
        }

        if (_spawnedLaser != null) {
            Destroy(_spawnedLaser);
        }

        var worldPos = transform.TransformPoint(SpawnOffset);
        _spawnedLaser = Instantiate(LaserPrefab, worldPos, transform.rotation);
        _spawnedLaser.transform.parent = transform;
    }

    /// <summary>
    ///     Destroys the spawned laser instance.
    /// </summary>
    public void DestroyLaser()
    {
        if (_spawnedLaser != null) {
            Destroy(_spawnedLaser);
            _spawnedLaser = null;
        }
    }
}