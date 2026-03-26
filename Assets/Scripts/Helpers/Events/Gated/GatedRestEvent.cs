using Helpers.ScriptableObjects.Gated;
using MoreMountains.Tools;

namespace Helpers.Events.Gated
{
    public struct GatedRestEvent
    {
        static GatedRestEvent _e;
        public GatedInteractionEventType EventType;
        public GatedRestDetails RestDetails;
        public int RestTimeMinutes;
        public string DockId;

        public static void Trigger(GatedInteractionEventType eventType, GatedRestDetails restDetails,
            int restTimeMinutes, string dockId)
        {
            _e.EventType = eventType;
            _e.RestDetails = restDetails;
            _e.RestTimeMinutes = restTimeMinutes;
            _e.DockId = dockId;


            MMEventManager.TriggerEvent(_e);
        }
    }
}
