using Manager.FirstPerson;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public struct PlayerDeathEvent
    {
        static PlayerDeathEvent _e;

        public DeathInformation DeathInformation;

        public static void Trigger(DeathInformation deathInformation)
        {
            _e.DeathInformation = deathInformation;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
