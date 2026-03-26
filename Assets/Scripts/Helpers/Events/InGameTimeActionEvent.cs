using MoreMountains.Tools;
using Structs;

namespace Helpers.Events
{
    public struct InGameTimeActionEvent
    {
        static InGameTimeActionEvent _e;


        public enum ActionType
        {
            Pause,
            Resume,
            LapseTime,
            StopLapseTime
        }

        public ActionType ActionTypeIG;

        public GameMode CurrentGameMode;


        public int NumMinutes;

        public static void Trigger(ActionType action, int numMinutes = 0, GameMode gameMode = GameMode.FirstPerson)
        {
            _e.ActionTypeIG = action;
            _e.NumMinutes = numMinutes;
            _e.CurrentGameMode = gameMode;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
