using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public struct PlayerToolUseRootMovementEvent
    {
        static PlayerToolUseRootMovementEvent _e;

        public PlayerAttack Attack;

        public static void Trigger(PlayerAttack attack)
        {
            _e.Attack = attack;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
