using System;
using GravityGame.Puzzle_Elements;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    /// Manages the state of carrying a Carryable object.
    /// </summary>
    public class PlayerCarry : MonoBehaviour
    {
        [SerializeField] Transform _carryPointTransform;
        [SerializeField] float _maxCarryDistance = 5f;
        [SerializeField] float _maxCarryMass = 250f;
        [SerializeField] float _maxVerticalAngular = 67f;

        Carryable _currentlyCarrying;
        PlayerMovement _playerMovement;
        FirstPersonCameraController _cameraController;

        void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _cameraController = GetComponentInChildren<FirstPersonCameraController>();
        }

        public bool IsCarrying()
            => _currentlyCarrying;

        public void AttemptPickUp(Carryable objectToCarry)
        {
            if (!IsCarrying() && objectToCarry && objectToCarry.GetComponent<Rigidbody>().mass <= _maxCarryMass) {
                _currentlyCarrying = objectToCarry;
                _currentlyCarrying.PickUp(_carryPointTransform);
            }
        }

        public void AttemptRelease()
        {
            if (IsCarrying()) {
                _currentlyCarrying.Release();
                _currentlyCarrying = null;
            }
        }

        void Update()
        {
            if (IsCarrying()) {
                if (Vector3.Distance(transform.position, _currentlyCarrying.transform.position) > _maxCarryDistance) {
                    AttemptRelease();
                    return;
                }

                if (_playerMovement.Ground.Hit.collider &&
                    _playerMovement.Ground.Hit.collider.gameObject == _currentlyCarrying.gameObject) {
                    AttemptRelease();
                }

                if (Math.Abs(_cameraController.LookDownRotation) >= _maxVerticalAngular) {
                    Debug.Log("Out of range");
                } else {
                    Debug.Log("Within range");
                }
            }
        }
    }
}