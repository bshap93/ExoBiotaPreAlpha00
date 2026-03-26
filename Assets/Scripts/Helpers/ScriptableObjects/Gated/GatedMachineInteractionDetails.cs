using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.ScriptableObjects.Gated
{
    [Serializable]
    public enum GatedMachineType
    {
        Generator
    }

    [Serializable]
    public enum MachineInteractionType
    {
        AddBatteryLikeItem,
        RepairMachine
    }

    [Serializable]
    public enum MachineStatus
    {
        Off,
        Operational
    }

    [CreateAssetMenu(
        fileName = "GatedMachineInteractionDetails",
        menuName = "Scriptable Objects/UI/Gated Machine Interaction Details", order = 3)]
    public class GatedMachineInteractionDetails : GatedInteractionDetailsBase
    {
        [FormerlySerializedAs("machineType")] [Header("Machine-Specific")]
        public GatedMachineType gatedMachineType;

        public string machineName;
        public Sprite machineIcon;
        public string machineDescription;

        [Header("Action Details")] public string actionDescription;
        public Sprite actionIcon;

        [Header("Effect Details")] public float effectMagnitude;
        public string effectDescription;
        public Sprite effectIcon;

        [Header("Requirements")] [ToggleLeft] [LabelText("Requires Fuel or Battery?")]
        public bool takesFuelBatteryItem;
        // requires tools is inherited

#if UNITY_EDITOR
        [ShowIf(nameof(takesFuelBatteryItem))]
        [LabelText("Most Effective Fuel or Battery")]
        [ValueDropdown(nameof(GetAllItemIDs))]
#endif
        public string mostEffectiveFuelBatteryID;

#if UNITY_EDITOR
        [ShowIf(nameof(takesFuelBatteryItem))]
        [LabelText("Compatible Fuel/Battery Items")]
        [ValueDropdown(nameof(GetAllItemIDs))]
#endif
        public List<string> compatibleFuelBatteryIDs;

        [ShowIf(nameof(takesFuelBatteryItem))] [LabelText("Required Tool Efficiencies")] [Range(0f, 1f)]
        public List<float> compatibleFuelBatteryEfficiencies;


        public MachineInteractionType machineInteractionType;

        public MachineStatus targetMachineStatus;
        public object GetMostEfficientFuelBatteryItemID(List<string> approriateFuelBatteriesFound)
        {
            var batteriesFoundEfficiencies = new List<float>();

            for (var i = 0; i < compatibleFuelBatteryIDs.Count; i++)
                if (approriateFuelBatteriesFound.Contains(compatibleFuelBatteryIDs[i]))
                    batteriesFoundEfficiencies.Add(compatibleFuelBatteryEfficiencies[i]);

            if (!takesFuelBatteryItem || approriateFuelBatteriesFound.Count == 0 ||
                batteriesFoundEfficiencies.Count == 0)
                return null;

            var bestIndex = 0;
            var highestEfficiency = batteriesFoundEfficiencies[0];

            for (var i = 1; i < batteriesFoundEfficiencies.Count; i++)
                if (batteriesFoundEfficiencies[i] > highestEfficiency)
                {
                    highestEfficiency = batteriesFoundEfficiencies[i];
                    bestIndex = i;
                }

            mostEffectiveFuelBatteryID = approriateFuelBatteriesFound[bestIndex];

            return approriateFuelBatteriesFound[bestIndex];
        }
    }
}
