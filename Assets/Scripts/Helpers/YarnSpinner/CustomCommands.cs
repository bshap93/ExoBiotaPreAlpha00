using System.Collections;
using FirstPersonPlayer.Interactable.ResourceBoxes;
using Helpers.Events;
using Helpers.Events.Dialog;
using Helpers.Events.Machine;
using Helpers.Events.ManagerEvents;
using Helpers.Events.NPCs;
using Helpers.Events.PlayerData;
using Helpers.Events.Progression;
using Helpers.Events.Progression.Scenario;
using Helpers.Events.Status;
using Helpers.Events.Terminals;
using Helpers.Events.Triggering;
using Inventory;
using Manager;
using Manager.ProgressionMangers;
using MoreMountains.InventoryEngine;
using Objectives;
using Overview.NPC;
using OWPData.Structs;
using SharedUI.Progression;
using UnityEngine;
using Yarn.Unity;

namespace Helpers.YarnSpinner
{
    public class CustomCommands : MonoBehaviour
    {
        // Drag and drop your Dialogue Runner into this variable.
        public DialogueRunner dialogueRunner;
        public GameObject characterNPCRoot;

        public void Awake()
        {
            // Create a new command called 'camera_look', which looks at a target. 
            // Note how we're listing 'GameObject' as the parameter type.
            dialogueRunner.AddCommandHandler(
                "camera_look", // the name of the command
                CameraLookAtTarget // the method to run
            );

            // Inventory Commands

            dialogueRunner.AddCommandHandler<string, int>(
                "give_player_item",
                GivePlayerItem
            );

            // Dialogue Gestures

            dialogueRunner.AddCommandHandler<string, string>(
                "trigger_gesture",
                TriggerGesture
            );

            dialogueRunner.AddCommandHandler<string, string>(
                "switch_idle_animation",
                SwitchIdleLoopingAnimation
            );

            // ----------- Objectives commands ----------

            dialogueRunner.AddCommandHandler<string>(
                "add_objective",
                AddObjective
            );

            dialogueRunner.AddCommandHandler<string>(
                "activate_objective",
                ActivateObjective
            );

            dialogueRunner.AddCommandHandler<string>(
                "make_objective_inactive",
                MakeObjectiveInactive
            );

            dialogueRunner.AddCommandHandler<string>(
                "complete_objective",
                CompleteObjective
            );

            dialogueRunner.AddCommandHandler<string>(
                "mark_poi_as_having_new_content",
                MarkPOIAsHavingNewContent
            );

            // dialogueRunner.AddCommandHandler<int>(
            //     "trigger_stat_upgrade",
            //     TriggerStatUpgrade
            // );

            dialogueRunner.AddCommandHandler<int>(
                "trigger_player_sets_class",
                TriggerPlayerSetsClass
            );

            dialogueRunner.AddCommandHandler<int>(
                "trigger_player_increment_attribute",
                TriggerPlayerIncrementAttribute);

            dialogueRunner.AddCommandHandler<string>(
                "unlock_door",
                UnlockDoor);

            dialogueRunner.AddCommandHandler<string>(
                "open_door",
                OpenDoor
            );

            dialogueRunner.AddCommandHandler<int>(
                "heal_player",
                HealPlayer);

            dialogueRunner.AddCommandHandler(
                "save_game",
                SaveGame);

            dialogueRunner.AddCommandHandler<string, string>("set_spawn_point", SetSpawnPoint);

            dialogueRunner.AddCommandHandler("set_last_spawn_point", () => StartCoroutine(SetLastSpawnPoint()));

            dialogueRunner.AddCommandHandler<int>(
                "fast_travel_to_terminal",
                FastTravelToTerminal);

            dialogueRunner.AddCommandHandler<string>(
                "make_contact_with_npc",
                MakeContactWithNPC);

            dialogueRunner.AddCommandHandler<string>(
                "started_quest",
                StartedQuest);

            dialogueRunner.AddCommandHandler<string>(
                "completed_quest",
                CompletedQuest);

            dialogueRunner.AddCommandHandler(
                "trigger_fade_out",
                TriggerFadeOut);

            dialogueRunner.AddCommandHandler(
                "trigger_fade_in",
                TriggerFadeIn);

            dialogueRunner.AddCommandHandler<string, string>(
                "trigger_elevator_move", TriggerElevatorMoveToDestination);

            dialogueRunner.AddCommandHandler<string>(
                "trigger_scene_load", TriggerSceneLoad);

            dialogueRunner.AddCommandHandler<string>(
                "trigger_scene_unload", TriggerSceneUnload);

            dialogueRunner.AddCommandHandler<string>(
                "trigger_aquire_journal_entry", TriggerAquireJournalEntry);

            // scenario methods
            dialogueRunner.AddCommandHandler<string, string, bool>(
                "set_scenario_flag_value", SetScenarioFlagValue);

            dialogueRunner.AddCommandHandler<string, string, int>(
                "set_scenario_counter_value", SetScenarioCounterValue);

            dialogueRunner.AddCommandHandler<string, string>(
                "increment_scenario_counter_value", IncrementScenarioCounterValue);

            dialogueRunner.AddCommandHandler<string, string>(
                "decrement_scenario_counter_value", DecrementScenarioCounterValue);

            // Resource methods

            // 0 = AddResource, 1 = RemoveResource, 2 = SetCurrency
            dialogueRunner.AddCommandHandler<int, float>(
                "remove_player_resources", RemovePlayerResources);

            dialogueRunner.AddCommandHandler<int, float>(
                "add_player_resources", AddPlayerResources);
        }

