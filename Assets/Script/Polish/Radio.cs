using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Radio : MonoBehaviour
{
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip[] _audioClips;
    [SerializeField] private AudioClip _transitionEffect;
    private int currentIndex = 0;

    private void Start()
    {
        if (_audioClips.Length>0)
        {
            PlayCurrentTrack();
        }
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            Transition();
            PreviusSong();
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            Transition();
            NextSong();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            StopTrack();
        }
    }
    
    private void PlayCurrentTrack()
    {
        if (_audioClips[currentIndex] != null)
        {
            _audioSource.clip = _audioClips[currentIndex];
            _audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Şarkı yok");
        }
    }

    public void NextSong()
    {
        Debug.Log("Next Song Tıklandı");
        currentIndex++;
        if (currentIndex >= _audioClips.Length)
        {
            currentIndex = 0; // Liste sonuna gelince başa dön
        }
        PlayCurrentTrack();
    }
    
    public void PreviusSong()
    {
        Debug.Log("Previus Song Tıklandı");
        currentIndex--;
        if (currentIndex >= _audioClips.Length)
        {
            currentIndex = 0; // Liste sonuna gelince başa dön
        }
        PlayCurrentTrack();
    }
    
    public void StopTrack()
    {
        _audioSource.Stop();
    }

    private void Transition()
    {
        _audioSource.PlayOneShot(_transitionEffect);
    }
    
}
