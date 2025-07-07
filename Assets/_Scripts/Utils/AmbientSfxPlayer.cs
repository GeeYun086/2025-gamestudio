using System.Collections;
using UnityEngine;

namespace GravityGame.Utils
{
    /// <summary>
    /// Plays random sound effect from a list at random intervals.
    /// </summary>
    [RequireComponent(typeof(AudioSource))]
    public class AmbientSfxPlayer : MonoBehaviour
    {
        [Header("Audio Clips")]
        public AudioClip[] AudioClips;

        [Header("Sound Settings")]
        [Range(0.1f, 3f)] public float MinPitch = 1.0f;
        [Range(0.1f, 3f)] public float MaxPitch = 1.0f;
        [Range(0f, 5f)] public float MinVolume = 0.8f;
        [Range(0f, 5f)] public float MaxVolume = 1.2f;

        [Header("Timing")]
        public bool PlayOnStart;
        public float MinDelayPlayInterval = 5f;
        public float MaxDelayPlayInterval = 20f;

        AudioSource _audioSource;

        void Reset()
        {
            var source = GetComponent<AudioSource>();
            source.spatialBlend = 1.0f;
            // Note FS: For realism this should be Logarithmic
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 0.0f;
            source.maxDistance = 30.0f;
            source.playOnAwake = false;
        }

        void Awake() => _audioSource = GetComponent<AudioSource>();

        void Start() => StartCoroutine(PlayAmbientSounds());

        IEnumerator PlayAmbientSounds()
        {
            if (!PlayOnStart) yield return new WaitForSeconds(Random.Range(MinDelayPlayInterval, MaxDelayPlayInterval));

            while (true) {
                PlayRandomSound();
                float randomDelay = Random.Range(MinDelayPlayInterval, MaxDelayPlayInterval);
                yield return new WaitForSeconds(randomDelay);
            }
        }

        void PlayRandomSound()
        {
            if (AudioClips == null || AudioClips.Length == 0) {
                Debug.LogWarning($"No audio clips assigned {gameObject.name}");
                return;
            }

            var randomClip = AudioClips[Random.Range(0, AudioClips.Length)];
            _audioSource.pitch = Random.Range(MinPitch, MaxPitch);
            float randomVolume = Random.Range(MinVolume, MaxVolume);

            _audioSource.PlayOneShot(randomClip, randomVolume);
        }
    }
}