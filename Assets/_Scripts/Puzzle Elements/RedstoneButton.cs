using System.Collections;
using UnityEngine;

namespace GravityGame.Puzzle_Elements
{
    /// <summary>
    ///     Sends a short power pulse to a target RedstoneComponent when pressed.
    /// </summary>
    public class RedstoneButton : MonoBehaviour
    {
        [Tooltip("The RedstoneComponent that will receive the pulse.")]
        public RedstoneComponent Target;

        [Tooltip("How long the power stays on (seconds).")]
        public float PulseDuration = 0.2f;

        /// <summary>
        ///     Call this (e.g. via an Interactable event) to press the button.
        /// </summary>
        public void Press()
        {
            if (Target != null) {
                StartCoroutine(PowerPulse());
            }
        }

        IEnumerator PowerPulse()
        {
            Target.IsPowered = true;
            yield return new WaitForSeconds(PulseDuration);
            Target.IsPowered = false;
        }
    }
}