using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.ScriptableObjects;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.EnemyWeapon
{
    public class EnemyWeaponPrefab : MonoBehaviour
    {
        [SerializeField] protected EnemyWeaponDefinition weaponDefinition;

        public EnemyAttack CurrentAttack { get; private set; }
        public bool IsHitBoxActive { get; set; }
        public virtual void SetHitBoxActive(bool p0)
        {
            IsHitBoxActive = p0;
        }
        public void SetAttack(EnemyAttack attack)
        {
            CurrentAttack = attack;
        }
    }
}
