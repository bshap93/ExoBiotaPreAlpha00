using MoreMountains.Tools;

namespace Helpers.Events.PlayerData
{
    public enum JournalTopicEventType
    {
        Added,
        Updated,
        Initialized
    }

    public struct JournalTopicEvent
    {
        static JournalTopicEvent _e;

        public string JournalTopicUniqueId;
        public JournalTopicEventType EventType;

        public static void Trigger(JournalTopicEventType eventType, string topicUniqueId = null)
        {
            _e.EventType = eventType;
            _e.JournalTopicUniqueId = topicUniqueId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
