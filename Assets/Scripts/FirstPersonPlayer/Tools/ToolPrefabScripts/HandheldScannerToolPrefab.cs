using System;
using System.Collections;
using CompassNavigatorPro;
using Domains.Gameplay.Equipment.Events;
using FirstPersonPlayer.Minable;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.ManagerEvents;
using Helpers.Wrappers;
using HighlightPlus;
using LevelConstruct.Highlighting;
using Manager;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI.Interface;
using UnityEngine;
using UnityEngine.Serialization;
using ItemPicker = LevelConstruct.Interactable.ItemInteractables.ItemPicker.ItemPicker;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class HandheldScannerToolPrefab : MonoBehaviour, IRuntimeTool


    {
        [SerializeField] Sprite defaultReticleForTool;

        [SerializeField] CompassPro compass;
        [SerializeField] CanBeAreaScannedType detectableType = CanBeAreaScannedType.BasicScanner;

        // [SerializeField] private MineScanTracker mineTracker;

        [SerializeField] MMFeedbacks useFeedbacks;
        [SerializeField] MMFeedbacks finishedExamination;
        [SerializeField] MMFeedbacks duringExaminationFB;

        [SerializeField] bool includeDisabledPOIs = true;
        [SerializeField] bool toolIsUsedOnRelease;

        [SerializeField] MMFeedbacks equippedFeedbacks;
        [SerializeField] MMFeedbacks unequippedFeedbacks;

        [FormerlySerializedAs("ScannerItemObject")] [SerializeField]
        RightHandEquippableTool rightHandEquippableTool;

// Optional: mild highlight nudge while targeting
        [SerializeField] HighlightEffect softAimHighlight; // assign if you want a little glow


        // === Examining (aim + hold) ===
        [SerializeField] float examineRange = 4f;
        [SerializeField] float examineDuration = 1.25f; // seconds; later hook progress UI
        [SerializeField] LayerMask examinableLayerMask = ~0; // or make a dedicated layer
        [SerializeField] bool autoExamineWhileAiming = true;
        Camera _cam;
        IExaminable _currentTarget;

        Coroutine _examineRoutine;
        RightHandEquippableTool _rightHandEquippableTool;

        void Update()
        {
            if (!autoExamineWhileAiming) return;
            if (_cam == null) return;

            // Raycast from camera center
            var ray = new Ray(_cam.transform.position, _cam.transform.forward);
            if (!Physics.Raycast(ray, out var hit, examineRange, examinableLayerMask, QueryTriggerInteraction.Ignore))
            {
                AbortExamine();
                return;
            }

            var examinable = hit.collider.GetComponentInParent<IExaminable>();
            if (examinable == null)
            {
                AbortExamine();
                return;
            }

            var picker = hit.collider.GetComponentInParent<ItemPicker>();
            if (picker != null)
                if (!CanScannerExamineItem(picker.inventoryItem))
                {
                    AbortExamine(); // skip starting/continuing examine for this target
                    return;
                }

            var node = (examinable as Component)?.GetComponentInParent<MyOreNode>();

            if (node != null && node.itemTypeMined != null)
                if (ExaminationManager.Instance != null &&
                    ExaminationManager.Instance.HasOreBeenExamined(node.itemTypeMined.ItemID))
                {
                    AbortExamine();
                    return; // already known → do nothing
                }


            // If target changed, restart coroutine
            if (!ReferenceEquals(examinable, _currentTarget))
            {
                _currentTarget = examinable;
                if (_examineRoutine != null) StopCoroutine(_examineRoutine);
                _examineRoutine = StartCoroutine(ExamineAfterHold(_currentTarget));
            }
        }


        void OnEnable()
        {
            // If this tool is equipped, start lightweight polling
            if (_cam == null && Camera.main != null) _cam = Camera.main;
        }


        public void Initialize(PlayerEquipment owner)
        {
            if (compass == null) compass = FindFirstObjectByType<CompassPro>(FindObjectsInactive.Include);
            // if (mineTracker == null) mineTracker = FindFirstObjectByType<MineScanTracker>(FindObjectsInactive.Include);
            if (_rightHandEquippableTool == null)
                _rightHandEquippableTool = owner.CurrentToolSo as RightHandEquippableTool;
        }

        public void Use()
        {
            if (compass == null) return;

            useFeedbacks?.PlayFeedbacks();


            EquipmentEvent.Trigger(EquipmentEventType.UseEquipment, ToolType.Scanner);


            // Swap to the profile from the equipped item
            if (_rightHandEquippableTool != null && _rightHandEquippableTool.scannerProfile != null)
                compass.scanProfile = _rightHandEquippableTool.scannerProfile;


            var scan = compass.Scan(includeDisabledPOIs);
            if (scan == null) return;
            ScannerEvent.Trigger(ScannerEventType.ScanStarted);

            if (scan != null)
                scan.OnScanHit.AddListener((fx, poi, tr) =>
                {
                    if (poi == null) Debug.LogWarning("Scan hit with null poi ");
                    var go = poi?.gameObject;
                    string uniqueId = null;
                    if (go != null)
                    {
                        var wrapper = go.GetComponent<GamePOIWrapper>();
                        if (wrapper != null) uniqueId = wrapper.UniqueID;
                    }


                    GamePOIEvent.Trigger(uniqueId, GamePOIEventType.POIWasAreaScanned, null);
                });

            scan.OnScanEnd.AddListener(_ =>
            {
                ScannerEvent.Trigger(ScannerEventType.ScanEnded);
                Debug.Log("[GamePOIManager] Scan end triggered.");
            });
        }

        public void Unequip()
        {
            AbortExamine();
            _cam = null;
        }
        public void Equip()
        {
            
        }

        public bool CanInteractWithObject(GameObject colliderGameObject)
        {
            var examinable = colliderGameObject.GetComponent<IExaminable>();
            if (examinable == null) return false;

            if (examinable.ExaminableWithRuntimeTool(this)) return true;

            return false;
        }

        public Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return defaultReticleForTool;
        }
        public bool ToolIsUsedOnRelease()
        {
            return toolIsUsedOnRelease;
        }
        public bool ToolMustBeHeldToUse()
        {
            return false;
        }

        public bool CanAbortAction()
        {
            return false;
        }
        public SecondaryActionType GetSecondaryActionType()
        {
            return SecondaryActionType.None;
        }

        public MMFeedbacks GetEquipFeedbacks()
        {
            return equippedFeedbacks;
        }

        public CanBeAreaScannedType GetDetectableType()
        {
            return detectableType;
        }
        public MMFeedbacks GetUnequipFeedbacks()
        {
            return unequippedFeedbacks;
        }
        public void ChargeUse(bool justPressed)
        {
            throw new NotImplementedException();
        }

        bool CanScannerExamineItem(InventoryItem item)
        {
            if (item == null) return false;

            // Already known? Don’t examine again.
            if (ExaminationManager.Instance != null &&
                ExaminationManager.Instance.HasItemPickerBeenExamined(item.ItemID))
                return false;

            var mode = item
                .identificationMode; // RecognizableOnSight, NeedsScanOnce, NeedsExaminationOnce, NeedsBiologicalAnalysis...

            switch (mode)
            {
                case IdentificationMode.NeedsScanOnce:
                case IdentificationMode.NeedsExaminationOnce:
                    return true;

                case IdentificationMode.NeedsBiologicalAnalysisOnce:
                    return false;

                case IdentificationMode.RecognizableOnSight:
                default:
                    return false;
            }
        }


        void FireScanVFXFor(IExaminable target, float vfxDuration = 1.5f)
        {
            var comp = target as Component;
            if (comp == null) return;

            var controller = comp.GetComponentInParent<HighlightEffectController>();
            if (controller == null) return;

            // Tell everyone (notably the target’s HighlightEffectController)
            MMEventManager.TriggerEvent(
                new ScannerExaminedVFXEvent
                {
                    TargetId = controller.targetID,
                    Target = controller.transform,
                    Duration = vfxDuration
                });
        }

        IEnumerator ExamineAfterHold(IExaminable target)
        {
            // NEW: re-check before timing
            var node = (target as Component)?.GetComponentInParent<MyOreNode>();
            if (node != null && node.itemTypeMined != null &&
                ExaminationManager.Instance != null &&
                ExaminationManager.Instance.HasOreBeenExamined(node.itemTypeMined.ItemID))
                yield break; // known now → stop immediately

            ScannerEvent.Trigger(ScannerEventType.ExaminationStart, examineDuration);
            var t = 0f;
            duringExaminationFB?.PlayFeedbacks();
            while (t < examineDuration)
            {
                if (_cam == null || target == null) yield break;

                var dist = Vector3.Distance(_cam.transform.position, (target as Component).transform.position);
                if (dist > examineRange) yield break;

                t += Time.deltaTime;
                yield return null;
            }

            Debug.Log($"[Scanner] Examined {(target as Component)?.name ?? "unknown"}");

            target.OnFinishExamining(); // triggers ExaminationEvent → ExaminationManager
            FireScanVFXFor(target); // plays highlight VFX
            ScannerEvent.Trigger(ScannerEventType.ExaminationEnd);
            duringExaminationFB?.StopFeedbacks();
            finishedExamination?.PlayFeedbacks();

            _examineRoutine = null;
        }


        void AbortExamine()
        {
            if (_examineRoutine != null)
            {
                StopCoroutine(_examineRoutine);
                duringExaminationFB?.StopFeedbacks();
                ScannerEvent.Trigger(ScannerEventType.ExaminationEnd);

                _examineRoutine = null;
            }

            _currentTarget = null;
        }
    }
}
