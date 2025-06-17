using System.Collections;
using GravityGame.Gravity;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     (Re)spawns the assigned cube prefab on load and when redstone-powered.
    /// </summary>
    public class CubeSpawner : RedstoneComponent
    {
        [Tooltip("Cube prefab to spawn")]
        public GameObject Cube;

        [Tooltip("Delay before allowing another spawn after power is applied")]
        public float RespawnDelay = 1f;

        GameObject _currentCube;
        Vector3 _cubePosition;
        bool _isPowered;

        void Start()
        {
            // Calculate spawn position 1 unit above the spawner
            _cubePosition = transform.position + transform.up;
        }

        /// <summary>
        ///     Called whenever redstone power changes.
        ///     Spawns a cube on the rising edge (false→true) only once per pulse.
        /// </summary>
        public override bool IsPowered
        {
            get => _isPowered;
            set {
                if (value && !_isPowered) {
                    Respawn();
                    // Cooldown so holding power only spawns once
                    StartCoroutine(ResetPowerAfterDelay());
                }
                _isPowered = value;
            }
        }

        IEnumerator ResetPowerAfterDelay()
        {
            yield return new WaitForSeconds(RespawnDelay);
            _isPowered = false;
        }

        /// <summary>
        ///     Instantiates a new cube and destroys the previous one.
        ///     Also applies custom gravity if available.
        /// </summary>
        void Respawn()
        {
            if (_currentCube != null)
                Destroy(_currentCube);

            _currentCube = Instantiate(
                Cube,
                _cubePosition,
                transform.rotation,
                transform
            );

            if (_currentCube.TryGetComponent<GravityModifier>(out var gm))
                gm.GravityDirection = -transform.up;
        }
    }
}