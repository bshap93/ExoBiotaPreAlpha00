using System;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Plugins.HighlightPlus.Runtime.Scripts;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable.Doors
{
    public abstract class InteractableDoor : MonoBehaviour, IInteractable, IBillboardable, IRequiresUniqueID
    {
        public bool isLocked;
        public string keyId;
        [SerializeField] protected string uniqueId;
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        [SerializeField] protected float interactionDistance = 2f;


        protected SceneObjectData data;
        protected HighlightTrigger trigger;
        protected virtual void Awake()
        {
            // if (access == null) access = GetComponent<DoorAccessRequirement>();
            trigger = GetComponent<HighlightTrigger>();
        }
        void OnEnable()
        {
            if (trigger == null) return;
            trigger.OnObjectHighlightStart += OnHoverStart; // return false to cancel hover highlight
            trigger.OnObjectHighlightStay += OnHoverStay; // called while highlighted; return false to force unhighlight
            trigger.OnObjectHighlightEnd += OnHoverEnd;
        }

        void OnDisable()
        {
            if (trigger == null) return;
            trigger.OnObjectHighlightStart -= OnHoverStart;
            trigger.OnObjectHighlightStay -= OnHoverStay;
            trigger.OnObjectHighlightEnd -= OnHoverEnd;
        }
        public abstract string GetName();
        public abstract Sprite GetIcon();
        public abstract string ShortBlurb();
        public abstract Sprite GetActionIcon();
        public abstract string GetActionText();


        public abstract void Interact();
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }


        public virtual void OnInteractionStart()
        {
            // Default implementation can be empty or provide basic interaction start behavior
        }
        public void OnInteractionEnd(string param)
        {
        }

        public bool CanInteract()
        {
            // Default implementation can be empty or provide basic interaction check behavior
            return true;
        }

        public bool IsInteractable()
        {
            // Default implementation can be empty or provide basic interactable check behavior
            return true;
        }


        public virtual void OnFocus()
        {
            // ControlsHelpEvent.Trigger(ControlHelpEventType.Show, actionId);
        }

        public virtual void OnUnfocus()
        {
            // ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }
        public string UniqueID => uniqueId;
        public void SetUniqueID()
        {
            uniqueId = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueId);
        }


        public virtual void OnInteractionEnd()
        {
            // Default implementation can be empty or provide basic interaction end behavior
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif
        protected virtual bool OnHoverStart(GameObject obj)
        {
            var nameToShow = GetName();
            var iconToShow = GetIcon();
            var shortToShow = ShortBlurb();
            var icon = GetActionIcon();

            data = new SceneObjectData(nameToShow, iconToShow, shortToShow, icon, "Use Door");
            data.Id = uniqueId;

            BillboardEvent.Trigger(data, BillboardEventType.Show);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, actionId
                );

            return true; // return false to cancel hover highlight
        }
        protected virtual bool OnHoverStay(GameObject obj)
        {
            return true;
        }
        protected virtual bool OnHoverEnd(GameObject obj)
        {
            if (data == null)
                data = SceneObjectData.Empty();

            BillboardEvent.Trigger(data, BillboardEventType.Hide);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Hide, actionId
                );

            return true;
        }


        protected virtual bool IsLocked()
        {
            if (!isLocked)
                return false;

            AlertEvent.Trigger(AlertReason.DoorLocked, "The door is locked. You need a key to open it.");

            return true;
        }

        protected bool TryOpenWithAccess()
        {
            if (IsLocked())
                // locked feedback here
                return false;

            // access?.MarkOpenedIfPermanent();
            return true;
        }
    }
}
