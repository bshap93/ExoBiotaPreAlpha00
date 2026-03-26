using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public struct CriticalHitEvent
    {
        static CriticalHitEvent _e;

        public enum WhoseCriticalHit
        {
            Player,
            Enemy
        }

        public WhoseCriticalHit MyWhoseCriticalHit;
        public float Multipler;

        public static void Trigger(WhoseCriticalHit whoseCriticalHit, float multipler)
        {
            _e.MyWhoseCriticalHit = whoseCriticalHit;
            _e.Multipler = multipler;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
