using MoreMountains.Tools;

namespace Helpers.Events
{
    public struct InGameTimeUpdateEvent
    {
        static InGameTimeUpdateEvent _e;

        public int MinutesIntoDay;
        public int DayNumber;
        public int MinutesElapsed;

        public static void Trigger(int minutesIntoDay, int dayNumber, int minutesElapsed)
        {
            _e.MinutesIntoDay = minutesIntoDay;
            _e.DayNumber = dayNumber;
            _e.MinutesElapsed = minutesElapsed;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
