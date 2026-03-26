using System;
using CompassNavigatorPro;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.ManagerEvents;
using Helpers.Wrappers;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Dirigible.SystemsControl
{
    public class ScanningAntennaBasicPrefab : MonoBehaviour, IRuntimeDirigibleModule
    {
        [SerializeField] CompassPro compass;

        // TODO Something analagous to MineScanTracker for POI scanning
        public DirigibleFrontMountedModule scanningAntennaItem;
        public Animator antennaAnimator;
        public DirigibleEquipment dOwner;

        public GameObject antennaBase;
        [SerializeField] MMFeedbacks equipFeedbacks;
        [SerializeField] MMFeedbacks useFeedbacks;

        [SerializeField] bool includeDisabledPOIs = true;


        // [SerializeField] float examineRange = 25f;
        // [SerializeField] float examineDuration = 1.25f; // seconds; later hook progress UI
        [SerializeField] LayerMask examinableLayerMask = ~0; // or make a dedicated layer
        [SerializeField] bool autoExamine = true;

        UnityEngine.Camera _cam;


        DirigibleFrontMountedModule _currentEquippedModule;

        // private IExaminable _currentTarget;

        void Update()
        {
            if (!autoExamine) return;
            if (_cam == null) return;

            // .... Examine logic here (raycast, etc) ... //
        }

        public void Initialize(DirigibleEquipment owner)
        {
            dOwner = owner;
            if (compass == null) compass = FindFirstObjectByType<CompassPro>(FindObjectsInactive.Include);
            // if (mineTracker == null) mineTracker = FindFirstObjectByType<MineScanTracker>(FindObjectsInactive.Include);
            if (_currentEquippedModule == null)
                _currentEquippedModule = owner.CurrentEquippedModuleSo;

            equipFeedbacks?.PlayFeedbacks();
        }

        public void Use()
        {
            if (compass == null) return;

            useFeedbacks?.PlayFeedbacks();

            // Could emit an event here if needed

            // Swap the profile from the equipped module
            if (_currentEquippedModule != null && _currentEquippedModule.mntScannerProfile != null)
                compass.scanProfile = _currentEquippedModule.mntScannerProfile;

            var scan = compass.Scan(includeDisabledPOIs);
            if (scan == null) return;
            ScannerEvent.Trigger(ScannerEventType.ScanStarted);

            useFeedbacks?.PlayFeedbacks();

            scan.OnScanHit.AddListener((fx, poi, tr) =>
            {
                if (poi == null) Debug.LogWarning("Scan hit with null poi.");
                var go = poi?.gameObject;
                string uniqueId = null;
                if (go != null)
                {
                    var wrapper = go.GetComponent<GamePOIWrapper>();
                    if (wrapper != null) uniqueId = wrapper.UniqueID;
                }


                GamePOIEvent.Trigger(uniqueId, GamePOIEventType.POIWasAreaScanned, null);
            });

            scan.OnScanEnd.AddListener(_ => { ScannerEvent.Trigger(ScannerEventType.ScanEnded); });
        }

        public void Unequip()
        {
        }

        public bool CanInteractWithObject(GameObject colliderGameObject)
        {
            return false;
        }

        public int GetMainActionID()
        {
            throw new NotImplementedException();
        }
    }
}
