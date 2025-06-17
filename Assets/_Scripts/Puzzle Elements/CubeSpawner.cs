using System.Collections;
using GravityGame.Gravity;
using UnityEngine;
using UnityEngine.Serialization;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// (Re)spawns assigned (<see cref="Cube"/>) cube on load and when redstone‐powered.
    /// </summary>
    public class CubeSpawner : RedstoneComponent
    {
        [Tooltip("Cube prefab to spawn")]
        public GameObject Cube;

        [FormerlySerializedAs("respawnDelay")] [Tooltip("Delay before allowing another spawn after power on")]
        public float RespawnDelay = 1f;

        GameObject _currentCube;
        Vector3   _cubePosition;
        bool      _isPowered;

        void Start()
        {
            // Calculate spawn position 1 unit above the spawner
            _cubePosition = transform.position + transform.up;
            Respawn();
        }

        /// <summary>
        /// Called whenever redstone power changes.
        /// Spawns a cube on the rising edge (false→true) only once per pulse.
        /// </summary>
        public override bool IsPowered
        {
            get => _isPowered;
            set
            {
                if (value && !_isPowered)
                {
                    Respawn();
                    // Start cooldown so a constant powered state only spawns once
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
        /// Instantiates a new cube and destroys the old one.
        /// Also applies custom gravity if available.
        /// </summary>
        private void Respawn()
        {
            // Destroy previous cube
            if (_currentCube != null)
                Destroy(_currentCube);

            // Spawn new cube
            _currentCube = Instantiate(
                Cube,
                _cubePosition,
                transform.rotation,
                transform
            );

            // If the cube has a GravityModifier, set its direction
            if (_currentCube.TryGetComponent<GravityModifier>(out var gm))
                gm.GravityDirection = -transform.up;
        }
    }
}
