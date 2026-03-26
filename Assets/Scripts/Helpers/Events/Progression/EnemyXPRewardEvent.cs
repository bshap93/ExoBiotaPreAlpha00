using MoreMountains.Tools;

namespace Helpers.Events.Progression
{
    public struct EnemyXPRewardEvent
    {
        static EnemyXPRewardEvent _e;

        public int XPReward;

        public static void Trigger(int xpReward)
        {
            _e.XPReward = xpReward;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
