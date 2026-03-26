using System;
using System.Collections.Generic;
using Dirigible.Input;
using EditorScripts;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Events.Spawn;
using Lightbug.Utilities;
using Manager;
using Manager.DialogueScene;
using MoreMountains.Feedbacks;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace LevelConstruct.Interactable
{
    public enum TerminalType
    {
        Elevator,
        Navigation,
        MetaTerminal,
        LoreTablet
    }

    public abstract class Terminal : MonoBehaviour, IBillboardable, IRequiresUniqueID, IInteractable, IHoverable
    {
        public string uniqueID;

        [ValueDropdown("GetNpcIdOptions")] public
            string npcId;

        public string nodeToUse;

        public string defaultStartNode = "TerminalSwitch";

        public MMFeedbacks startDialogueFeedback;

        [SerializeField] [InlineProperty] [HideLabel]
        public SpawnInfoEditor overrideSpawnInfo;

        public float interactionDistance = 3f;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        public TerminalType terminalType;

        protected SceneObjectData _sceneObjectData;


        public abstract string GetName();
        public virtual Sprite GetIcon()
        {
            switch (terminalType)
            {
                case TerminalType.Elevator:
                    return AssetManager.Instance.iconRepository.elevatorTerminalIcon;
                case TerminalType.Navigation:
                    return AssetManager.Instance.iconRepository.navigationTerminalIcon;
                case TerminalType.MetaTerminal:
                    return AssetManager.Instance.iconRepository.metaTerminalIcon;
                case TerminalType.LoreTablet:
                    return AssetManager.Instance.iconRepository.loreTabletIcon;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public abstract string ShortBlurb();

        public Sprite GetActionIcon()
        {
            switch (terminalType)
            {
                case TerminalType.Elevator:
                case TerminalType.Navigation:
                case TerminalType.MetaTerminal:
                case TerminalType.LoreTablet:
                    return AssetManager.Instance.iconRepository.usableConsoleIcon;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        public virtual string GetActionText()
        {
            return "Access Terminal";
        }
        public virtual bool OnHoverStart(GameObject go)
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
        public virtual bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public virtual bool OnHoverEnd(GameObject go)
        {
            if (_sceneObjectData == null) _sceneObjectData = SceneObjectData.Empty();

            BillboardEvent.Trigger(_sceneObjectData, BillboardEventType.Hide);
            if (actionId != 0) ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);

            return true;
        }
        public virtual void Interact()
        {
            if (!CanInteract()) return;

            if (nodeToUse.IsNullOrWhiteSpace())
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, defaultStartNode);
            else
                FirstPersonDialogueEvent.Trigger(FirstPersonDialogueEventType.StartDialogue, npcId, nodeToUse);

            startDialogueFeedback?.PlayFeedbacks();

            SpawnAssignmentEvent.Trigger(
                SpawnAssignmentEventType.SetMostRecentSpawnPoint, overrideSpawnInfo.SceneName,
                overrideSpawnInfo.SpawnPointId);

            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
        }
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }

        public virtual void OnInteractionStart()
        {
        }
        public virtual void OnInteractionEnd(string param)
        {
        }
        public abstract bool CanInteract();
        public abstract bool IsInteractable();

        public virtual void OnFocus()
        {
        }
        public virtual void OnUnfocus()
        {
        }
        public virtual float GetInteractionDistance()
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

        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif
    }
}
