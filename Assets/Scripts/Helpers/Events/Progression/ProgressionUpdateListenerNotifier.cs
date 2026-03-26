using MoreMountains.Tools;

namespace Helpers.Events.Progression
{
    public struct ProgressionUpdateListenerNotifier
    {
        static ProgressionUpdateListenerNotifier _e;

        public int CurrentTotalXP;
        public int CurrentLevel;
        // public int CurrentUpgradesUnused;
        public int CurrentAttributePointsUnused;

        public static void Trigger(int currentTotalXP, int currentLevel, //int currentUpgradesUnused,
            int currentAttributePointsUnused)
        {
            _e.CurrentTotalXP = currentTotalXP;
            _e.CurrentLevel = currentLevel;
            // _e.CurrentUpgradesUnused = currentUpgradesUnused;
            _e.CurrentAttributePointsUnused = currentAttributePointsUnused;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
