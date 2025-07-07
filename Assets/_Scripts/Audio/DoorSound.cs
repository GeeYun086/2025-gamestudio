using UnityEngine;

namespace GravityGame.Audio
{
    /// <summary>
    /// Plays door open/close clips at the door’s position,
    /// using PlayClipAtPoint so the sound continues even if the door is disabled.
    /// </summary>
    public class DoorSound : MonoBehaviour
    {
        [Header("Door Sounds")]
        [Tooltip("Sound played when the door opens")]
        [SerializeField] private AudioClip _openClip;
        [Tooltip("Sound played when the door closes")]
        [SerializeField] private AudioClip _closeClip;

        /// <summary>Call to play the "open" sound.</summary>
        public void PlayOpen()
        {
            if (_openClip != null)
                AudioSource.PlayClipAtPoint(_openClip, transform.position);
        }

        /// <summary>Call to play the "close" sound.</summary>
        public void PlayClose()
        {
            if (_closeClip != null)
                AudioSource.PlayClipAtPoint(_closeClip, transform.position);
        }
    }
}