using System.Collections.Generic;
using UnityEngine;

namespace Helpers.ScriptableObjects.Gated
{
    [CreateAssetMenu(
        fileName = "GatedInteractionDetails", menuName = "Scriptable Objects/UI/Gated Interaction Details", order = 1)]
    public class GatedBreakableInteractionDetails : GatedInteractionDetailsBase
    {
        public string GetMostEfficientRequiredToolID(List<string> appropriateToolsFound)
        {
            var toolsFoundEfficiencies = new List<float>();
            for (var i = 0; i < requiredToolIDs.Count; i++)
                if (appropriateToolsFound != null)
                    if (appropriateToolsFound.Contains(requiredToolIDs[i]))
                        toolsFoundEfficiencies.Add(requiredToolEfficiency[i]);

            if (appropriateToolsFound != null &&
                (!requireTools || appropriateToolsFound.Count == 0 || toolsFoundEfficiencies.Count == 0))
                return null;

            var bestIndex = 0;

            if (toolsFoundEfficiencies.Count > 0)
            {
                var highestEfficiency = toolsFoundEfficiencies[0];

                for (var i = 1; i < toolsFoundEfficiencies.Count; i++)
                    if (toolsFoundEfficiencies[i] > highestEfficiency)
                    {
                        highestEfficiency = toolsFoundEfficiencies[i];
                        bestIndex = i;
                    }

                if (appropriateToolsFound != null)
                    return appropriateToolsFound[bestIndex];
            }


            return null;
        }
    }
}
