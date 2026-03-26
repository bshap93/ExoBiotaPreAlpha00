using System.Collections.Generic;
using Helpers.Events.Gated;
using Helpers.ScriptableObjects.Gated;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public struct GatedMachineInteractionEvent
    {
        static GatedMachineInteractionEvent _e;
        public GatedInteractionEventType EventType;

        public GatedMachineInteractionDetails Details;
        public string SubjectUniqueID;
        public List<string> FuelBatteriesFound;
        public List<string> ToolsFound;

        public static void Trigger(GatedInteractionEventType eventType, GatedMachineInteractionDetails details,
            string subjectUniqueID, List<string> fuelBatteriesFound, List<string> toolsFound)
        {
            _e.Details = details;
            _e.EventType = eventType;
            _e.SubjectUniqueID = subjectUniqueID;
            _e.FuelBatteriesFound = fuelBatteriesFound;
            _e.ToolsFound = toolsFound;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
