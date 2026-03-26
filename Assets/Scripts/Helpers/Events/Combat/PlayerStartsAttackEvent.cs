using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public enum AttackType
    {
        Normal,
        Heavy
    }


    public struct PlayerStartsAttackEvent
    {
        static PlayerStartsAttackEvent _e;

        public PlayerAttack Attack;
        public string CreatureUniqueId;

        public static void Trigger(PlayerAttack attack, string creatureUniqueId)
        {
            _e.Attack = attack;
            _e.CreatureUniqueId = creatureUniqueId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
