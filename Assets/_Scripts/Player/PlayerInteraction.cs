using GravityGame.Puzzle_Elements;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    /// Pressing the interact key attempts to release a carried object if held,
    /// otherwise, it will try to interact with or pick up an object the player is looking at.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] float _interactDistance = 3f;
        [SerializeField] KeyCode _interactKey = KeyCode.E;
        [SerializeField] LayerMask _interactableLayer;

        [Header("References")]
        Camera _playerCamera;
        PlayerCarry _playerCarry;

        IInteractable _currentlyAimedInteractable;

        void Awake()
        {
            _playerCarry = GetComponent<PlayerCarry>();
            _playerCamera = GetComponentInChildren<Camera>(true);
        }

        void Update()
        {
            CheckForAimedInteractable();

            if (Input.GetKeyDown(_interactKey)) {
                if (_playerCarry && _playerCarry.IsCarrying()) {
                    _playerCarry.AttemptRelease();
                } else if (_currentlyAimedInteractable is { IsInteractable: true }) {
                    _currentlyAimedInteractable.Interact();
                }
            }
        }

        void CheckForAimedInteractable()
        {
            if (!_playerCamera) return;

            var ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            bool hitSomething = Physics.Raycast(ray, out var hit, _interactDistance, _interactableLayer);

            Debug.DrawRay(
                ray.origin, ray.direction * _interactDistance,
                hitSomething ? Color.green : Color.red, 0.1f
            );

            _currentlyAimedInteractable = hitSomething ? hit.collider.GetComponent<IInteractable>() : null;
        }
    }
}