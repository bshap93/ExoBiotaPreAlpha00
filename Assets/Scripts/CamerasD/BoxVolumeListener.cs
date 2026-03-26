using Helpers.Events;
using Helpers.Events.Status;
using Manager.Settings;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Volume = UnityEngine.Rendering.Volume;

namespace CamerasD
{
    public class BoxVolumeListener : MonoBehaviour, MMEventListener<GlobalSettingsEvent>,
        MMEventListener<LoadedManagerEvent>, MMEventListener<StatsStatusEvent>,
        MMEventListener<VisionAffectingStatusEffEvent>
    {
        [FormerlySerializedAs("volume")] [FormerlySerializedAs("_volume")] [SerializeField]
        Volume mainVolume;
        [SerializeField] Volume distortVolume01;
        [SerializeField] Volume floatersVolume01;
        [SerializeField] bool disableNonDithering;

        public VolumeProfile ditheringProfile;
        // public VolumeProfile cleanProfile;

        public VolumeProfile initialDistortProfile;

        public VolumeProfile distortProfile;
        public VolumeProfile floatersProfile;
        VolumeProfile _runtimeProfile;
        public BoxVolumeListener(VolumeProfile initialDistortProfile)
        {
            this.initialDistortProfile = initialDistortProfile;
        }

        void Awake()
        {
            // Clone the original profile so you don't modify the asset
            if (mainVolume.profile != null)
            {
                _runtimeProfile = Instantiate(mainVolume.profile);
                mainVolume.profile = _runtimeProfile;
            }
        }


        void OnEnable()
        {
            this.MMEventStartListening<GlobalSettingsEvent>();
            this.MMEventStartListening<LoadedManagerEvent>();
            this.MMEventStartListening<StatsStatusEvent>();
            this.MMEventStartListening<VisionAffectingStatusEffEvent>();

            SceneManager.sceneLoaded -= OnSceneLoaded;

            SceneManager.sceneLoaded += OnSceneLoaded;
        }


        void OnDisable()
        {
            this.MMEventStopListening<GlobalSettingsEvent>();
            this.MMEventStopListening<LoadedManagerEvent>();
            this.MMEventStopListening<StatsStatusEvent>();
            this.MMEventStopListening<VisionAffectingStatusEffEvent>();

            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
        public void OnMMEvent(GlobalSettingsEvent eventType)
        {
            if (eventType.EventType == GlobalSettingsEventType.DitheringToggled)
            {
                SetDithering(eventType.ChoiceIndex == 1);
                Debug.Log("Dithering set to " + (eventType.ChoiceIndex == 1));
            }
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All)
            {
                var gsm = GlobalSettingsManager.Instance;
                if (gsm != null)
                    SetDithering(gsm.DitheringEnabled);
            }
        }
        public void OnMMEvent(StatsStatusEvent eventType)
        {
            switch (eventType.StatType)
            {
                case StatsStatusEvent.StatsStatusType.Contamination:
                    HandleContaminationStatus(eventType.Enabled, eventType.Status);
                    break;
            }
        }
        public void OnMMEvent(VisionAffectingStatusEffEvent eventType)
        {
            if (eventType.Enable)
                switch (eventType.StatusEffType)
                {
                    case VisionAffectingStatusEffType.Distortion:
                        distortVolume01.profile = distortProfile;
                        distortVolume01.enabled = true;
                        break;
                    case VisionAffectingStatusEffType.Floaters:
                        floatersVolume01.profile = floatersProfile;
                        floatersVolume01.enabled = true;
                        break;
                }
            else
                switch (eventType.StatusEffType)
                {
                    case VisionAffectingStatusEffType.Distortion:
                        distortVolume01.enabled = false;
                        break;
                    case VisionAffectingStatusEffType.Floaters:
                        floatersVolume01.enabled = false;
                        break;
                    case VisionAffectingStatusEffType.All:
                        distortVolume01.enabled = false;
                        floatersVolume01.enabled = false;
                        break;
                }
        }

        void HandleContaminationStatus(bool isBeingEnabled, StatsStatusEvent.StatsStatus eventTypeStatus)
        {
            switch (eventTypeStatus)
            {
                case StatsStatusEvent.StatsStatus.IsMax:
                    // SetDistortion(isBeingEnabled);
                    break;
            }
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            var gsm = GlobalSettingsManager.Instance;
            if (gsm != null)
                SetDithering(gsm.DitheringEnabled);
        }

        void SetDithering(bool ditheringEnabled)
        {
            if (disableNonDithering) return;
            if (ditheringEnabled)
                mainVolume.profile = ditheringProfile;
            // else
            //     mainVolume.profile = cleanProfile;
        }
    }
}
