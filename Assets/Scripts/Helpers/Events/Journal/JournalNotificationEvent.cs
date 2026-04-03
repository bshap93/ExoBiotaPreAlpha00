using MoreMountains.Tools;

namespace Helpers.Events.Journal
{
    public enum JournalEntityType
    {
        Topic,
        Entry
    }

    public struct JournalNotificationEvent
    {
        static JournalNotificationEvent _e;

        public JournalEntityType EntityType;
        public string EntityName;
        public string CategoryName;

        public static void Trigger(JournalEntityType entityType, string entryDataEntryName, string entityName)
        {
            _e.EntityType = entityType;
            _e.EntityName = entityName;
            _e.CategoryName = entryDataEntryName;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
