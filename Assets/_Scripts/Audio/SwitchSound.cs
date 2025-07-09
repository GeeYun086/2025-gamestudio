using UnityEngine;

/// <summary>
/// Plays the assigned on/off audio clips when its PlayOn() or PlayOff() methods are called.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class SwitchSound : MonoBehaviour
{
    [Header("Switch Sounds")]
    [Tooltip("Audio clip to play when switched on")]
    [SerializeField] AudioClip _clipOn;
    
    [Tooltip("Audio clip to play when switched off")]
    [SerializeField] AudioClip _clipOff;

    AudioSource _src;

    void OnEnable()
    {
        _src = GetComponent<AudioSource>();
    }

    
    public void PlayOn()
    {
        if (_clipOn != null)
            _src?.PlayOneShot(_clipOn);
    }

    
    public void PlayOff()
    {
        if (_clipOff != null)
            _src?.PlayOneShot(_clipOff);
    }
}