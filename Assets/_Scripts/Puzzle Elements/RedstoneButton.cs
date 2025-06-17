using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    /// Sends a short power pulse to one or more RedstoneComponents when pressed,
    /// and tints the button green while active, red when idle.
    /// </summary>
   
    public class RedstoneButton : MonoBehaviour
    {
        [Header("Redstone Targets")]
        [Tooltip("All components that should receive the pulse.")]
        public List<RedstoneComponent> Targets = new List<RedstoneComponent>();

        [Tooltip("How long (seconds) the button stays ‘on’")]
        public float PulseDuration = 0.2f;

        [Header("Visuals")]
        [Tooltip("Renderer on the button mesh to tint.")]
        public MeshRenderer ButtonRenderer;

        [Tooltip("Idle tint color.")]
        public Color IdleColor = Color.red;

        [Tooltip("Tint while powered.")]
        public Color ActiveColor = Color.green;

        private Coroutine _pulseRoutine;

        void Awake()
        {
            // Auto-assign the renderer if missing
            if (ButtonRenderer == null)
                ButtonRenderer = GetComponentInChildren<MeshRenderer>();

            if (ButtonRenderer != null)
            {
                // Instance the material so we can safely tint
                ButtonRenderer.material = new Material(ButtonRenderer.material);
                ButtonRenderer.material.color = IdleColor;
            }
        }

        /// <summary>
        /// Called by an Interactable event. Ignored if a pulse is still active.
        /// </summary>
        public void Press()
        {
            if (_pulseRoutine != null) return;  // ignore rapid re-presses

            // Only pulse components that are not null
            Targets.RemoveAll(t => t == null);
            if (Targets.Count == 0) return;

            _pulseRoutine = StartCoroutine(PulseCoroutine());
        }

        private IEnumerator PulseCoroutine()
        {
            // Visual: active color
            if (ButtonRenderer != null)
                ButtonRenderer.material.color = ActiveColor;

            // Power on all targets
            foreach (var t in Targets)
                t.IsPowered = true;

            yield return new WaitForSeconds(PulseDuration);

            // Power off all targets
            foreach (var t in Targets)
                t.IsPowered = false;

            // Visual: back to idle
            if (ButtonRenderer != null)
                ButtonRenderer.material.color = IdleColor;

            _pulseRoutine = null;
        }
    }
}
