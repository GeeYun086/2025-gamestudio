using System;
using System.Collections;
using UnityEngine;

namespace GravityGame._Scripts.Audio
{
    /// <summary>
    ///     This class manages the background music and the transitions between different background tracks.
    ///     The script has to be attached on a GameObject with two audio sources
    /// </summary>
    public class AmbientMusicManager : MonoBehaviour
    {
        [SerializeField] AudioClip _defaultAmbientMusic;
        [SerializeField] float _transitionTime = 5;
        [SerializeField] float _crossfadeOverlapTime = 3;

        // TODO connect this value to the music value set in the settings
        [SerializeField] [Range(0, 1)] float _ambientDefaultVolume;

        AudioSource _audioSource1;
        AudioSource _audioSource2;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_crossfadeOverlapTime > _transitionTime) {
                _crossfadeOverlapTime = _transitionTime;
            }

            SetAudioSources();
            StartCoroutine(FadeIn(_audioSource1));
        }

        void SetAudioSources()
        {
            var audioSources = GetComponents<AudioSource>();
            if (audioSources.Length != 2) {
                throw new Exception("The AmbientMusicManager needs exactly two audio sources on the same GameObject in order to work.");
            }
            _audioSource1 = audioSources[0];
            _audioSource2 = audioSources[1];
        }

        public void ChangeTrack(AudioClip audioClip)
        {
            if (_audioSource1.isPlaying) {
                if (_audioSource1.clip != audioClip) {
                    _audioSource2.clip = audioClip;
                    StartCoroutine(CrossfadeAudioSources(_audioSource1, _audioSource2));
                }
            } else {
                if (_audioSource2.clip != audioClip) {
                    _audioSource1.clip = audioClip;
                    StartCoroutine(CrossfadeAudioSources(_audioSource2, _audioSource1));
                }
            }
        }

        IEnumerator CrossfadeAudioSources(AudioSource activeSource, AudioSource targetSource)
        {
            float fadeDuration = _transitionTime / 2;
            float fadeInDelay = (_transitionTime - _crossfadeOverlapTime) / 2;

            targetSource.volume = 0;
            targetSource.Play();

            float activeSourceVolumePercentage = 0;
            float targetSourceVolumePercentage = 0;
            float elapsedTime = 0;
            while (elapsedTime < _transitionTime) {
                elapsedTime += Time.deltaTime;

                if (activeSource.volume > 0) {
                    activeSource.volume = Mathf.Lerp(_ambientDefaultVolume, 0, activeSourceVolumePercentage);
                    activeSourceVolumePercentage += Time.deltaTime / fadeDuration;
                }

                if (elapsedTime >= fadeInDelay) {
                    targetSource.volume = Mathf.Lerp(0, _ambientDefaultVolume, targetSourceVolumePercentage);
                    targetSourceVolumePercentage += Time.deltaTime / fadeDuration;
                }
                yield return null;
            }
            targetSource.volume = _ambientDefaultVolume;
            activeSource.Stop();
        }

        IEnumerator FadeIn(AudioSource audioSource)
        {
            audioSource.volume = 0;
            audioSource.Play();

            float percentage = 0;
            while (audioSource.volume < _ambientDefaultVolume) {
                audioSource.volume = Mathf.Lerp(0, _ambientDefaultVolume, percentage);
                percentage += Time.deltaTime / _transitionTime;

                yield return null;
            }
        }
    }
}