using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum BioOrganismEventType
    {
    }

    public struct BioOrganismEvent
    {
        private static BioOrganismEvent _e;

        public BioOrganismEventType EventType;
        public string UniqueId;


        public static void Trigger(BioOrganismEventType eventType, string uniqueId)
        {
            _e.EventType = eventType;
            _e.UniqueId = uniqueId;

            MMEventManager.TriggerEvent(_e);
        }
    }
}