using Manager.StateManager;
using MoreMountains.Tools;

namespace Helpers.Events.NPCs
{
    public enum CreatureStateEventType
    {
        SetNewCreatureState
    }

    public struct CreatureInitializationStateEvent
    {
        static CreatureInitializationStateEvent _e;

        public CreatureStateEventType EventType;
        public string UniqueID;
        public CreatureStateManager.CreatureInitializationState CreatureInitializationState;

        public static void Trigger(CreatureStateEventType eventType, string uniqueID,
            CreatureStateManager.CreatureInitializationState creatureInitializationState)
        {
            _e.EventType = eventType;
            _e.UniqueID = uniqueID;
            _e.CreatureInitializationState = creatureInitializationState;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
