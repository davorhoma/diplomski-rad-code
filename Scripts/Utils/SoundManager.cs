using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip _audioClip;
    [SerializeField] private AudioClip _twoDiceAudio;
    [SerializeField] private AudioClip _audioGrabDice;

    public void PlaySoundHitBoard(float volumeScale)
    {
        _audioSource.PlayOneShot(_audioClip, volumeScale);
    }

    public void PlaySoundHitDice(float volumeScale)
    {
        _audioSource.PlayOneShot(_twoDiceAudio, volumeScale);
    }

    public void PlaySoundGrabDice(float volumeScale = 1.0f)
    {
        _audioSource.PlayOneShot(_audioGrabDice, volumeScale);
    }
}