        // The method that gets called when '<<camera_look>>' is run.
        void CameraLookAtTarget()
        {
            Debug.LogWarning("Looking at target: ");
        }

        void TriggerElevatorMoveToDestination(string elevatorSystemUniqueId, string destinationId)
        {
            ElevatorRootSystemEvent.Trigger(elevatorSystemUniqueId, destinationId);
        }

        void TriggerAquireJournalEntry(string journalEntryId)
        {
            JournalEntryEvent.Trigger(JournalEntryEventType.Added, journalEntryId);
        }

        // Game State Save

        void MakeContactWithNPC(string npcId)
        {
            MakeContactWithNPCEvent.Trigger(npcId);
        }

        void TriggerFadeOut()
        {
            DialogueCameraEvent.Trigger(DialogueCameraEventType.FadeOut);
        }

        void TriggerFadeIn()
        {
            DialogueCameraEvent.Trigger(DialogueCameraEventType.FadeIn);
        }

        void TriggerSceneLoad(string sceneName)
        {
            MySceneTransitionAdditiveEvent.Trigger(
                MySceneTransitionAdditiveEvent.MySceneTransEventType.Load, sceneName);
        }

        void TriggerSceneUnload(string sceneName)
        {
            MySceneTransitionAdditiveEvent.Trigger(
                MySceneTransitionAdditiveEvent.MySceneTransEventType.Unload, sceneName);
        }

        void SetSpawnPoint(string sceneName, string spawnPointId)
        {
            var info = new SpawnInfo
            {
                sceneName = sceneName,
                spawnPointId = spawnPointId,
                mode = GameMode.FirstPerson
            };

            PlayerSpawnManager.Instance.Save(info); // writes checkpoint
        }

        IEnumerator SetLastSpawnPoint()
        {
            var info = PlayerSpawnManager.Instance.LastAssignedSpawn;

            PlayerSpawnManager.Instance.Save(info); // writes checkpoint

            yield return null;
        }

        void SaveGame()
        {
            SaveDataEvent.Trigger();
            AlertEvent.Trigger(
                AlertReason.SavingGame, "All data saved successfully!", "Saved Game",
                alertIcon: PlayerUIManager.Instance.defaultIconRepository.saveConsoleIcon);
        }

