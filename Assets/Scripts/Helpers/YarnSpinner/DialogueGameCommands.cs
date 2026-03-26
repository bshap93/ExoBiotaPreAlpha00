using System;
using System.Collections.Generic;
using Events;
using Helpers.Events;
using Helpers.StaticHelpers;
using Manager.DialogueScene;
using Overview.NPC;
using SharedUI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Yarn.Unity;

namespace Helpers.YarnSpinner
{
    public class DialogueGameCommands : MonoBehaviour
    {
        [Tooltip("If not assigned, will try DialogueManager.Instance.dialogueRunner")]
        public DialogueRunner dialogueRunner;


        public VariableStorageBehaviour variableStorage;
#if UNITY_EDITOR
        [FormerlySerializedAs("DefaultActionId")] [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int defaultActionId = -1;

        // Dialogue Gesture Commands
        // Character Avatar animations -----------
        public GameObject characterNPCRoot;


        void Awake()
        {
            if (dialogueRunner == null && DialogueManager.Instance != null)
                dialogueRunner = DialogueManager.Instance.dialogueRunner;


            if (variableStorage == null && DialogueManager.Instance != null)
                variableStorage = DialogueManager.Instance.variableStorage;

            dialogueRunner.AddCommandHandler("test_command", TestCommand);
        }

        // ---------- Inventory  commands ----------

// DialogueGameCommands.cs
        // [YarnCommand("give_player_item")]
        // public void GivePlayerItem(string itemId, int amount = 1)
        // {
        //     Debug.Log($"[Yarn] give_player_item on {name} (instanceID={GetInstanceID()}) x{amount}");
        //
        //     var inv = GlobalInventoryManager.Instance;
        //     if (inv == null)
        //     {
        //         Debug.LogWarning("GlobalInventoryManager not found, cannot give item.");
        //         return;
        //     }
        //
        //     var item = inv.CreateItem(itemId); // SINGLE unit item
        //     if (item == null)
        //     {
        //         Debug.LogWarning($"Item with ID '{itemId}' not found.");
        //         return;
        //     }
        //
        //     MMInventoryEvent.Trigger(
        //         MMInventoryEventType.Pick, null,
        //         item.TargetInventoryName, item, amount, 0, inv.playerId);
        //     // if (inv.playerInventory.AddItem(item, amount))
        //     //     // Event stays 'amount' so your PickupDisplayer shows the right number
        //     //     MMInventoryEvent.Trigger(MMInventoryEventType.Pick, null,
        //     //         item.TargetInventoryName, item, amount, 0, inv.playerId);
        // }

        [YarnCommand("remove_player_item")]
        public void RemovePlayerItem(string itemId)
        {
            InventoryHelperCommands.RemovePlayerItem(itemId);
        }

        // MMInventoryEvent.Trigger(MMInventoryEventType.Destroy, null, item);

        [YarnCommand("show_controls")]
        public void ShowControls(int actionId = -1)
        {
            var acID = actionId;
            if (actionId == -1) acID = defaultActionId;

            ControlsHelpEvent.Trigger(ControlHelpEventType.ShowThenHide, acID);
        }

        [YarnCommand("give_liquid_samples")]
        public void GiveLiquidSamples(int amount = -1)
        {
            if (amount == -1) BioSampleEvent.Trigger("All", BioSampleEventType.GiveToNPC, null, 0f);
        }


        [YarnCommand("test_command")]
        public void TestCommand()
        {
            Debug.Log("Test command executed.");
        }


        // ----------- Currency commands ----------

        [YarnCommand("give_player_money")]
        public void GivePlayerMoney(float amount)
        {
            CurrencyEvent.Trigger(CurrencyEventType.AddCurrency, amount);
            AlertEvent.Trigger(AlertReason.CurrencyGained, "You gained $" + amount.ToString("F2"), "Currency Gained");
        }

        [YarnCommand("try_remove_player_money")]
        public void TryRemovePlayerMoney(float amount)
        {
            CurrencyEvent.Trigger(CurrencyEventType.RemoveCurrency, amount);
        }

        // ----------- Objectives commands ----------

        // [YarnCommand("add_objective")]
        // public void AddObjective(string objectiveId)
        // {
        //     var objMgr = ObjectivesManager.Instance;
        //     if (objMgr == null)
        //     {
        //         Debug.LogWarning("ObjectivesManager not found, cannot add objective.");
        //         return;
        //     }
        //
        //     var obj = objMgr.GetObjectiveById(objectiveId);
        //     if (obj == null)
        //     {
        //         Debug.LogWarning($"Objective with ID '{objectiveId}' not found.");
        //         return;
        //     }
        //
        //     ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveAdded);
        //     AlertEvent.Trigger(AlertReason.NewObjective, obj.objectiveText, obj.objectiveId);
        // }
        //
        // [YarnCommand("activate_objective")]
        // public void ActivateObjective(string objectiveId)
        // {
        //     var objMgr = ObjectivesManager.Instance;
        //     if (objMgr == null)
        //     {
        //         Debug.LogWarning("ObjectivesManager not found, cannot add objective.");
        //         return;
        //     }
        //
        //     var obj = objMgr.GetObjectiveById(objectiveId);
        //     if (obj == null)
        //     {
        //         Debug.LogWarning($"Objective with ID '{objectiveId}' not found.");
        //         return;
        //     }
        //
        //     ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveActivated);
        //     AlertEvent.Trigger(AlertReason.NewObjective, obj.objectiveText, obj.objectiveId);
        // }
        //
        // [YarnCommand("make_objective_inactive")]
        // public void MakeObjectiveInactive(string objectiveId)
        // {
        //     var objMgr = ObjectivesManager.Instance;
        //     if (objMgr == null)
        //     {
        //         Debug.LogWarning("ObjectivesManager not found, cannot add objective.");
        //         return;
        //     }
        //
        //     var obj = objMgr.GetObjectiveById(objectiveId);
        //     if (obj == null)
        //     {
        //         Debug.LogWarning($"Objective with ID '{objectiveId}' not found.");
        //         return;
        //     }
        //
        //     ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveMadeInactive);
        // }
        //
        // [YarnCommand("mark_poi_as_having_new_content")]
        // public void MarkPOIAsHavingNewContent(string uniqueID)
        // {
        //     GamePOIEvent.Trigger(uniqueID, GamePOIEventType.POIMarkedAsHavingNewContent, null);
        // }
        //
        // [YarnCommand("complete_objective")]
        // public void CompleteObjective(string objectiveId)
        // {
        //     var objMgr = ObjectivesManager.Instance;
        //     if (objMgr == null)
        //     {
        //         Debug.LogWarning("ObjectivesManager not found, cannot complete objective.");
        //         return;
        //     }
        //
        //     var obj = objMgr.GetObjectiveById(objectiveId);
        //     if (obj == null)
        //     {
        //         Debug.LogWarning($"Objective with ID '{objectiveId}' not found.");
        //         return;
        //     }
        //
        //     ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveCompleted);
        // }

        // ----------- Shopping commands ----------
        [YarnCommand("start_shopping_buy")]
        public void StartShoppingBuy(string npcId)
        {
            ShoppingEvent.Trigger(npcId, ShoppingEventType.StartShoppingBuy);
        }
        //
        [YarnCommand("start_shopping_sell")]
        public void StartShoppingSell(string npcId)
        {
            ShoppingEvent.Trigger(npcId, ShoppingEventType.StartShoppingSell);
        }

        [YarnCommand("stop_shopping_buy")]
        public void StopShoppingBuy(string npcId)
        {
            ShoppingEvent.Trigger(npcId, ShoppingEventType.StopShoppingBuy);
        }

        [YarnCommand("stop_shopping_sell")]
        public void StopShoppingSell(string npcId)
        {
            ShoppingEvent.Trigger(npcId, ShoppingEventType.StopShoppingSell);
        }


        [YarnCommand("start_shopping_sell_illegal")]
        public void StartShoppingSellIllegal(string npcId)
        {
            ShoppingEvent.Trigger(npcId, ShoppingEventType.StartShoppingSellIllegal);
        }

        // ---------- Flow / nodes ----------

        // Persist a new starting node for an NPC (used by DialogueManager before StartDialogue).
        [YarnCommand("set_start_node")]
        public void SetStartNode(string npcId, string node)
        {
            DialogueStartNodeManager.Instance.SetStartNode(npcId, node);
        }

        // Optional: store a "next" node in variables (lets Yarn set where to enter next time).
        [YarnCommand("set_next_for")]
        public void SetNextFor(string npcId, string node)
        {
            if (variableStorage == null)
            {
                Debug.LogWarning("No variable storage available");
                return;
            }

            variableStorage.SetValue($"${npcId}.next", node);
        }


        // ---------- Events / gameplay side-effects ----------

        // Emit your DialogueEvent (you already use this on start/finish).
        [YarnCommand("emit_dialogue_event")]
        public void EmitDialogueEvent(string type, string npcId = "", string payload = "")
        {
            if (Enum.TryParse<DialogueEventType>(type, true, out var evtType))
                DialogueEvent.Trigger(evtType, npcId, payload);
            else
                Debug.LogWarning($"emit_dialogue_event: unknown type '{type}'");
        }

        [YarnCommand("end_playtest")]
        public void EndPlaytest()
        {
            AlertEvent.Trigger(
                AlertReason.PlayTestEndYesOrNo, "Will you end the playtest here? No additional content.",
                "End Playtest", AlertType.ChoiceModal, onConfirm:
                () => { Application.Quit(); },
                onCancel:
                () => { Application.Quit(); });
        }

        [YarnCommand("initiate_trade")]
        public void InitiateTrade(string npcId)
        {
            // This is a placeholder for your trade initiation logic    
            Debug.Log($"Initiating trade with NPC: {npcId}");
            // You can call your trade manager or UI here
        }


        // ---------- Time Based Commands ----------

        // Yarn will wait for this to finish


        [YarnCommand("halt_in_game_time")]
        public void HaltInGameTime(string npcId)
        {
            Debug.Log($"Halting in-game time with NPC: {npcId}");
            InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.Pause);
        }

