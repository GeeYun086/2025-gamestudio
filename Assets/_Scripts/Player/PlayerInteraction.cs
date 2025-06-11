using UnityEngine;
using GravityGame.Puzzle_Elements;
using UnityEngine.Serialization;

namespace GravityGame.Player
{
    /// <summary>
    ///     Checks in update with a raycast to see if the player is looking at an interactable object
    ///     If the player looks at an interactiable object, and presses the interact key, the object will be interacted with
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        
        [Header("Settings")]
        [SerializeField]  float _interactDistance = 3f; 
        [SerializeField]  KeyCode _interactKey = KeyCode.E;
        [SerializeField]  LayerMask _interactableLayer;
        
        
        [Header("References")]
        [SerializeField]  Camera _playerCamera;
        [SerializeField]  bool _showDebugRays = true;
        
         IInteractable _currentInteractable;

         void Update()
        {
            CheckForInteractables();
            
            if (Input.GetKeyDown(_interactKey) && _currentInteractable != null && _currentInteractable.IsInteractable)
            {
                _currentInteractable.Interact();
            }
        }
        
         void CheckForInteractables()
        {
            if (_playerCamera == null) return;
            
            Ray ray = new Ray(_playerCamera.transform.position, _playerCamera.transform.forward);
            bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, _interactDistance, _interactableLayer);
            
            
            if (_showDebugRays)
            {
                Debug.DrawRay(ray.origin, ray.direction * _interactDistance, 
                    hitSomething ? Color.green : Color.red, 0.1f);
            }

            _currentInteractable = hitSomething ? hit.collider.GetComponent<IInteractable>() : null;
            
        }
    }
}