        void StartedQuest(string questId)
        {
            QuestEvent.Trigger(questId, QuestEvent.QuestEventType.Started);
        }

        void CompletedQuest(string questId)
        {
            QuestEvent.Trigger(questId, QuestEvent.QuestEventType.Completed);
        }

        // Scenario Commands
        void SetScenarioFlagValue(string scenarioID, string flagName, bool value)
        {
            ScenarioBoolValueEvent.Trigger(scenarioID, flagName, value);
        }

        void SetScenarioCounterValue(string scenarioID, string counterName, int value)
        {
            ScenarioIntValueEvent.Trigger(
                ScenarioIntValueEvent.ScenarioDataEventType.SetValue, scenarioID, counterName, value);
        }

        void IncrementScenarioCounterValue(string scenarioID, string counterName)
        {
            ScenarioIntValueEvent.Trigger(
                ScenarioIntValueEvent.ScenarioDataEventType.IncrementValue, scenarioID, counterName, 1);
        }

        void DecrementScenarioCounterValue(string scenarioID, string counterName)
        {
            ScenarioIntValueEvent.Trigger(
                ScenarioIntValueEvent.ScenarioDataEventType.DecrementValue, scenarioID, counterName, 1);
        }

        // Currency

        void RemovePlayerResources(int resourceTypeEnumIndex, float amount)
        {
            ResourceCurrencyEvent.Trigger(
                ResourceCurrencyEventType.RemoveResource, amount,
                (ResourceCollectionContainerInteractable.ResourceType)resourceTypeEnumIndex);
        }

        void AddPlayerResources(int resourceTypeEnumIndex, float amount)
        {
            ResourceCurrencyEvent.Trigger(
                ResourceCurrencyEventType.AddResource, amount,
                (ResourceCollectionContainerInteractable.ResourceType)resourceTypeEnumIndex);
        }

        // Healing

        void HealPlayer(int amount)
        {
            var isPlayerHealthMaxed = false;
            var statsMgr = PlayerMutableStatsManager.Instance;
            if (statsMgr != null)
                if (statsMgr.CurrentHealth >= statsMgr.CurrentMaxHealth - 0.5f)
                    isPlayerHealthMaxed = true;

            if (!isPlayerHealthMaxed)
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentHealth,
                    PlayerStatsEvent.PlayerStatChangeType.Increase,
                    amount);
        }

        void FastTravelToTerminal(int terminalId)
        {
            Debug.Log("Fast traveling to terminal with ID: " + terminalId);
        }

        // Inventory Commands

        public void GivePlayerItem(string itemId, int amount = 1)
        {
            Debug.Log($"[Yarn] give_player_item on {name} (instanceID={GetInstanceID()}) x{amount}");

            var inv = GlobalInventoryManager.Instance;
            if (inv == null)
            {
                Debug.LogWarning("GlobalInventoryManager not found, cannot give item.");
                return;
            }

            var item = inv.CreateItem(itemId); // SINGLE unit item
            if (item == null)
            {
                Debug.LogWarning($"Item with ID '{itemId}' not found.");
                return;
            }

            MMInventoryEvent.Trigger(
                MMInventoryEventType.Pick, null,
                item.TargetInventoryName, item, amount, 0, inv.playerId);
        }


        // Progression Commands
        // void TriggerStatUpgrade(int typeId)
        // {
        //     if (typeId < 0 || typeId >= Enum.GetValues(typeof(StatType)).Length)
        //     {
        //         Debug.LogWarning($"Invalid StatType id: {typeId}");
        //         return;
        //     }
        //
        //     var statType = (StatType)typeId;
        //
        //     SpendStatUpgradeEvent.Trigger(statType);
        // }

        void TriggerPlayerSetsClass(int classId)
        {
            if (classId < 1 || classId >= LevelingManager.Instance.availablePresetClasses.Length)
            {
                Debug.LogWarning($"Invalid class id: {classId}");
                return;
            }

            PlayerSetsClassEvent.Trigger(classId);
        }

