using System;
using System.Collections.Generic;
using DG.Tweening;
using Dirigible.Input;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events;
using HighlightPlus;
using Manager;
using MoreMountains.Feedbacks;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.Interactable
{
    public class LootboxInteractable : MonoBehaviour, IInteractable, IBillboardable, IExaminable, IHoverable
    {
        public enum LootBoxType
        {
            WeaponChest,
            ToolChest,
            AbilityInjectorChest,
            AmmoBox
        }

        [Header("Controls Help & Action Info")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        public LootBoxType lootBoxType;

        [SerializeField] GameObject topPiece;
        [SerializeField] MMFeedbacks openFeedbacks;
        [Header("Rotation Settings")] [SerializeField]
        Vector3 openRotation;
        [SerializeField] Vector3 closedRotation;
        [Header("Position Settings")] [SerializeField]
        Vector3 openPosition;
        [SerializeField] Vector3 closedPosition;
        [Header("Interaction Settings")] [SerializeField]
        float interactionDistance = 3.0f;
        [SerializeField] BoxCollider interactionCollider;
        [SerializeField] HighlightEffect highlightEffect;
        [SerializeField] bool disableHighlightOnOpen = true;

        [Header("Settings")] [SerializeField] float openDuration = 1.0f;

        SceneObjectData _sceneObjectData;
        public string GetName()
        {
            switch (lootBoxType)
            {
                case LootBoxType.WeaponChest:
                    return "Weapon Chest";
                case LootBoxType.ToolChest:
                    return "Tool Chest";
                case LootBoxType.AbilityInjectorChest:
                    return "Ability Injector Chest";
                default:
                    return "Loot Chest";
            }
        }
        public Sprite GetIcon()
        {
            return PlayerUIManager.Instance.defaultIconRepository.chestIcon;
        }
        public string ShortBlurb()
        {
            return "N/A";
        }
        public Sprite GetActionIcon()
        {
            // throw new NotImplementedException();

            return PlayerUIManager.Instance.defaultIconRepository.chestIcon;
        }
        public string GetActionText()
        {
            return "Open";
        }
        public void OnFinishExamining()
        {
        }
        public bool ExaminableWithRuntimeTool(IRuntimeTool tool)
        {
            return false;
        }
        public bool OnHoverStart(GameObject go)
        {
            _sceneObjectData = SceneObjectData.Empty();

            _sceneObjectData.ActionIcon = GetActionIcon();
            _sceneObjectData.ActionText = GetActionText();
            _sceneObjectData.Name = GetName();
            _sceneObjectData.ShortBlurb = ShortBlurb();
            _sceneObjectData.Icon = GetIcon();

            BillboardEvent.Trigger(_sceneObjectData, BillboardEventType.Show);

            if (actionId != 0) ControlsHelpEvent.Trigger(ControlHelpEventType.Show, actionId);

            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (_sceneObjectData == null) _sceneObjectData = SceneObjectData.Empty();

            BillboardEvent.Trigger(_sceneObjectData, BillboardEventType.Hide);
            if (actionId != 0) ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);

            return true;
        }

        // Update is called once per frame
        public void Interact()
        {
            openFeedbacks?.PlayFeedbacks();
            if (topPiece != null) topPiece.transform.DOLocalRotate(openRotation, openDuration);
            if (topPiece != null) topPiece.transform.DOLocalMove(openPosition, openDuration);
            interactionCollider.enabled = false;
            if (disableHighlightOnOpen && highlightEffect != null)
                highlightEffect.enabled = false;
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

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif
    }
}
