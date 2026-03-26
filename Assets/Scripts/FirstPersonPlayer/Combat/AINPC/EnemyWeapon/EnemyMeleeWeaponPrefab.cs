using Helpers.Collider;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.EnemyWeapon
{
    public class EnemyMeleeWeaponPrefab : EnemyWeaponPrefab
    {
        [SerializeField] protected Collider weaponHitboxCollider;

        void Awake()
        {
            weaponHitboxCollider.enabled = false;
        }

        public override void SetHitBoxActive(bool active)
        {
            base.SetHitBoxActive(active);
            weaponHitboxCollider.enabled = active;
        }
    }
}
