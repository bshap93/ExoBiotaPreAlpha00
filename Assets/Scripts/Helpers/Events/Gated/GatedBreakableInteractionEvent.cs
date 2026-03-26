using System;
using System.Collections.Generic;
using Helpers.ScriptableObjects.Gated;
using MoreMountains.Tools;

namespace Helpers.Events.Gated
{
    [Serializable]
    public enum GatedInteractionEventType
    {
        CloseGatedInteractionUI,
        TriggerGateUI,
        StartInteraction,
        CompleteInteraction
    }

    public enum GatedInteractionType
    {
        // This includes mining and breaking things for their yield
        NotGated,
        BreakObstacle,
        HarvesteableBiological,
        InteractMachine,
        Rest
    }

    public struct GatedBreakableInteractionEvent
    {
        static GatedBreakableInteractionEvent _e;
        public GatedInteractionEventType EventType;

        public GatedBreakableInteractionDetails Details;
        public string SubjectUniqueID;
        public List<string> ToolsFound;


        public static void Trigger(GatedInteractionEventType eventType, GatedBreakableInteractionDetails details,
            string subjectUniqueID, List<string> toolsFound)
        {
            _e.Details = details;
            _e.EventType = eventType;
            _e.SubjectUniqueID = subjectUniqueID;
            _e.ToolsFound = toolsFound;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
