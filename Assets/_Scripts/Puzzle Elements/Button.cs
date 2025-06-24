using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Sends a short power pulse to one or more RedstoneComponents when pressed,
    ///     and tints the button green while active, red when idle.
    /// </summary>
    public class Button : MonoBehaviour
    {
        [SerializeField] GameObject _buttonOn;
        [SerializeField] GameObject _buttonOff;
        
        [Header("Redstone Targets")]
        [Tooltip("All components that should receive the pulse.")]
        public List<RedstoneComponent> Targets = new();

        [Tooltip("How long (seconds) the button stays ‘on’")]
        public float PulseDuration = 0.2f;
        
        Coroutine _pulseRoutine;

        void Start()
        {
            if(Targets.Count == 0) Debug.LogWarning($"{gameObject.name} has no connected redstone components, did you forget to add them?");
        }

        void SetPowered(bool value)
        {
            _buttonOn.SetActive(value);
            _buttonOff.SetActive(!value);
            // Update connected components
            foreach (var component in Targets.Where(c => c != null)) {
                component.IsPowered = value;
            }
        }
        
        /// <summary>
        ///     Called by an Interactable event. Ignored if a pulse is still active.
        /// </summary>
        public void Press()
        {
            if (_pulseRoutine != null) return; // ignore rapid re-presses

            // Only pulse components that are not null
            Targets.RemoveAll(t => t == null);
            if (Targets.Count == 0) return;

            _pulseRoutine = StartCoroutine(PulseCoroutine());
        }

        IEnumerator PulseCoroutine()
        {
            SetPowered(true);
            yield return new WaitForSeconds(PulseDuration);
            SetPowered(false);
            _pulseRoutine = null;
        }
    }
}