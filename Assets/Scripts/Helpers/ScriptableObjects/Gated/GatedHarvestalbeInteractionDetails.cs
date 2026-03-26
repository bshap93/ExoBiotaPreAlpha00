using System;
using System.Collections.Generic;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.ScriptableObjects.Gated
{
    [Serializable]
    public enum HarvestableInteractionType
    {
        DissolveWithSolvent,
        CatalyzeWithCatalyst
    }

    [CreateAssetMenu(
        fileName = "GatedHarvestalbeInteractionDetails",
        menuName = "Scriptable Objects/UI/Gated Harvestable Interaction Details", order = 2)]
    public class GatedHarvestalbeInteractionDetails : GatedInteractionDetailsBase

    {
        public OuterCoreItemObject item;
        [Header("General Settings")]
        // public GatedInteractionType gatedInteractionType = GatedInteractionType.HarvesteableBiological;
        public HarvestableInteractionType harvestableInteractionType;
        [FormerlySerializedAs("contaminationPerMinute")]
        [FormerlySerializedAs("harvestableInnerBaseItem")]
        [FormerlySerializedAs("harvestableInnerObjectID")]
        [FormerlySerializedAs("quantityYielded")]
        [FormerlySerializedAs("yieldsInnerObject")]
        [Header("Harvestable Yield Settings")]
        [Header("Contamination Settings")]
        // public float contaminationCostPerMinute;
        [FormerlySerializedAs("requiresSupplies")]
        [Header("Chemical Settings")]
        [ToggleLeft]
        [LabelText("Requires Chemical?")]
        public bool requiresChemical;
#if UNITY_EDITOR
        [FormerlySerializedAs("appropriateSupplyIDs")]
        [ShowIf(nameof(requiresChemical))]
        [LabelText("Required Chemical IDs")]
        [ValueDropdown(nameof(GetAllItemIDs))]
#endif
        public List<string> requiredChemicalIDs;


        [FormerlySerializedAs("appropriateChemicalEfficiencies")]
        [FormerlySerializedAs("appropriateCatalystEfficiencies")]
        [LabelText("Appropriate Chemical Efficiencies")]
        [SerializeField]
        [Range(0f, 1f)]
        public List<float> requiredChemicalEfficiencies;


        [Header("Tools Settings")]


#if UNITY_EDITOR
        [ShowIf(nameof(requiresChemical))]
        [LabelText("Most Efficient Chemical ID")]
        [ValueDropdown(nameof(GetAllItemIDs))]
#endif
        public string mostEfficientChemicalID;


        public string GetMostEfficientChemicalID(List<string> appropriateChemsFound)
        {
            var chemsFoundEfficiencies = new List<float>();
            for (var i = 0; i < requiredChemicalIDs.Count; i++)
                if (appropriateChemsFound.Contains(requiredChemicalIDs[i]))
                    chemsFoundEfficiencies.Add(requiredChemicalEfficiencies[i]);

            if (!requiresChemical || appropriateChemsFound.Count == 0 || chemsFoundEfficiencies.Count == 0)
                return null;

            var bestIndex = 0;
            var highestEfficiency = chemsFoundEfficiencies[0];

            for (var i = 1; i < chemsFoundEfficiencies.Count; i++)
                if (chemsFoundEfficiencies[i] > highestEfficiency)
                {
                    highestEfficiency = chemsFoundEfficiencies[i];
                    bestIndex = i;
                }

            mostEfficientChemicalID = appropriateChemsFound[bestIndex];

            return appropriateChemsFound[bestIndex];
        }
    }
}
