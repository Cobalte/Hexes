using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour {

    public bool AudioEnabled { get; private set; }

    public GameObject AudioOnImage;
    public GameObject AudioOffImage;
    
    private AudioSource audioSource;
    private bool isInitialized;

    //--------------------------------------------------------------------------------------------------------
    private void Start() {
        if (!isInitialized) {
            Initialize();
        }
    }

    //--------------------------------------------------------------------------------------------------------
    private void Initialize() {

        audioSource = GetComponent<AudioSource>();
        isInitialized = true;
    }

    //--------------------------------------------------------------------------------------------------------
    public void Play(AudioClip audioClip) {
        if (!isInitialized) {
            Initialize();
        }
        
        if (!AudioEnabled) {
            return;
        }

        audioSource.PlayOneShot(audioClip);
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void SetAudioEnabled(bool isEnabled) {
        AudioOnImage.SetActive(isEnabled);
        AudioOffImage.SetActive(!isEnabled);

        AudioSource[] allAudioSources = FindObjectsByType<AudioSource>(FindObjectsSortMode.None);
        foreach (AudioSource source in allAudioSources) {
            source.Stop();
        }
        
        AudioListener.pause = !isEnabled;
        AudioListener.volume = isEnabled ? 1f : 0f;
        
        AudioEnabled = isEnabled;
    }
    
    //--------------------------------------------------------------------------------------------------------
    public void ToggleAudio() {
        SetAudioEnabled(!AudioEnabled);
    }
}
