using System;
using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Events.Triggering;
using Manager;
using Manager.DialogueScene;
using MoreMountains.Tools;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace PhysicsHandlers.Triggers
{
    [DisallowMultipleComponent]
    public class DialogueInitTrigger : MonoBehaviour, IRequiresUniqueID, MMEventListener<TriggerColliderEvent>
    {
        public TriggerType triggerType = TriggerType.OnEnter;
        [ValueDropdown("GetStartNodeOptions")] public string startNode;


        [ValueDropdown("GetNpcIdOptions")] [OnValueChanged("OnNpcIdChanged")]
        public string npcId;
        public string uniqueID;
        public bool startDisabled;

        public string nextDialogueInitTriggerToEnable;

        bool _isDisabled;
        bool _isPlayerInTrigger;

        Player _player;
        TriggerColliderManager _triggerColliderManager;

        void Start()
        {
            _player = ReInput.players.GetPlayer(0);

            _triggerColliderManager = TriggerColliderManager.Instance;

            if (_triggerColliderManager == null)
                Debug.LogWarning(
                    "[DialogueInitTrigger] No TriggerColliderManager found in scene. Ensure one exists.");

            _isDisabled = startDisabled;
        }
        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }

        void OnTriggerEnter(Collider other)
        {
            if (TutorialManager.Instance == null) return;
            if (_isDisabled) return;
            if (_triggerColliderManager)
                if (!_triggerColliderManager.IsDialogueColliderTriggerable(uniqueID))
                    return;

            if (triggerType == TriggerType.OnEnter)
            {
                if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer"))
                    return;

                TriggerDialogueEvents();
            }
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
        public void OnMMEvent(TriggerColliderEvent eventType)
        {
            if (eventType.ColliderType != TriggerColliderType.Dialogue) return;
            if (eventType.ColliderID != uniqueID) return;

            switch (eventType.EventType)
            {
                case TriggerColliderEventType.SetTriggerable:
                    _isDisabled = !eventType.IsTriggerable;
                    break;
            }
        }
        public void TriggerDialogueEvents()
        {
            FirstPersonDialogueEvent.Trigger(
                FirstPersonDialogueEventType.StartDialogue, npcId, startNode);

            TriggerColliderEvent.Trigger(
                uniqueID, TriggerColliderEventType.SetTriggerable, false, TriggerColliderType.Dialogue);

            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);

            TriggerColliderEvent.Trigger(
                nextDialogueInitTriggerToEnable, TriggerColliderEventType.SetTriggerable, true,
                TriggerColliderType.Dialogue);
        }


        void OnNpcIdChanged()
        {
            // Clear startNode if it's not valid for the new NPC
            var validNodes = GetStartNodeOptions();
            if (validNodes.Length > 0)
            {
                var isValid = false;
                foreach (var node in validNodes)
                    if (node == startNode)
                    {
                        isValid = true;
                        break;
                    }

                if (!isValid)
                    startNode = string.Empty;
            }
        }

        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }

        // Instance method that uses the current npcId
        string[] GetStartNodeOptions()
        {
            if (string.IsNullOrEmpty(npcId))
                return new[] { "Select an NPC first" };

            var nodes = DialogueManager.GetNpcStartNodesByNpcId(npcId);
            return nodes != null && nodes.Length > 0
                ? nodes
                : new[] { "No start nodes found" };
        }
    }
}
