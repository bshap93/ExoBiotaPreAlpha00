using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FirstPersonPlayer.Interactable.BioOrganism;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.AnimancerHelper;
using Helpers.Events;
using Helpers.Events.Inventory;
using Helpers.Events.ManagerEvents;
using Helpers.Events.UI;
using Helpers.Wrappers;
using LevelConstruct.Highlighting;
using Manager.SceneManagers;
using MoreMountains.Feedbacks;
using Plugins.HighlightPlus.Runtime.Scripts;
using UnityEngine;

// LiquidSampleTool.cs

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    [RequireComponent(typeof(ToolObjectController))]
    public class LiquidSampleTool : MonoBehaviour, IRuntimeTool, IToolAnimationControl
    {
        [SerializeField] LiquidSampleToolObject liquidSampleToolObject;
        [SerializeField] Sprite defaultReticleForTool;
        [SerializeField] Sprite reticleForInabilityToSample;

        [SerializeField] string[] tagsWhichShouldShowInabilityReticle;

        [SerializeField] float sightRange = 10f;
        [SerializeField] LayerMask organismLayerMask;
        [SerializeField] bool showHighlights = true;

        [SerializeField] bool toolIsUsedOnRelease;


        [SerializeField] CanBeAreaScannedType detectableType = CanBeAreaScannedType.BasicBioScanner;


        [Header("Visuals (optional)")] [SerializeField]
        GameObject mainGunObject;

        [SerializeField] MMFeedbacks equippedFeedbacks;
        [SerializeField] MMFeedbacks unequippedFeedbacks;


        [Header("Feedbacks")] [SerializeField] MMFeedbacks startSamplingFeedbacks;

        [SerializeField] MMFeedbacks completeSamplingFeedbacks;
        [SerializeField] MMFeedbacks samplerPenetrationFeedbacks;

        [SerializeField] GameObject liquidCartridgeObject;

        [Header("Sampling")] [SerializeField] float sampleDuration = 1.25f; // seconds to complete a sample

        [SerializeField] float contactRange = 2.0f; // meters
        [SerializeField] LayerMask hitMask = ~0; // filter if desired
        [SerializeField] MMFeedbacks injectStartFeedbacks;

        readonly HashSet<GamePOIWrapper> _highlightedPOIs = new();

        AnimancerArmController _animController;


        Camera _cam;

        float _checkObjectsCooldown;
        BioOrganismSampleNode _currentTarget;
        float _timer;
        bool _useHeldThisFrame;

        void Start()
        {
            Equip();
        }

        void Update()
        {
            if (showHighlights)
            {
                _checkObjectsCooldown -= Time.deltaTime;
                if (_checkObjectsCooldown <= 0f)
                {
                    if (showHighlights) UpdateVisibleOrganisms();
                    _checkObjectsCooldown = 0.25f;
                }

                UpdateVisibleOrganisms();
            }

            // If Use wasn’t pressed this frame, we’re not sampling
            if (!_useHeldThisFrame)
            {
                AbortSampling();
                return;
            }

            if (_cam == null) return;

            // Raycast from center to find a BioOrganismNode
            var ray = new Ray(_cam.transform.position, _cam.transform.forward);
            if (!Physics.Raycast(ray, out var hit, contactRange, hitMask, QueryTriggerInteraction.Ignore))
            {
                AbortSampling();
                return;
            }

            var node = hit.collider.GetComponentInParent<BioOrganismSampleNode>();
            if (node == null)
            {
                AbortSampling();
                return;
            }

            var sampleUniqueId = Guid.NewGuid().ToString();


            if (!ReferenceEquals(node, _currentTarget))
            {
                if (!node.CanBeSampledViaManager())
                {
                    AbortSampling();
                    AlertEvent.Trigger(
                        AlertReason.SampleLimitExceeded,
                        "Sample Limit Exceeded for this BioOrganism",
                        "Sampling Failed");

                    return;
                }

                _currentTarget = node;
                _timer = 0f;
                StartCoroutine(
                    DelayedSamplingCompleteFeedbacks(
                        _animController.currentToolAnimationSet.beginUseAnimation.length));

                BioSampleEvent.Trigger(
                    sampleUniqueId, BioSampleEventType.StartCollection, node.bioOrganismType,
                    sampleDuration);
            }

            _timer += Time.deltaTime;
            var pct = Mathf.Clamp01(_timer / sampleDuration);

            // Complete
            if (_timer >= sampleDuration)
            {
                if (!node.CanBeSampledViaManager())
                {
                    AbortSampling();
                    AlertEvent.Trigger(
                        AlertReason.SampleLimitExceeded,
                        "Sample Limit Exceeded for this BioOrganism",
                        "Sampling Failed");

                    return;
                }
                // Consume one allowance *first*; if it fails, abort and don’t fire Complete

                if (!node.TryTakeSample(sampleUniqueId))
                {
                    AbortSampling();
                    return;
                }

                startSamplingFeedbacks?.StopFeedbacks();
                completeSamplingFeedbacks?.PlayFeedbacks();

                // if (!node.CanBeSampledViaManager())
                _timer = 0f;
            }

            // Reset held flag for the next frame; Use() will set it if still held
            _useHeldThisFrame = false;
        }


        void OnEnable()
        {
            if (_cam == null && Camera.main != null) _cam = Camera.main;
            ResetProgress();

            // Disable hover highlight only on organisms detectable by this tool
            var mgr = CoreGamePOIManager.Instance ?? FindFirstObjectByType<CoreGamePOIManager>();
            if (mgr != null)
                foreach (GamePOIWrapper wrapper in mgr.GetWrappersOfType(CanBeAreaScannedType.BasicBioScanner))
                {
                    var trigger = wrapper.GetComponent<HighlightTrigger>();
                    if (trigger != null) trigger.highlightOnHover = false;
                }
        }

        public void Initialize(PlayerEquipment owner)
        {
            if (liquidSampleToolObject == null) liquidSampleToolObject = owner.CurrentToolSo as LiquidSampleToolObject;
            _animController = owner.animancerPrimaryArmsController;
        }


        public void Use()
        {
            // Called every frame while the Use input is held (see PlayerEquipment.Update). 
            // We just mark the intent; Update() drives the hold logic.
            _useHeldThisFrame = true;
        }

        public void Unequip()
        {
            AbortSampling();

            // Restore hover highlight for organisms
            var mgr = CoreGamePOIManager.Instance ?? FindFirstObjectByType<CoreGamePOIManager>();
            if (mgr != null)
                foreach (GamePOIWrapper wrapper in mgr.GetWrappersOfType(CanBeAreaScannedType.BasicBioScanner))
                {
                    var trigger = wrapper.GetComponent<HighlightTrigger>();
                    if (trigger != null) trigger.highlightOnHover = true;
                }

            // Clear any forced highlights
            foreach (var poi in _highlightedPOIs)
            {
                var controller = poi.GetComponentInParent<HighlightEffectController>();
                if (controller != null)
                {
                    controller.SetHighlighted(false);
                    controller.SetTargetVisible(false);
                }
            }

            _highlightedPOIs.Clear();
            LiquidToolStateEvent.Trigger(LiquidToolStateEventType.UnequippedLiquidTool);
        }
        public void Equip()
        {
            LiquidToolStateEvent.Trigger(LiquidToolStateEventType.EquippedLiquidTool);
        }


        public bool CanInteractWithObject(GameObject colliderGameObject)
        {
            if (colliderGameObject.CompareTag("LiquidFilledBiostructure"))
                return true;

            return false;
        }

        public Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            // Check if the object has a tag that should show inability reticle
            if (tagsWhichShouldShowInabilityReticle != null)
                foreach (var tagName in tagsWhichShouldShowInabilityReticle)
                    if (colliderGameObject.CompareTag(tagName))
                        return reticleForInabilityToSample;

            // Default to the normal reticle
            return defaultReticleForTool;
        }
        public bool ToolIsUsedOnRelease()
        {
            return toolIsUsedOnRelease;
        }
        public bool ToolMustBeHeldToUse()
        {
            return true;
        }

        public bool CanAbortAction()
        {
            return true;
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

        // IToolAnimationControl implementation
        public void OnEquipped()
        {
            // Tool decides what happens on equip
            // Could play pull-out animation here if desired
            if (_animController != null)
            {
                // Just start with idle - no special equip animation
            }
        }
        public void OnUseStarted()
        {
            // Play begin -> during sequence when starting to sample
            if (_animController != null && _animController.currentToolAnimationSet != null &&
                _animController.currentToolAnimationSet.beginUseAnimation != null)
                _animController.PlayToolUseSequence();
        }


        public void OnUseStopped()
        {
            // Play end animation when stopping
            if (_animController != null && _animController.currentToolAnimationSet != null &&
                _animController.currentToolAnimationSet.endUseAnimation != null)
                _animController.EndToolUse();
        }
        public SecondaryActionType GetSecondaryActionType()
        {
            return SecondaryActionType.None;
        }

        // Wait then play penetration complete feedbacks
        IEnumerator DelayedSamplingCompleteFeedbacks(float delay)
        {
            yield return new WaitForSeconds(delay);
            samplerPenetrationFeedbacks?.PlayFeedbacks();
            startSamplingFeedbacks?.PlayFeedbacks();
            if (_currentTarget != null) _currentTarget.MakeJiggle();
        }

        void UpdateVisibleOrganisms()
        {
            if (_cam == null) return;

            var hits = Physics.OverlapSphere(_cam.transform.position, sightRange, organismLayerMask);

            var seenThisFrame = new HashSet<GamePOIWrapper>();

            foreach (var col in hits)
            {
                var wrapper = col.GetComponentInParent<GamePOIWrapper>();
                if (wrapper == null) continue;

                if (!wrapper.HasScannerCapability(GetDetectableType())) continue;
                // ^ implement HasScannerCapability to check type, or add tag/enum

                seenThisFrame.Add(wrapper);

                if (!_highlightedPOIs.Contains(wrapper))
                {
                    // Newly seen → fire highlight
                    HighlightPOI(wrapper, true);
                    _highlightedPOIs.Add(wrapper);
                }
            }

            // Remove highlights for those no longer seen
            foreach (var prev in _highlightedPOIs.Except(seenThisFrame).ToList())
            {
                HighlightPOI(prev, false);
                _highlightedPOIs.Remove(prev);
            }
        }

        void HighlightPOI(GamePOIWrapper wrapper, bool state)
        {
            if (wrapper == null) return;
            var controller = wrapper.GetComponentInParent<HighlightEffectController>();
            if (controller != null)
            {
                controller.SetHighlighted(state);
                controller.SetTargetVisible(state);
            }


            // Optional: also fire a GamePOIEvent so UI or Compass can respond
            if (state)
                GamePOIEvent.Trigger(wrapper.UniqueID, GamePOIEventType.POIWasAreaScanned, null);
        }

        void AbortSampling()
        {
            if (CanAbortAction())
            {
                _currentTarget = null;
                _timer = 0f;
                _useHeldThisFrame = false;
                BioSampleEvent.Trigger("Any", BioSampleEventType.Abort, null, 0f);
                startSamplingFeedbacks?.StopFeedbacks();
            }
        }

        void ResetProgress()
        {
            _timer = 0f;
        }

        public void InjectAvailableIchor()
        {
            // Animancer Arm should animate

            injectStartFeedbacks?.PlayFeedbacks();


            SecondaryActionEvent.Trigger(SecondaryActionType.InjectAvailableIchor);
        }
    }
}