        void TriggerPlayerIncrementAttribute(int attributeId)
        {
            if (attributeId < 0 || attributeId > 5)
            {
                Debug.LogWarning($"Invalid AttributeType id: {attributeId}");
                return;
            }

            var attributeType = (AttributeType)attributeId;

            IncrementAttributeEvent.Trigger(attributeType);
        }

        // Change Environment Commands

        void UnlockDoor(string uniqueId)
        {
            DoorEvent.Trigger(uniqueId, DoorEventType.Unlock);
        }

        void OpenDoor(string uniqueId)
        {
            DoorEvent.Trigger(uniqueId, DoorEventType.Open);
        }

        // Dialogue Gestures


        public void TriggerGesture(string npcId, string key)
        {
            // Find NPC by id in the scene
            if (characterNPCRoot == null)
            {
                Debug.LogError($"NPC '{npcId}' not found in scene.");
                return;
            }

            var helper = characterNPCRoot.GetComponentInChildren<NPCCharacterAnimancerHelper>();

            if (helper == null) return;

            helper.PlayGesture(key);
        }

        public void SwitchIdleLoopingAnimation(string npcId, string key)
        {
            // Find NPC by id in the scene
            if (characterNPCRoot == null)
            {
                Debug.LogError($"NPC '{npcId}' not found in scene.");
                return;
            }

            var helper = characterNPCRoot.GetComponentInChildren<NPCCharacterAnimancerHelper>();

            if (helper == null) return;

            helper.SwitchIdleLoopingAnimation(key);
        }

        // ----------- Objectives commands ----------

        public void AddObjective(string objectiveId)
        {
            var objMgr = ObjectivesManager.Instance;
            if (objMgr == null)
            {
                Debug.LogWarning("ObjectivesManager not found, cannot add objective.");
                return;
            }

            var obj = objMgr.GetObjectiveById(objectiveId);
            if (obj == null)
            {
                Debug.LogWarning($"Objective with ID '{objectiveId}' not found.");
                return;
            }

            ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveAdded);
            AlertEvent.Trigger(AlertReason.NewObjective, obj.objectiveText, obj.objectiveId);
        }

        public void ActivateObjective(string objectiveId)
        {
            var objMgr = ObjectivesManager.Instance;
            if (objMgr == null)
            {
                Debug.LogWarning("ObjectivesManager not found, cannot add objective.");
                return;
            }

            var obj = objMgr.GetObjectiveById(objectiveId);
            if (obj == null)
            {
                Debug.LogWarning($"Objective with ID '{objectiveId}' not found.");
                return;
            }

            ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveActivated);
            AlertEvent.Trigger(AlertReason.NewObjective, obj.objectiveText, obj.objectiveId);
        }

        public void MakeObjectiveInactive(string objectiveId)
        {
            var objMgr = ObjectivesManager.Instance;
            if (objMgr == null)
            {
                Debug.LogWarning("ObjectivesManager not found, cannot add objective.");
                return;
            }

            var obj = objMgr.GetObjectiveById(objectiveId);
            if (obj == null)
            {
                Debug.LogWarning($"Objective with ID '{objectiveId}' not found.");
                return;
            }

            ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveDeactivated);
        }

        public void MarkPOIAsHavingNewContent(string uniqueID)
        {
            GamePOIEvent.Trigger(uniqueID, GamePOIEventType.POIMarkedAsHavingNewContent, null);
        }

        public void CompleteObjective(string objectiveId)
        {
            var objMgr = ObjectivesManager.Instance;
            if (objMgr == null)
            {
                Debug.LogWarning("ObjectivesManager not found, cannot complete objective.");
                return;
            }

            var obj = objMgr.GetObjectiveById(objectiveId);
            if (obj == null)
            {
                Debug.LogWarning($"Objective with ID '{objectiveId}' not found.");
                return;
            }

            ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveCompleted);
        }
    }
}
