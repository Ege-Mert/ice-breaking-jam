using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Try : MonoBehaviour
{
    public AudioSource audioSource; // Şarkıları çalacak AudioSource
    public AudioClip[] audioClips;  // Şarkı listesi
    private int currentTrackIndex = 0; // Şu anki şarkının indeksi

    void Start()
    {
        if (audioClips.Length > 0)
        {
            PlayCurrentTrack();
        }
        else
        {
            Debug.LogWarning("AudioClips listesi boş!");
        }
    }

    // Mevcut şarkıyı çalma
    private void PlayCurrentTrack()
    {
        if (audioClips[currentTrackIndex] != null)
        {
            audioSource.clip = audioClips[currentTrackIndex];
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("Seçilen şarkı boş!");
        }
    }

    // Sonraki şarkıya geç
    public void PlayNextTrack()
    {
        currentTrackIndex++;
        if (currentTrackIndex >= audioClips.Length)
        {
            currentTrackIndex = 0; // Liste sonuna gelince başa dön
        }
        PlayCurrentTrack();
    }

    // Önceki şarkıya geç
    public void PlayPreviousTrack()
    {
        currentTrackIndex--;
        if (currentTrackIndex < 0)
        {
            currentTrackIndex = audioClips.Length - 1; // Listenin sonuna dön
        }
        PlayCurrentTrack();
    }

    // Şarkıyı durdur
    public void StopTrack()
    {
        audioSource.Stop();
    }
}
