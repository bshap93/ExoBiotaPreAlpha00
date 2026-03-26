using System.Collections.Generic;
using System.Linq;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events.Gated;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.ScriptableObjects.Gated
{
    public abstract class GatedInteractionDetailsBase : ScriptableObject
    {
        [Header("General Settings")] public string interactionName;
        public GatedInteractionType gatedInteractionType;

        [FormerlySerializedAs("timeToCompleteInteraction")]
        [Header("Timing and Costs")]
        [LabelText("Time to Complete (Mins)")]
        public int timeCostMins;
        [Header("Timing and Costs")]
        [LabelText("Real-World Wait Duration (Seconds)")]
        [Tooltip("How long the player actually waits in real time")]
        public float realWorldWaitDuration = 5f;
        public float staminaCost;

        [Header("Item Yield Settings")] [ToggleLeft] [LabelText("Yields Item?")]
        public bool yieldsItem;

        [ShowIf(nameof(yieldsItem))] [LabelText("Item Yielded Quantity")] [MinValue(0)]
        public int itemYieldedQuantity;

        [FormerlySerializedAs("harvestableInnerBaseItem")] [ShowIf(nameof(yieldsItem))] [LabelText("Yielded Base Item")]
        public MyBaseItem yieldedBaseItem;

        [Header("Contamination Settings")] [ToggleLeft] [LabelText("Contaminates Player?")]
        public bool contaminatesPlayer;
        [ShowIf(nameof(contaminatesPlayer))] [LabelText("Contamination Cost Per Minute")]
        public float contaminationCostPerMinute;


        [Header("Requirements")] [ToggleLeft] [LabelText("Requires Tools?")]
        public bool requireTools;

        [ShowIf(nameof(requireTools))] [LabelText("Required Tool Efficiencies")] [Range(0f, 1f)]
        public List<float> requiredToolEfficiency;

#if UNITY_EDITOR
        [ShowIf(nameof(requireTools))] [LabelText("Most Efficient Tool ID")] [ValueDropdown(nameof(GetAllItemIDs))]
#endif
        public string mostEfficientToolID;

#if UNITY_EDITOR
        [ShowIf(nameof(requireTools))] [LabelText("Required Tool IDs")] [ValueDropdown(nameof(GetAllToolIDs))]
#endif
        public List<string> requiredToolIDs;

        public float TotalContaminationCost => contaminationCostPerMinute * timeCostMins;


        public string ObjectID => name;


        public string GetMostEfficientToolID(List<string> appropriateToolsFound)
        {
            var toolsFoundEfficiencies = new List<float>();

            for (var i = 0; i < requiredToolIDs.Count; i++)
                if (appropriateToolsFound.Contains(requiredToolIDs[i]))
                    toolsFoundEfficiencies.Add(requiredToolEfficiency[i]);

            if (!requireTools || appropriateToolsFound.Count == 0 || toolsFoundEfficiencies.Count == 0)
                return null;

            var bestIndex = 0;
            var highestEfficiency = toolsFoundEfficiencies[0];

            for (var i = 1; i < toolsFoundEfficiencies.Count; i++)
                if (toolsFoundEfficiencies[i] > highestEfficiency)
                {
                    highestEfficiency = toolsFoundEfficiencies[i];
                    bestIndex = i;
                }

            mostEfficientToolID = appropriateToolsFound[bestIndex];

            return appropriateToolsFound[bestIndex];
        }

#if UNITY_EDITOR
        protected static string[] GetAllItemIDs()
        {
            // Assumes all item ScriptableObjects are under Resources/Items
            var items = Resources.LoadAll<MyBaseItem>("Items");

            // You can replace 'ScriptableObject' with your actual base class type if you have one, e.g. 'ItemBase'
            return items
                .Select(i => i.name)
                .Distinct()
                .OrderBy(n => n)
                .ToArray();
        }

        // ✅ Dropdown for only BaseTool items (subclass of MyBaseItem)
        public static IEnumerable<string> GetAllToolIDs()
        {
            var allItems = Resources.LoadAll<MyBaseItem>("Items");
            var toolItems = allItems.OfType<BaseTool>();

            return toolItems
                .Select(tool => tool.name)
                .Distinct()
                .OrderBy(name => name)
                .ToArray();
        }
#endif
    }
}
