using GravityGame.Puzzle_Elements;
using GravityGame.Utils;
using UnityEngine;

namespace GravityGame.Player
{
    /// <summary>
    ///     Pressing the interact key attempts to release a carried object if held,
    ///     otherwise, it will try to interact with or pick up an object the player is looking at.
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] float _interactDistance = 3f;
        [SerializeField] KeyCode _interactKey = KeyCode.E;
        [SerializeField] LayerMask _interactableLayer;
        [SerializeField] Timer _interactBuffer = new(0.5f);


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
                _interactBuffer.Start();
                Debug.Log("Interact");
            }

            if (_interactBuffer.IsActive) {
                if (_playerCarry.AttemptRelease()) {
                    _interactBuffer.Stop();
                } else if (_currentlyAimedInteractable is { IsInteractable: true }) {
                    _currentlyAimedInteractable.Interact();
                    _interactBuffer.Stop();
                    Debug.Log("consume");
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