using System.Collections.Generic;
using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager
{
    public class AudioManager : MonoBehaviour, MMEventListener<AudioEvent>
    {
        [Header("Audio Sources")] public AudioSource UIAudioSource;
        public AudioSource GlobalAmbientAudioSource;
        public AudioSource OtherEnvironmentalAudioSource;


        [Header("MMAudio Settings")] public MMSoundManager SoundManager;

        List<AudioSource> _audioSources = new();
        public static AudioManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(AudioEvent eventType)
        {
            switch (eventType.EventType)
            {
                case AudioEventType.PauseAudio:
                    PauseAudio();
                    break;
                case AudioEventType.UnPauseAudio:
                    UnPauseAudio();
                    break;
            }
        }

        void UnPauseAudio()
        {
            foreach (var audioSource in _audioSources)
                if (audioSource != null && audioSource != UIAudioSource)
                    audioSource.UnPause();

            UIAudioSource.UnPause();
            GlobalAmbientAudioSource.UnPause();
            OtherEnvironmentalAudioSource.UnPause();
            SoundManager.PlayAllSounds();
        }

        void PauseAudio()
        {
            _audioSources = new List<AudioSource>(FindObjectsByType<AudioSource>(FindObjectsSortMode.None));
            foreach (var audioSource in _audioSources)
                if (audioSource != null && audioSource != UIAudioSource)
                    audioSource.Pause();

            UIAudioSource.Pause();
            GlobalAmbientAudioSource.Pause();
            OtherEnvironmentalAudioSource.Pause();
            SoundManager.StopAllSounds();
        }
    }
}
