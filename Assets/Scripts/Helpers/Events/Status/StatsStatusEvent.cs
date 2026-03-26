using MoreMountains.Tools;

namespace Helpers.Events.Status
{
    // Categorical Complement to PlayerStatsEvent
    public struct StatsStatusEvent
    {
        static StatsStatusEvent _e;


        public bool Enabled;

        public enum StatsStatusType
        {
            Health,
            Stamina,
            Contamination
        }

        public enum StatsStatus
        {
            IsMax,
            IsMin,
            IsLow,
            IsHigh
        }


        public StatsStatus Status;
        public StatsStatusType StatType;

        public static void Trigger(bool enabled, StatsStatus status, StatsStatusType statType)
        {
            _e.Enabled = enabled;
            _e.Status = status;
            _e.StatType = statType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
