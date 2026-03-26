using Dirigible.SystemsControl;
using Domains.Gameplay.DirigibleFlight;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Dirigible.Controllers
{
    public class DirigibleEffectsController : MonoBehaviour
    {
        [Header("Components")] public DirigibleLiftRotor[] dirigibleLiftRotors;

        public RotorHousingTilt[] rotorHousingTilts;
        public RearPropellerController rearPropellerController;

        public DirigibleMovementController movementController;

        // public RotorAudioCtrl rotorAudioCtrl;
        [SerializeField] private AudioSource engineLoopSource;


        [Header("Feedbacks")] public MMFeedbacks turnOffEngineFeedbacks;

        public MMFeedbacks turnOnEngineFeedbacks;

        [SerializeField] public bool isEngineOn;
        private float smoothedEnginePitch;

        // Add these private fields to your DirigibleEffectsController
        private float smoothedEngineVolume;


        // Update is called once per frame
        private void Update()
        {
            if (!isEngineOn) return; // Only update if the engine is on
            var status = movementController.GetStatus();
            rearPropellerController.UpdatePropeller(status);


            foreach (var rotor in dirigibleLiftRotors) rotor.UpdateRotor(status);

            foreach (var tilt in rotorHousingTilts) tilt.UpdateTilt(status);

            UpdateEngineLoop(status);
        }

        private void OnEnable()
        {
            isEngineOn = true;


            // Start engine loop if not already playing
            if (engineLoopSource != null && !engineLoopSource.isPlaying)
            {
                engineLoopSource.loop = true;
                engineLoopSource.Play();
            }

            turnOnEngineFeedbacks?.PlayFeedbacks();
        }

        private void OnDisable()
        {
            isEngineOn = false;

            // Stop engine loop
            if (engineLoopSource != null && engineLoopSource.isPlaying) engineLoopSource.Stop();

            turnOffEngineFeedbacks?.PlayFeedbacks();
        }

        private void UpdateEngineLoop(DirigibleStatus status)
        {
            if (engineLoopSource == null) return;

            // Normalize thrust (0 = idle, 1 = full forward thrust)
            var normalized = Mathf.InverseLerp(0f, 1500f, Mathf.Abs(status.currentThrust));

            // Target values based on thrust
            var targetVolume = Mathf.Lerp(0.3f, 1f, normalized);
            var targetPitch = Mathf.Lerp(0.9f, 1.4f, normalized);

            // Smooth toward targets over time
            var smoothSpeed = 2f; // smaller = slower ramp, larger = quicker
            smoothedEngineVolume = Mathf.Lerp(smoothedEngineVolume, targetVolume, Time.deltaTime * smoothSpeed);
            smoothedEnginePitch = Mathf.Lerp(smoothedEnginePitch, targetPitch, Time.deltaTime * smoothSpeed);

            // Apply to AudioSource
            engineLoopSource.volume = smoothedEngineVolume;
            engineLoopSource.pitch = smoothedEnginePitch;
        }

        // private void UpdateEngineLoop(DirigibleStatus status)
        // {
        //     if (engineLoopSource == null) return;
        //
        //     // Normalize thrust (0 = idle, 1 = full forward thrust)
        //     var normalized = Mathf.InverseLerp(0f, 1500f, Mathf.Abs(status.currentThrust));
        //
        //     // Apply to audio
        //     engineLoopSource.volume = Mathf.Lerp(0.3f, 1f, normalized); // base hum to loud roar
        //     engineLoopSource.pitch = Mathf.Lerp(0.9f, 1.4f, normalized); // deeper idle to strained whine
        // }
    }
}