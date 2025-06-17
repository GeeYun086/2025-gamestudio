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
        [SerializeField] float _maxVerticalRotation = 50f;

        Carryable _currentlyCarrying;
        PlayerMovement _playerMovement;
        FirstPersonCameraController _cameraController;
        Collider[] _playerColliders;

        bool _isYPositionFrozen;
        float _yOffset;

        void Awake()
        {
            _playerMovement = GetComponent<PlayerMovement>();
            _cameraController = GetComponentInChildren<FirstPersonCameraController>();
            _playerColliders = GetComponentsInChildren<Collider>();
        }

        public bool IsCarrying() => _currentlyCarrying;
        
        public void AttemptPickUp(Carryable objectToCarry)
        {
            if (IsCarrying() || !objectToCarry || objectToCarry.GetComponent<Rigidbody>().mass > _maxCarryMass) return;

            _currentlyCarrying = objectToCarry;
            _currentlyCarrying.PickUp(_carryPointTransform);
            IgnorePlayerCollision(true);
        }
        
        public void AttemptRelease()
        {
            if (IsCarrying()) {
                _isYPositionFrozen = false;
                IgnorePlayerCollision(false);
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
            }
        }
        
        void LateUpdate()
        {
            if (!IsCarrying()) {
                _isYPositionFrozen = false;
                return;
            }

            if (Mathf.Abs(_cameraController.LookDownRotation) > _maxVerticalRotation) {
                if (!_isYPositionFrozen) {
                    _isYPositionFrozen = true;
                    _yOffset = _carryPointTransform.position.y - _playerMovement.transform.position.y;
                }

                var correctedPosition = _carryPointTransform.position;
                correctedPosition.y = _playerMovement.transform.position.y + _yOffset;
                _carryPointTransform.position = correctedPosition;
            } else {
                _isYPositionFrozen = false;
            }
        }
        
        void IgnorePlayerCollision(bool ignore)
        {
            if (!_currentlyCarrying) return;
            var carriedCollider = _currentlyCarrying.GetComponent<Collider>();
            if (!carriedCollider) return;
            foreach (var playerCollider in _playerColliders) Physics.IgnoreCollision(playerCollider, carriedCollider, ignore);
        }
    }
}