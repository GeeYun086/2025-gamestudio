using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

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
        [SerializeField]  string _interactionPrompt = "Press [E] to interact";
        [SerializeField]  bool _isInteractable = true;
        
        [Header("Events")]
        public UnityEvent OnInteract;
        
        public string InteractionPrompt => _interactionPrompt;
        public bool IsInteractable => _isInteractable;
        
        public virtual void Interact()
        {
            if (!_isInteractable) return;
            OnInteract.Invoke();
        }
        
        public void SetInteractable(bool state) => _isInteractable = state;
    }
}