        [YarnCommand("resume_in_game_time")]
        public void ResumeInGameTime(string npcId)
        {
            Debug.Log($"Resuming in-game time with NPC: {npcId}");
            InGameTimeActionEvent.Trigger(InGameTimeActionEvent.ActionType.Resume);
        }

        // [YarnCommand("wait")]
        // public IEnumerator Wait(float seconds)
        // {
        //     yield return new WaitForSeconds(seconds);
        // }
        // [YarnCommand("shrug")]
        // public void Shrug(string npcId)
        // {
        //     TriggerGesture(npcId, "shrug");
        // }
        //
        // [YarnCommand("greet")]
        // public void Greet(string npcId)
        // {
        //     TriggerGesture(npcId, "greet");
        // }
        //
        // [YarnCommand("smalltalk")]
        // public void SmallTalk(string npcId)
        // {
        //     TriggerGesture(npcId, "smalltalk");
        // }

        // [YarnCommand("trigger_gesture")]
        // public void TriggerGesture(string npcId, string key)
        // {
        //     // Find NPC by id in the scene
        //     if (characterNPCRoot == null)
        //     {
        //         Debug.LogError($"NPC '{npcId}' not found in scene.");
        //         return;
        //     }
        //
        //     var helper = characterNPCRoot.GetComponentInChildren<NPCCharacterAnimancerHelper>();
        //
        //     if (helper == null) return;
        //
        //     helper.PlayGesture(key);
        // }


