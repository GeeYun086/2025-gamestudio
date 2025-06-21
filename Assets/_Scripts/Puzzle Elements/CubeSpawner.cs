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
        bool _isPowered;
        Vector3 CubePosition => transform.position + 1.0f * transform.up;
        
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
                }
                _isPowered = value;
            }
        }

        /// <summary>
        ///     Instantiates a new cube and destroys the previous one.
        ///     Also applies custom gravity if available.
        /// </summary>
        void Respawn()
        {
            if (_currentCube != null) {
                _currentCube.transform.position = new Vector3(10000, 10000, 10000);
                Destroy(_currentCube);
            }

            _currentCube = Instantiate(
                Cube,
                CubePosition,
                transform.rotation,
                transform
            );

            if (_currentCube.TryGetComponent<GravityModifier>(out var gm))
                gm.GravityDirection = -transform.up;
        }
    }
}