using System;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Smoothly changes the camera FOV. Used for running.
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public class DynamicFOV : MonoBehaviour
    {
        [SerializeField] float _baseFOV = 80f;
        [SerializeField] float _fovChangeSpeed = 35f;
        [SerializeField] float _sprintFovMultiplier = 1.1f;

        public enum State { Normal, Sprinting }

        public State CurrentState { get; set; }

        Camera _camera;

        float _targetFOV;

        void OnEnable()
        {
            _camera = GetComponent<Camera>();
        }

        void Update()
        {
            _targetFOV = CurrentState switch {
                State.Normal => _baseFOV,
                State.Sprinting => _baseFOV * _sprintFovMultiplier,
                _ => throw new ArgumentOutOfRangeException()
            };
            var fov = Mathf.Lerp(_camera.fieldOfView, _targetFOV, _fovChangeSpeed * Time.deltaTime);
            SetFov(fov);
        }

        void OnValidate()
        {
            SetFov(_baseFOV);
        }

        void SetFov(float fov)
        {
            foreach (var cam in GetComponentsInChildren<Camera>()) {
                cam.fieldOfView = fov;
            }
        }
    }
}