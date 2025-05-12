using UnityEngine;
using UnityEngine.Events;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Base class for all interactable objects in the game.
    /// Implements the IInteractable interface to provide consistent interaction behavior.
    /// When implemented on an object, allows player to interact via raycast detection and key press.
    /// </summary>
    public class InteractableObject : MonoBehaviour, IInteractable
    {
        [Header("Interaction Settings")]
        [SerializeField] private string interactionPrompt = "Press [E] to interact";
        [SerializeField] private bool isInteractable = true;
        
        [Header("Events")]
        public UnityEvent onInteract;
        
        public string InteractionPrompt => interactionPrompt;
        public bool IsInteractable => isInteractable;
        
        public virtual void Interact()
        {
            if (!isInteractable) return;
            onInteract.Invoke();
        }
        
        public void SetInteractable(bool state) => isInteractable = state;
    }
}