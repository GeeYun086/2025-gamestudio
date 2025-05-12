using UnityEngine;
using GravityGame.Puzzle_Elements;

namespace GravityGame.Player
{
    /// <summary>
    ///     Checks in update with a raycast to see if the player is looking at an interactable object
    ///     If the player looks at an interactiable object, and presses the interact key, the object will be interacted with
    /// </summary>
    public class PlayerInteraction : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float interactDistance = 3f;
        [SerializeField] private KeyCode interactKey = KeyCode.E;
        [SerializeField] private LayerMask interactableLayer;
        
        [Header("References")]
        [SerializeField] private Camera playerCamera;
        [SerializeField] private bool showDebugRays = true;
        
        private IInteractable currentInteractable;

        private void Update()
        {
            CheckForInteractables();
            
            if (Input.GetKeyDown(interactKey) && currentInteractable != null && currentInteractable.IsInteractable)
            {
                currentInteractable.Interact();
            }
        }
        
        private void CheckForInteractables()
        {
            if (playerCamera == null) return;
            
            Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
            bool hitSomething = Physics.Raycast(ray, out RaycastHit hit, interactDistance, interactableLayer);
            
            if (showDebugRays)
            {
                Debug.DrawRay(ray.origin, ray.direction * interactDistance, 
                    hitSomething ? Color.green : Color.red, 0.1f);
            }

            currentInteractable = hitSomething ? hit.collider.GetComponent<IInteractable>() : null;
        }
    }
}