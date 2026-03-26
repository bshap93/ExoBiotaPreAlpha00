using System;
using System.Collections;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events;
using Manager;
using Manager.SceneManagers;
using MoreMountains.InventoryEngine;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace LevelConstruct.Interactable.ItemInteractables
{
    public abstract class ActionConsole : MonoBehaviour, IBillboardable, IInteractable, IHoverable, IRequiresUniqueID
    {
        [Serializable]
        public enum ActionConsoleState
        {
            None,
            PoweredOn,
            Broken,
            LacksPower,
            HailingPlayer
        }

        public ConsoleType consoleType;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        public string uniqueID;

        [SerializeField] protected float interactionDistance = 2f;

        [FormerlySerializedAs("state")] [SerializeField]
        protected ActionConsoleState currentConsoleState = ActionConsoleState.None;

        protected SceneObjectData Data;
        // protected HighlightTrigger Trigger;

        protected virtual void Awake()
        {
            // Trigger = GetComponent<HighlightTrigger>();
        }


        protected virtual void OnEnable()
        {
            // if (!Trigger) return;
            // Trigger.OnObjectHighlightStart += OnHoverStart; // return false to cancel hover highlight
            // Trigger.OnObjectHighlightStay += OnHoverStay; // called while highlighted; return false to force unhighlight
            // Trigger.OnObjectHighlightEnd += OnHoverEnd;
        }

        protected virtual void OnDisable()
        {
            // if (!Trigger) return;
            // Trigger.OnObjectHighlightStart -= OnHoverStart;
            // Trigger.OnObjectHighlightStay -= OnHoverStay;
            // Trigger.OnObjectHighlightEnd -= OnHoverEnd;
        }

        public string GetName()
        {
            return consoleType != null ? consoleType.consoleName : "Unknown Console";
        }

        public virtual Sprite GetIcon()
        {
            return consoleType != null ? consoleType.consoleIcon : null;
        }

        public virtual string ShortBlurb()
        {
            return consoleType != null ? consoleType.shortDescription : "An unrecognized console.";
        }

        public virtual Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.usableConsoleIcon;
        }

        public virtual string GetActionText()
        {
            return consoleType != null ? consoleType.actionText : "Use Console";
        }

        public bool OnHoverStart(GameObject go)
        {
            if (!consoleType) return true;

            var recognizable = consoleType.identificationMode == IdentificationMode.RecognizableOnSight;
            var nameToShow = recognizable ? GetName() : consoleType.UnknownName;
            var iconToShow =
                ExaminationManager.Instance?.defaultUnknownIcon;

            var shortToShow = recognizable ? ShortBlurb() : string.Empty;

            Data = new SceneObjectData(
                nameToShow,
                iconToShow,
                shortToShow,
                AssetManager.Instance?.iconRepository.usableConsoleIcon,
                GetActionText()
            );

            Data.Id = consoleType.consoleID;

            BillboardEvent.Trigger(Data, BillboardEventType.Show);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, actionId, additionalInfoText: "to " + GetActionText()
                );

            return true;
        }

        public bool OnHoverStay(GameObject go)
        {
            // ControlsHelpEvent.Trigger(ControlHelpEventType.ShowIfNothingElseShowing, actionId);

            return true;
        }

        public bool OnHoverEnd(GameObject go)
        {
            if (Data == null) Data = SceneObjectData.Empty();
            BillboardEvent.Trigger(Data, BillboardEventType.Hide);
            // BillboardEvent.Trigger(Data, BillboardEventType.Hide);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Hide, actionId);

            return true;
        }

        public abstract void Interact();
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }

        public abstract void OnInteractionStart();
        public void OnInteractionEnd(string param)
        {
        }

        public virtual bool CanInteract()
        {
            if (consoleType == null) return false;

            if (currentConsoleState == ActionConsoleState.Broken ||
                currentConsoleState == ActionConsoleState.LacksPower)
                return false;

            if (consoleType.canInteract)
                return true;

            return false;
        }

        public virtual bool IsInteractable()
        {
            return true;
        }

        public virtual void OnFocus()
        {
        }

        public virtual void OnUnfocus()
        {
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }

        protected virtual IEnumerator InitializeAfterMachineStateManager()
        {
            yield return null;

            if (MachineStateManager.Instance != null)
            {
                var consoleState = MachineStateManager.Instance.GetConsoleStateByID(uniqueID);

                if (consoleState != ActionConsoleState.None)
                    currentConsoleState = consoleState;
            }
            else
            {
                Debug.LogWarning($"[ActionConsole] MachineStateManager not found in scene for console {uniqueID}");
            }
        }

        public abstract void OnInteractionEnd();
#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }
#endif

        protected abstract string GetActionText(bool recognizableOnSight);
        public abstract void SetConsoleToLacksPowerState();
        public abstract void SetConsoleToPoweredOnState();
        public abstract void SetConsoleToHailPlayerState();
    }
}
