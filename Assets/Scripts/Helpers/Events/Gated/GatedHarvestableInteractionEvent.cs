using System.Collections.Generic;
using Helpers.ScriptableObjects.Gated;
using MoreMountains.Tools;

namespace Helpers.Events.Gated
{
    public struct GatedHarvestableInteractionEvent
    {
        static GatedHarvestableInteractionEvent _e;
        public GatedInteractionEventType EventType;

        public GatedHarvestalbeInteractionDetails Details;
        public string SubjectUniqueID;
        public List<string> ChemicalsFound;
        public List<string> ToolsFound;

        public static void Trigger(GatedInteractionEventType eventType, GatedHarvestalbeInteractionDetails details,
            string subjectUniqueID, List<string> chemicalsFound, List<string> toolsFound)
        {
            _e.Details = details;
            _e.EventType = eventType;
            _e.SubjectUniqueID = subjectUniqueID;
            _e.ToolsFound = toolsFound;
            _e.ChemicalsFound = chemicalsFound;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
