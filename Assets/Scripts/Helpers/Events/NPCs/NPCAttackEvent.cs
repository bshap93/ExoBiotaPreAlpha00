using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using MoreMountains.Tools;

namespace Helpers.Events.NPCs
{
    public struct NPCAttackEvent
    {
        static NPCAttackEvent _e;

        public EnemyAttack Attack;


        public static void Trigger(EnemyAttack attack)
        {
            _e.Attack = attack;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
