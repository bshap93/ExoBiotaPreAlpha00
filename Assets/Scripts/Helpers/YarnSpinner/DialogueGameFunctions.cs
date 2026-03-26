using FirstPersonPlayer.FPNPCs;
using Inventory;
using Manager;
using Manager.ProgressionMangers;
using Manager.SceneManagers;
using Manager.StateManager;
using UnityEngine;
using Yarn.Unity;

namespace Helpers.YarnSpinner
{
    public class DialogueGameFunctions : MonoBehaviour
    {
        [Tooltip("If not assigned, will try DialogueManager.Instance.dialogueRunner")]
        public DialogueRunner dialogueRunner;


        // Scene/ Travel


        // Progression Getters

        [YarnFunction("get_current_class_id")]
        public static int GetUnsetClassStatus()
        {
            if (LevelingManager.Instance != null)
            {
                var playerClassId = LevelingManager.Instance.CurrentPlayerClass.id;
                return playerClassId;
            }

            return 0;
        }

        [YarnFunction("is_playtest")]
        public static bool IsPlaytest()
        {
            var coreBuildSettingsMgr = PlaytestSettingsManager.Instance;
            if (coreBuildSettingsMgr != null) return coreBuildSettingsMgr.IsPlayTest;
            return false;
        }

        [YarnFunction("get_current_elevator_destination")]
        public static string GetCurrentElevatorDestination(string elevatorSystemUniqueId)
        {
            var elevatorManager = ElevatorManager.Instance;
            if (elevatorManager != null)
            {
                var destination = elevatorManager.GetDestination(elevatorSystemUniqueId);
                return destination ?? "None";
            }

            return "None";
        }

        [YarnFunction("get_creature_initialization_state")]
        public static string GetCreatureSpecialState(string uniqueId)
        {
            var creatureStateManager = CreatureStateManager.Instance;
            if (creatureStateManager != null)
            {
                var specialState = creatureStateManager.GetCreatureSpecialState(uniqueId);
                return specialState.ToString();
            }

            return "None";
        }


        [YarnFunction("has_npc_been_contacted")]
        public static bool HasNPCBeenContacted(string npcId)
        {
            if (FriendlyNPCManager.Instance != null)
                return FriendlyNPCManager.Instance.HasNPCBeenContactedAtLeastOnce(npcId);

            Debug.LogError("FriendlyNPCManager instance is null. Returning false for NPC contact status.");
            return false;
        }

        [YarnFunction("get_current_level")]
        public static int GetCurrentLevel()
        {
            if (LevelingManager.Instance != null) return LevelingManager.Instance.CurrentLevel;

            Debug.LogError("LevelingManager instance is null. Returning 0 for current level.");
            return 0;
        }


        [YarnFunction("get_attribute_points_unused")]
        public static int GetAttributePointsUnused()
        {
            if (LevelingManager.Instance != null) return LevelingManager.Instance.UnspentAttributePoints;
            Debug.LogError("LevelingManager instance is null. Returning 0 for unspent attribute points.");
            return 0;
        }

        // Attribute Getters

        [YarnFunction("get_dexterity")]
        public static int GetDexterity()
        {
            return AttributesManager.Instance.Dexterity;
        }

        [YarnFunction("get_agility")]
        public static int GetAgility()
        {
            return AttributesManager.Instance.Agility;
        }


        [YarnFunction("get_strength")]
        public static int GetStrength()
        {
            return AttributesManager.Instance.Strength;
        }

        [YarnFunction("get_toughness")]
        public static int GetToughness()
        {
            return AttributesManager.Instance.Toughness;
        }


        [YarnFunction("get_biotic_level")]
        public static int GetBioticLevel()
        {
            return AttributesManager.Instance.Exobiotic;
        }

        // Quests

        [YarnFunction("has_started_quest")]
        public static bool HasStartedQuest(string questId)
        {
            if (FriendlyNPCManager.Instance != null)
                return FriendlyNPCManager.Instance.HasQuestBeenStarted(questId);

            Debug.LogError("FriendlyNPCManager instance is null. Returning false for quest started status.");
            return false;
        }

        [YarnFunction("has_completed_quest")]
        public static bool HasCompletedQuest(string questId)
        {
            if (FriendlyNPCManager.Instance != null)
                return FriendlyNPCManager.Instance.HasQuestBeenCompleted(questId);

            Debug.LogError("FriendlyNPCManager instance is null. Returning false for quest completed status.");
            return false;
        }

        [YarnFunction("get_quantity_of_item_player_has")]
        public static int GetQuantityOfItemPlayerHas(string itemId)
        {
            if (GlobalInventoryManager.Instance != null)
                return GlobalInventoryManager.Instance.GetTotalQuantityOfItem(itemId);

            Debug.LogError("InventoryManager instance is null. Returning 0 for item quantity.");
            return 0;
        }

        public class AttributFunctions
        {
        }
    }
}
