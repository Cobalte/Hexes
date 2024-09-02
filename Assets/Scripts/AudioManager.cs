using System;
using System.Collections;
using System.Collections.Generic;
using E7.Native;
using UnityEngine;
using UnityEngine.Serialization;

public class AudioManager : MonoBehaviour {

    public bool AudioEnabled { get; private set; }

    public GameObject AudioOnImage;
    public GameObject AudioOffImage;
    
    private AudioSource audioSource;
    private NativeSource nativeSource;
    private NativeAudioPointer audioPointer;
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
        return;

        // everything below here is old, from the time I needed to use native audio 
        
#if UNITY_EDITOR
        audioSource = GetComponent<AudioSource>();
#else
        try {
            DeviceAudioInformation deviceAudioInformation = NativeAudio.GetDeviceAudioInformation();
            NativeAudio.Initialize(new NativeAudio.InitializationOptions {
                androidAudioTrackCount = 2,
                androidBufferSize = deviceAudioInformation.optimalBufferSize
            });    
        }
        catch (Exception ex) {
            Debug.LogError("Sushi Audio Error: " + ex);
        }
#endif
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
        return;
        
        // everything below here is old, from the time I needed to use native audio
        
#if UNITY_EDITOR
        audioSource.PlayOneShot(audioClip);
#else
        try {
            if (audioPointer != null) {
                audioPointer.Unload();
                audioPointer = null;
            }
            
            audioPointer = NativeAudio.Load(audioClip);
            Debug.Log($"Sushi Audio: Loaded audio of length {audioPointer.Length}");
            
            nativeSource = NativeAudio.GetNativeSourceAuto();
            var playOptions = NativeSource.PlayOptions.defaultOptions;
            playOptions.volume = 0.3f;
            playOptions.pan = 1f;
            nativeSource.Play(audioPointer, playOptions);
        }
        catch (Exception ex) {
            Debug.Log("Sushi Audio Error: " + ex);
        }  
#endif

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
