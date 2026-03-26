using System;
using System.Collections.Generic;
using Dirigible.Input;
using EditorScripts;
using FirstPersonPlayer.Interface;
using FirstPersonPlayer.ScriptableObjects;
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

namespace FirstPersonPlayer.Interactable
{
    public class MetaServiceTerminal : MonoBehaviour, IInteractable, IRequiresUniqueID, IBillboardable, IHoverable
    {
        public string uniqueID;
        [SerializeField] MMFeedbacks startDialogueFeedback;

        [SerializeField] string defaultStartNode = "NavigationServerSwitch";

        [SerializeField] string nodeToUse;

        [SerializeField] MetaTerminalInfoSO metaTerminalInfoSO;

        [ValueDropdown("GetNpcIdOptions")] public
            string npcId;

#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        [SerializeField] [InlineProperty] [HideLabel]
        SpawnInfoEditor overrideSpawnInfo;
        SceneObjectData _sceneObjectData;
        public string GetName()
        {
            return metaTerminalInfoSO.terminalName;
        }
        public Sprite GetIcon()
        {
            return AssetManager.Instance.iconRepository.metaTerminalIcon;
        }
        public string ShortBlurb()
        {
            return metaTerminalInfoSO.shortBlurb;
        }
        public Sprite GetActionIcon()
        {
            return AssetManager.Instance.iconRepository.useTerminalIcon;
        }
        public string GetActionText()
        {
            return "Access Terminal";
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
        public void Interact()
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
            return 5f;
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
