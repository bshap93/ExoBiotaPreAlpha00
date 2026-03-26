using Manager.StateManager;
using MoreMountains.Tools;

namespace Helpers.Events.NPCs
{
    public struct CreatureSpecialStateEvent
    {
        static CreatureSpecialStateEvent _e;

        public string UniqueID;
        public CreatureStateManager.CreatureSpecialState CreatureSpecialState;

        public static void Trigger(string uniqueID, CreatureStateManager.CreatureSpecialState creatureSpecialState)
        {
            _e.UniqueID = uniqueID;
            _e.CreatureSpecialState = creatureSpecialState;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
