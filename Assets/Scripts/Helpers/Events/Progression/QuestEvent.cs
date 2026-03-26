using MoreMountains.Tools;

namespace Helpers.Events.Progression
{
    public struct QuestEvent
    {
        public enum QuestEventType
        {
            Started,
            Completed
        }

        static QuestEvent _e;

        public string QuestID;

        public QuestEventType Type;


        public static void Trigger(string questID, QuestEventType type)
        {
            _e.QuestID = questID;
            _e.Type = type;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
