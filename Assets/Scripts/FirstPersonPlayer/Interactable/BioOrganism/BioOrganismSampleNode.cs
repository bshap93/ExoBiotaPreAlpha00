using System;
using System.Linq;
using Animancer;
using Feedbacks.Interface;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events;
using Inventory;
using LevelConstruct.Highlighting;
using Manager;
using Manager.SceneManagers;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using NewScript;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace FirstPersonPlayer.Interactable.BioOrganism
{
    public class BioOrganismSampleNode : BioOrganismBase, IInteractable, IFleshyObject,
        MMEventListener<LoadedManagerEvent>
    {
#if ODIN_INSPECTOR && UNITY_EDITOR
        [InlineButton(nameof(SyncSampleFromType), "Sync Sample From Type")]
        [OnValueChanged(nameof(AutoSyncSampleFromType), true)]
#endif
        [Header("Sampling")]
        public BioOrganismSample sampleItem;

        public UnityEvent OnDepleted;

        public float baseBlowbackContaminationAmt = 0.5f;

        public int timesAllowedToSample = 1;

        [SerializeField] float interactionDistance = 2f;

        // [SerializeField] GameObject[] disableWhenDepleted;

        [SerializeField] string actionTextIfNotToolEquipped = "Equip";
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionIdIfNotToolEquipped;

        [SerializeField] HighlightEffectController highlightEffectController;
        [SerializeField] MMFeedbacks jiggleFeedbacks;

        ObjectiveHelper _objectiveHelper;


        string SceneKey => gameObject.scene.name;

        // Advertise sampling capability to the manager:
        public override bool SupportsSampling => true;
        public override int DefaultSamplingAllowance => Mathf.Max(0, timesAllowedToSample);
        void Start()
        {
            _objectiveHelper = GetComponent<ObjectiveHelper>();

            // StartCoroutine(DelayedRefreshDepletionState());
        }


        protected override void OnEnable()
        {
            base.OnEnable();
            this.MMEventStartListening();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            this.MMEventStopListening();
        }
        public void MakeJiggle()
        {
            jiggleFeedbacks?.PlayFeedbacks();
        }
        public float BaseBlowbackContaminationAmt => baseBlowbackContaminationAmt;
        public void Interact()
        {
            var isToolEquipped = IsLiquidScannerEquipped();
            var isToolInInventory = IsLiquidScannerInInventory();
            if (!isToolEquipped && isToolInInventory)
                EquipLiquidScanner();
            else if (!IsLiquidScannerEquipped())
                AlertEvent.Trigger(
                    AlertReason.LackToolForInteraction, "You aren't carrying a liquid sampling tool.",
                    "No Sampling Tool");
        }
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }
        public bool CanInteract()
        {
            return true;
        }
        public bool IsInteractable()
        {
            return true;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All)
                RefreshDepletionState();
        }

        // IEnumerator DelayedRefreshDepletionState()
        // {
        //     // Wait one frame to ensure manager has initialized and loaded
        //     yield return null;
        //
        //     RefreshDepletionState();
        // }

        void RefreshDepletionState()
        {
            var hasLeft = CanBeSampledViaManager();
            // if (disableWhenDepleted != null)
            //     foreach (var go in disableWhenDepleted)
            //         if (go)
            //             go.SetActive(hasLeft);

            if (!hasLeft)
            {
                Debug.Log("Depleted showing yellow highlight");
                highlightEffectController.SetSecondaryStateHighlightColor();
            }
            // or: enable/disable collider, prompts, etc.
        }

        public bool TryTakeSample(string sampleUniqueId)
        {
            // consume first to guard races
            if (!ConsumeOneViaManager())
                return false;

            // fire the completion event (BioSamplesManager will persist it)
            BioSampleEvent.Trigger(sampleUniqueId, BioSampleEventType.CompleteCollection, bioOrganismType, 0f);

            if (_objectiveHelper)
                _objectiveHelper.ProgressObjectiveByN(1);

            // if you have a depletion UI toggle, do it here:
            RefreshDepletionState();

            return true;
        }

        bool IsLiquidScannerEquipped()
        {
            var equippedSlot = GlobalInventoryManager.Instance.equipmentInventory.Content.First();
            if (equippedSlot == null) return false;
            return equippedSlot.ItemID == "BasicLiquidTool";
        }

        bool IsLiquidScannerInInventory()
        {
            var inventory = GlobalInventoryManager.Instance.playerInventory;
            if (inventory == null) return false;
            return inventory.Content.Any(slot => slot != null && slot.ItemID == "BasicLiquidTool");
        }

        void EquipLiquidScanner()
        {
            var inventory = GlobalInventoryManager.Instance.playerInventory;
            if (inventory == null) return;
            var scannerItem = inventory.Content.FirstOrDefault(s => s != null && s.ItemID == "BasicLiquidTool");
            var sourceIndex = Array.IndexOf(inventory.Content, scannerItem);
            if (scannerItem == null) return;

            // scannerItem.Equip("Player1");
            MMInventoryEvent.Trigger(
                MMInventoryEventType.EquipRequest, null, scannerItem.TargetInventoryName, scannerItem, 1, sourceIndex,
                "Player1");
        }

        public override bool OnHoverStart(GameObject go)
        {
            if (!bioOrganismType) return true;

            var recognizable = bioOrganismType.identificationMode == IdentificationMode.RecognizableOnSight;

            var showKnown = recognizable; // later: OR with analysis progression
            var nameToShow = showKnown ? bioOrganismType.organismName : bioOrganismType.UnknownName;
            var iconToShow = showKnown
                ? bioOrganismType.organismIcon
                : bioOrganismType.organismIcon ?? ExaminationManager.Instance?.defaultUnknownIcon;

            var shortToShow = showKnown ? bioOrganismType.shortDescription : bioOrganismType.UnknownDescription;


            string actionTextToUse;
            int actionIdToUse;
            if (IsLiquidScannerEquipped())
            {
                actionTextToUse = actionText;
                actionIdToUse = actionId;
            }
            else
            {
                actionTextToUse = actionTextIfNotToolEquipped;
                actionIdToUse = actionIdIfNotToolEquipped;
            }


            data = new SceneObjectData(
                nameToShow,
                iconToShow,
                shortToShow,
                ExaminationManager.Instance?.iconRepository.bioOrganismIcon,
                GetActionText(recognizable)
            );

            data.Id = bioOrganismType.organismID;

            BillboardEvent.Trigger(data, BillboardEventType.Show);
            if (actionId != 0)
                if (ExaminationManager.Instance != null)
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Show, actionIdToUse,
                        string.IsNullOrEmpty(actionTextToUse) ? null : actionTextToUse,
                        ExaminationManager.Instance.iconRepository.liquidSampleIcon);

            return true;
        }
        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Sample Biomass";
        }

        public bool CanBeSampledViaManager()
        {
            var timesLeft = BioOrganismManager.Instance.GetTimesLeft(SceneKey, UniqueID, DefaultSamplingAllowance);
            return timesLeft > 0;
        }

        bool ConsumeOneViaManager()
        {
            BioOrganismManager.Instance.ConsumeOne(SceneKey, UniqueID);
            // if depleted
            if (BioOrganismManager.Instance.IsDepleted(SceneKey, UniqueID))
            {
                OnDepleted.Invoke();
                highlightEffectController.SetSecondaryStateHighlightColor();
            }

            return true;
        }


        void AutoSyncSampleFromType()
        {
            if (!bioOrganismType) return;
            if (sampleItem == null)
            {
                sampleItem = new BioOrganismSample();
                sampleItem.uniqueID = Guid.NewGuid().ToString();
            }

            sampleItem.parentOrgamism = bioOrganismType;
            sampleItem.parentOrganismID = bioOrganismType.organismID;

            sampleItem.associatedBioLogFile = null; // set by manager at runtime
#if UNITY_EDITOR
            EditorApplication.delayCall += () =>
            {
                if (this) EditorUtility.SetDirty(this);
            };
#endif
        }

#if UNITY_EDITOR
        void SyncSampleFromType()
        {
            AutoSyncSampleFromType();
        }
#endif
    }
}
