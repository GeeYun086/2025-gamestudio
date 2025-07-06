using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class SwitchSound : MonoBehaviour
{
    [Header("Switch-Sounds")]
    [Tooltip("Sound beim Einschalten")]
    [SerializeField] private AudioClip _clipOn;
    [Tooltip("Sound beim Ausschalten")]
    [SerializeField] private AudioClip _clipOff;

    private AudioSource _src;

    void Awake()
    {
        _src = GetComponent<AudioSource>();
        _src.playOnAwake = false;
        // räumlicher 3D-Sound (kann weg, wenn 2D-Sound gewünscht)
        _src.spatialBlend = 1f;
    }

    /// <summary>Wird aufgerufen, wenn der Hebel an geht.</summary>
    public void PlayOn()
    {
        if (_clipOn != null)
            _src.PlayOneShot(_clipOn);
    }

    /// <summary>Wird aufgerufen, wenn der Hebel aus geht.</summary>
    public void PlayOff()
    {
        if (_clipOff != null)
            _src.PlayOneShot(_clipOff);
    }
}