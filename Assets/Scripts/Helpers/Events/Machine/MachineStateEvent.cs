using FirstPersonPlayer.Interactable.Gated;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public struct MachineStateEvent
    {
        static MachineStateEvent _e;

        public string UniqueID;
        public InteractableMachine.MachineState State;

        public static void Trigger(string uniqueID, InteractableMachine.MachineState state)
        {
            _e.UniqueID = uniqueID;
            _e.State = state;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
