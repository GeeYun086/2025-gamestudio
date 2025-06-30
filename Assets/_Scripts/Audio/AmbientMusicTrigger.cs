using System;
using UnityEngine;

namespace GravityGame._Scripts.Audio
{
    /// <summary>
    /// The AmbientMusicTrigger script can be attached to any trigger volume.
    /// It will play the specified audio clip as ambient background music, when a player enters the trigger.
    /// The scene needs to have an AmbientMusicManager to play the background music.
    /// </summary>
    public class AmbientMusicTrigger : MonoBehaviour
    {

        [SerializeField] AudioClip _targetAudio;
        
        AmbientMusicManager _ambientMusicManager;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _ambientMusicManager = FindAnyObjectByType<AmbientMusicManager>();
        }

        void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.CompareTag("Player")) {
                _ambientMusicManager.ChangeTrack(_targetAudio);
            }
        }
    }
}
