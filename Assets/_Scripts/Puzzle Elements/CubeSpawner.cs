using System;
using GravityGame.Gravity;
using GravityGame.SaveAndLoadSystem;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     (Re)spawns the assigned cube prefab on load and when redstone-powered.
    /// </summary>
    public class CubeSpawner : RedstoneComponent, ISaveData<CubeSpawner.SaveData>
    {
        [Tooltip("Cube prefab to spawn")]
        public GameObject Cube;

        GameObject _currentCube;
        bool _isPowered;
        Vector3 SpawnPosition => transform.position + 1.0f * transform.up;

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
            if (_currentCube != null)
                Destroy(_currentCube);


            _currentCube = Instantiate(
                Cube,
                SpawnPosition,
                transform.rotation,
                transform
            );

            if (_currentCube.TryGetComponent<GravityModifier>(out var gm)) {
                gm.GravityDirection = -transform.up;
                gm.ShouldBeSaved = false;
            }
        }

    #region Save and Load

        [Serializable]
        public struct SaveData
        {
            public bool IsSpawned;
            public Vector3 CubePosition;
            public Quaternion CubeRotation;
            public Vector3 CubeGravity;
        }

        public SaveData Save()
        {
            if (!_currentCube) return new SaveData { IsSpawned = false };
            return new SaveData {
                IsSpawned = true,
                CubePosition = _currentCube.transform.position,
                CubeRotation = _currentCube.transform.rotation,
                CubeGravity = _currentCube.TryGetComponent<GravityModifier>(out var gm) ? gm.Save() : Vector3.zero
            };
        }

        public void Load(SaveData data)
        {
            if (data.IsSpawned) {
                Respawn();
                if (_currentCube.TryGetComponent<Rigidbody>(out var rb)) {
                    rb.position = data.CubePosition;
                    rb.rotation = data.CubeRotation;
                }
                if (_currentCube.TryGetComponent<GravityModifier>(out var gravityModifier)) {
                    gravityModifier.Load(data.CubeGravity);
                }
            } else {
                if (_currentCube) Destroy(_currentCube);
            }
        }

        [field: SerializeField] public int SaveDataID { get; set; }

    #endregion
    }
}