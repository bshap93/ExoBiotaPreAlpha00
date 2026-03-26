using System;
using Animancer;
using CompassNavigatorPro;
using Helpers.Events;
using Helpers.Events.ManagerEvents;
using HighlightPlus;
using Manager.SceneManagers;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace Helpers.Wrappers
{
    [RequireComponent(typeof(CompassProPOI))]
    // Disallow multiple on same object to avoid confusin
    [DisallowMultipleComponent]
    public class GamePOIWrapper : MonoBehaviour, MMEventListener<LoadedManagerEvent>, MMEventListener<POIWrapperEvent>,
        IRequiresUniqueID
    {
        public enum ExaminationPolicy
        {
            Required,
            NonRequired
        }

        public enum RevealPolicy
        {
            Brief,
            StayVisible
        }

        [FormerlySerializedAs("OnScanHit")] public UnityEvent onScanHit;


        public RevealPolicy revealPolicy = RevealPolicy.Brief;

        public ExaminationPolicy examinationPolicy = ExaminationPolicy.NonRequired;


        public CanBeAreaScannedType canBeAreaScannedType;

        [FormerlySerializedAs("_compassProPOI")]
        public CompassProPOI compassProPOI;

        public SubjectLocationObject locationObject;

        HighlightEffect _highlightEffect;

        CoreGamePOIManager _manager;

        POIVisibility _usualVisibility;


        public string UniqueID
        {
            get
            {
                if (locationObject == null) return string.Empty;

                return locationObject.associatedPOIUniqueId;
            }
        }
        public void SetUniqueID()
        {
            if (locationObject == null)
                // Debug.LogWarning("LocationObject not yet set for " + gameObject.name);
                return;

            if (string.IsNullOrEmpty(locationObject.associatedPOIUniqueId))
                locationObject.associatedPOIUniqueId = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return locationObject == null || string.IsNullOrEmpty(locationObject.associatedPOIUniqueId);
        }

        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All)
                Initialize();
        }

        public void OnMMEvent(POIWrapperEvent evt)
        {
            if (locationObject == null)
            {
                Debug.LogWarning("LocationObject not yet set for " + gameObject.name);
                return;
            }

            if (evt.UniqueId != locationObject.associatedPOIUniqueId) return; // only care about self

            if (evt.Type == POIWrapperEventType.TrackedByObjective)
            {
                compassProPOI.visibility = POIVisibility.AlwaysVisible;
                compassProPOI.showOnScreenIndicator = true;
                compassProPOI.canBeVisited = true;
                compassProPOI.isVisited = false;
            }
            else if (evt.Type == POIWrapperEventType.Untracked)
            {
                // restore to usual state or hide
                compassProPOI.visibility = _usualVisibility;
                compassProPOI.showOnScreenIndicator = false;
                compassProPOI.canBeVisited = false;
                compassProPOI.isVisited = true;
            }
            else if (evt.Type == POIWrapperEventType.StateChanged)
            {
                Initialize(); // generic re-sync
            }
        }


        public void SetPOIVisitable(bool visitable, string sceneName)
        {
            if (compassProPOI == null) return;

            if (gameObject.scene.name != sceneName)
            {
                Debug.LogWarning(
                    $"POI {locationObject.associatedPOIUniqueId} tried to set visitable for {sceneName} but belongs to {gameObject.scene.name}");

                return;
            }
            // if (SceneManager.GetActiveScene().name != sceneName)
            // {
            //     // Possibly do a thing where we make link list type
            //     // structures to handle POIs across scenes
            //     Debug.LogError("GamePOIWrapper: Attempting to set POI visitable state for a scene that is not active.");
            //     return;
            // }

            if (visitable)
            {
                compassProPOI.canBeVisited = true;
                compassProPOI.isVisited = false;
            }
            else
            {
                compassProPOI.canBeVisited = false;
                compassProPOI.isVisited = true;
            }
        }

        public bool HasScannerCapability(CanBeAreaScannedType sdcAreaScannedType)
        {
            if (canBeAreaScannedType == CanBeAreaScannedType.NotDetectableByScan) return false;

            return canBeAreaScannedType == sdcAreaScannedType;
        }

        #region Lifecycle

        void Awake()
        {
            compassProPOI = GetComponent<CompassProPOI>();
            _highlightEffect = GetComponent<HighlightEffect>();
        }


        void Start()
        {
            _manager = CoreGamePOIManager.Instance ?? FindFirstObjectByType<CoreGamePOIManager>();

            _manager?.RegisterWrapper(this);

            _usualVisibility = compassProPOI.visibility;
        }

        void Initialize()
        {
            if (locationObject == null)
                // Debug.LogWarning($"LocationObject not yet set for {gameObject.name}");
                return;

            if (_manager != null && _manager.IsPOIAlwaysVisible(locationObject.associatedPOIUniqueId))
            {
                compassProPOI.visibility = POIVisibility.AlwaysVisible;
                compassProPOI.showOnScreenIndicator = true;
            }

            if ((_manager != null && _manager.IsPOILittleKnown(locationObject.associatedPOIUniqueId))
                || _manager.IsPOIWellKnown(locationObject.associatedPOIUniqueId))
            {
                compassProPOI.visibility = POIVisibility.WhenInRange;
                compassProPOI.showOnScreenIndicator = true;
            }

            // Tracked state
            if (_manager != null && _manager.IsPOITrackedByObjective(locationObject.associatedPOIUniqueId))
            {
                compassProPOI.visibility = POIVisibility.AlwaysVisible;
                compassProPOI.showOnScreenIndicator = true;
                compassProPOI.canBeVisited = true;
                compassProPOI.isVisited = false;
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<LoadedManagerEvent>();
            this.MMEventStartListening<POIWrapperEvent>();
            if (_manager == null) _manager = CoreGamePOIManager.Instance ?? FindFirstObjectByType<CoreGamePOIManager>();
            _manager?.RegisterWrapper(this);
        }

        void OnDisable()
        {
            this.MMEventStopListening<LoadedManagerEvent>();
            this.MMEventStopListening<POIWrapperEvent>();
            _manager?.UnregisterWrapper(this);
        }


        void OnDestroy()
        {
            _manager?.UnregisterWrapper(this);
        }

        #endregion
    }
}