        void TriggerSound(string npcId, string key)
        {
            if (characterNPCRoot == null)
            {
                Debug.LogError($"NPC '{npcId}' not found in scene.");
                return;
            }

            var helper = characterNPCRoot.GetComponentInChildren<NPCCharacterAnimancerHelper>();

            if (helper == null) return;

            helper.PlaySound(key);
        }

        //
        // [YarnCommand("scoffs")]
        // public void Scoffs(string npcId)
        // {
        //     TriggerGesture(npcId, "scoffs");
        // }
        //
        // [YarnCommand("pleased")]
        // public void Pleased(string npcId)
        // {
        //     TriggerGesture(npcId, "pleased");
        // }
        //
        // Character Sounds

        [YarnCommand("play_greet_sound")]
        public void PlayGreetSound(string npcId)
        {
            TriggerSound(npcId, "greet");
        }

        // ---------- Tutorial Media Commands ----------

        [YarnCommand("display_tutorial_image")]
        public void DisplayTutorialImage(string imageName)
        {
        }

        [YarnCommand("hide_tutorial_image")]
        public void HideTutorialImage()
        {
        }

        [YarnCommand("display_tutorial_video")]
        public void DisplayTutorialVideo(string videoName)
        {
        }

        [YarnCommand("hide_tutorial_video")]
        public void HideTutorialVideo()
        {
        }

        // ---------- Editor Helpers ----------

#if UNITY_EDITOR
        // This will be called from the parent ScriptableObject
        IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            var parent = ControlsPromptSchemeSet._currentContextSO;
            if (parent == null || parent.inputManagerPrefab == null) yield break;

            var data = parent.inputManagerPrefab.userData;
            if (data == null) yield break;

            foreach (var action in data.GetActions_Copy())
                yield return new ValueDropdownItem<int>(action.name, action.id);
        }
#endif
        public void RemoveDirigibleItem(string currentItemItemID)
        {
            InventoryHelperCommands.RemoveDirigibleItem(currentItemItemID);
        }
    }
}
