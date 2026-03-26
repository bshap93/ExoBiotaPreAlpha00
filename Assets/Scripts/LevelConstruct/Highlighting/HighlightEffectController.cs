using System;
using System.Collections;
using Domains.Gameplay.Equipment.Events;
using FirstPersonPlayer.Tools;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using FirstPersonPlayer.Tools.ToolPrefabScripts;
using Helpers.Events;
using HighlightPlus;
using MoreMountains.Tools;
using Plugins.HighlightPlus.Runtime.Scripts;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using Utilities.Interface;

namespace LevelConstruct.Highlighting
{
    [RequireComponent(typeof(HighlightEffect))]
    [RequireComponent(typeof(HighlightTrigger))]
    [DisallowMultipleComponent]
    public class HighlightEffectController : MonoBehaviour, MMEventListener<EquipmentEvent>,
        MMEventListener<ScannerExaminedVFXEvent>, MMEventListener<MyUIEvent>, IRequiresUniqueID
    {
        [SerializeField] public string targetID;
        [SerializeField] float seeThroughDuration = 5f; // seconds

        public UnityEvent onScanned;

        [SerializeField] Color secondaryStateHighlightColor;
        // [SerializeField] float targetDuration = 3f;
        bool _forcedHighlight;
        HighlightTrigger _highlightTrigger;
        bool _isHighlighted;
        bool _isTargetVisible;

        Color _primaryStateHighlightColor;
        Coroutine seeThroughCoroutine;

        public HighlightEffect HighlightEffect { get; private set; }


        void Awake()
        {
            HighlightEffect = GetComponent<HighlightEffect>();
            _highlightTrigger = GetComponent<HighlightTrigger>();
            _isHighlighted = HighlightEffect.highlighted;
            _isTargetVisible = HighlightEffect.targetFX;
            _primaryStateHighlightColor = HighlightEffect.outlineColor;
        }


        // Called when you ADD this component (or hit “Reset” in the inspector)
        void Reset()
        {
            if (string.IsNullOrEmpty(targetID))
            {
                targetID = Guid.NewGuid().ToString();
#if UNITY_EDITOR
                Undo.RecordObject(this, "Assign targetID");
                EditorUtility.SetDirty(this);
#endif
            }
        }

        void Start()
        {
            if (HighlightEffect == null)
            {
                Debug.LogError("HighlightEffect component not found on this GameObject.");
                return;
            }

            if (_highlightTrigger == null)
                Debug.LogError("HighlightTrigger component not found on this GameObject.");


            ConfigureForTerrainObjects();

            SetSeeThroughMode(SeeThroughMode.Never);

            _highlightTrigger.raycastCamera = GameObject.FindWithTag("MainCamera").GetComponent<Camera>();
        }

        void OnEnable()
        {
            this.MMEventStartListening<EquipmentEvent>();
            this.MMEventStartListening<ScannerExaminedVFXEvent>();
            this.MMEventStartListening<MyUIEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<EquipmentEvent>();
            this.MMEventStopListening<ScannerExaminedVFXEvent>();
            this.MMEventStopListening<MyUIEvent>();
        }
        public string UniqueID => UniqueID;
        public void SetUniqueID()
        {
            if (string.IsNullOrEmpty(targetID))
                targetID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(targetID);
        }


        public void OnMMEvent(EquipmentEvent eventType)
        {
            if (eventType.ToolType == ToolType.Scanner && eventType.EventType == EquipmentEventType.UseEquipment)
            {
                if (HighlightEffect != null) SetSeeThroughMode(SeeThroughMode.AlwaysWhenOccluded);
            }
            else if (eventType.ToolType == ToolType.Pickaxe || eventType.ToolType == ToolType.Shovel)
            {
                if (HighlightEffect != null) SetSeeThroughMode(SeeThroughMode.Never);
            }
        }

        public void OnMMEvent(MyUIEvent eventType)
        {
            // if (eventType.uiActionType == UIActionType.Open)
            // {
            //     // Save current state before wiping
            //     _isHighlighted = _highlightEffect.highlighted;
            //     _isTargetVisible = _highlightEffect.targetFX;
            //
            //     // Now wipe
            //     _highlightEffect.highlighted = false;
            //     _highlightEffect.targetFX = false;
            //     _highlightEffect.targetFXVisibility = Visibility.OnlyWhenOccluded;
            //     _highlightEffect.Refresh();
            // }
            // else if (eventType.uiActionType == UIActionType.Close)
            // {
            //     // Restore last saved state
            //     _highlightEffect.highlighted = _isHighlighted;
            //     _highlightEffect.targetFX = _isTargetVisible;
            //     _highlightEffect.targetFXVisibility = _isTargetVisible
            //         ? Visibility.AlwaysOnTop
            //         : Visibility.OnlyWhenOccluded;
            //
            //     _highlightEffect.Refresh();
            // }
        }

        public void OnMMEvent(ScannerExaminedVFXEvent e)
        {
            // Accept if our IDs match, or (as a fallback) the Transform matches
            var idMatch = !string.IsNullOrEmpty(e.TargetId) && e.TargetId == targetID;
            var trMatch = e.Target != null && e.Target == transform;

            if (!idMatch && !trMatch) return;

            // Optional hook for your existing UnityEvent
            onScanned?.Invoke();

            // Kick local highlight “hit” and enable the target FX glow
            TriggerHighlightHitEffect(); // calls _highlightEffect.HitFX();
            ActivateTarget(); // turns on targetFX (you can also auto-stop after a duration)

            // Optional: brief “see-through while scanned” window using your existing timer
            if (e.Duration > 0f) SetSeeThroughMode(SeeThroughMode.AlwaysWhenOccluded);
            // The controller already has a coroutine to reset see-through after a delay; reuse it:
            // We’ll piggyback by setting seeThroughDuration temporarily if you like:
            // (Or just leave your existing duration logic as-is.)
        }

        public void SetSecondaryStateHighlightColor()
        {
            if (HighlightEffect != null)
            {
                HighlightEffect.outlineColor = secondaryStateHighlightColor;
                HighlightEffect.Refresh();
            }
        }


        // Simplified to not need a parameter since we already checked the targetID
        public void ActivateTarget()
        {
            if (HighlightEffect != null) HighlightEffect.targetFX = true;
            _isTargetVisible = true;
            // After X seconds, set targetFX to false
        }


        public void SetSeeThroughMode(SeeThroughMode mode)
        {
            if (mode == SeeThroughMode.Never)
                if (HighlightEffect != null)
                {
                    HighlightEffect.seeThrough = SeeThroughMode.Never;
                    HighlightEffect.Refresh();
                }

            var playerEquipment = PlayerEquipment.GetWithToolType(typeof(HandheldScannerToolPrefab))
                                  ?? PlayerEquipment.GetWithActiveToolOrRight();

            if (playerEquipment == null) return;


            var distance = GetDistanceFromPlayer();
            var currentlyEquippedItem = playerEquipment.GetCurrentlyEquippedItem();
            float range;
            if (currentlyEquippedItem is RightHandEquippableTool scannerItem)
                range = scannerItem.scannerProfile.maxRange;
            else
                return;

            // Set at 5 for now
            if (distance < range)
                if (HighlightEffect != null)
                {
                    onScanned?.Invoke();
                    if (HighlightEffect.seeThrough != mode)
                    {
                        HighlightEffect.seeThrough = mode;
                        HighlightEffect.Refresh();

                        // Restart timer if duration > 0
                        if (mode != SeeThroughMode.Never && seeThroughDuration > 0f)
                        {
                            if (seeThroughCoroutine != null)
                                StopCoroutine(seeThroughCoroutine);

                            seeThroughCoroutine = StartCoroutine(ResetSeeThroughAfterDelay());
                        }
                    }
                }
            // var normalizedDist = distance / PlayerEquipment.Instance.scannerMaxRange;
        }


        IEnumerator ResetSeeThroughAfterDelay()
        {
            yield return new WaitForSeconds(seeThroughDuration);
            if (HighlightEffect != null)
            {
                HighlightEffect.seeThrough = SeeThroughMode.Never;
                HighlightEffect.Refresh();
            }

            seeThroughCoroutine = null;
        }

        public float GetDistanceFromPlayer()
        {
            if (HighlightEffect != null)
                if (Camera.main != null)
                    return Vector3.Distance(HighlightEffect.transform.position, Camera.main.transform.position);

            return 0f;
        }

        public void TriggerHighlightHitEffect()
        {
            HighlightEffect.HitFX();
        }

        public void ConfigureForTerrainObjects()
        {
            if (HighlightEffect != null)
            {
                // Enable ordered see-through for better terrain handling
                HighlightEffect.seeThroughOrdered = true;

                // Set accurate rendering for terrains
                HighlightEffect.seeThroughOccluderMaskAccurate = true;

                // Assign terrain to a specific layer if not already done
                if (GetComponent<Terrain>() != null)
                    gameObject.layer = LayerMask.NameToLayer("Terrain"); // Create this layer in your project
            }
        }


        public void SetHighlighted(bool state)
        {
            HighlightEffect.highlighted = state;
            _isHighlighted = state;

            HighlightEffect.Refresh();
        }

        public void SetTargetVisible(bool state)
        {
            HighlightEffect.targetFX = state;
            HighlightEffect.targetFXVisibility = state ? Visibility.AlwaysOnTop : Visibility.OnlyWhenOccluded;
            _isTargetVisible = state;
            HighlightEffect.Refresh();
        }
        public void SetPrimaryStateHighlightColor()
        {
            if (HighlightEffect != null)
            {
                HighlightEffect.outlineColor = _primaryStateHighlightColor;
                HighlightEffect.Refresh();
            }
        }
    }
}
