using System;
using System.Collections;
using JetBrains.Annotations;
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
        [SerializeField] float _minPlayTime = 10;

        [SerializeField] [Range(0, 1)] float _ambientDefaultVolume = 1;

        AudioSource _audioSource1;
        AudioSource _audioSource2;
        [CanBeNull] AudioClip _audioClipInQueue;
        
        bool _canMusicBeChanged = true;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            if (_crossfadeOverlapTime > _transitionTime) {
                _crossfadeOverlapTime = _transitionTime;
            }

            SetupAudioSources();
            _audioSource1.clip = _defaultAmbientMusic;
            StartCoroutine(FadeIn(_audioSource1));
        }

        void SetupAudioSources()
        {
            var audioSources = GetComponents<AudioSource>();
            if (audioSources.Length != 2) {
                throw new Exception("The AmbientMusicManager needs to be attached on a GameObject with exactly two audio sources in order to work.");
            }
            _audioSource1 = audioSources[0];
            _audioSource2 = audioSources[1];
        }

        public void ChangeTrack(AudioClip audioClip, bool forceChange = false)
        {
            if (!_canMusicBeChanged && !forceChange) {
                _audioClipInQueue = audioClip;
                return;
            }

            if (_audioSource1.isPlaying) {
                if (_audioSource1.clip != audioClip) {
                    _audioSource2.clip = audioClip;
                    StartCoroutine(CrossfadeAudioSources(_audioSource1, _audioSource2));
                    StartCoroutine(MusicChangeCooldown());
                }
            } else {
                if (_audioSource2.clip != audioClip) {
                    _audioSource1.clip = audioClip;
                    StartCoroutine(CrossfadeAudioSources(_audioSource2, _audioSource1));
                    StartCoroutine(MusicChangeCooldown());
                }
            }

        }

        // TODO call this method, when the music volume is set in the settings
        public void SetAmbientVolume(float volume)
        {
            if (volume > 1) {
                volume = 1;
            }

            _ambientDefaultVolume = volume;
            _audioSource1.volume = volume;
            _audioSource2.volume = volume;
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

        IEnumerator MusicChangeCooldown()
        {
            _canMusicBeChanged = false;
            yield return new WaitForSeconds(_minPlayTime);
            _canMusicBeChanged = true;

            if (_audioClipInQueue != null) {
                ChangeTrack(_audioClipInQueue);
                _audioClipInQueue = null;
            }
        }
    }
}