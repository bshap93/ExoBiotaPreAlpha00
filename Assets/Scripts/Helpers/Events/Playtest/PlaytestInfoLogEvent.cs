using MoreMountains.Tools;

namespace Helpers.Events.Playtest
{
    public enum PlaytestInfoLogEventType
    {
        Intro
    }

    public struct PlaytestInfoLogEvent
    {
        static PlaytestInfoLogEvent _e;

        public PlaytestInfoLogEventType Type;

        public static void Trigger(PlaytestInfoLogEventType type)
        {
            _e.Type = type;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
