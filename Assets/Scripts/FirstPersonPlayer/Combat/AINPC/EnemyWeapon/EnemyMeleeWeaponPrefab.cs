using Helpers.Collider;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.EnemyWeapon
{
    public class EnemyMeleeWeaponPrefab : EnemyWeaponPrefab
    {
        [SerializeField] EnemyMeleeWeaponHitboxCollider hitboxColliderScript;

        public bool HasHitThisSwing { get; set; }


        void Awake()
        {
            hitboxColliderScript.EnableHitboxCollider(false);
        }

        public override void SetHitBoxActive(bool active)
        {
            base.SetHitBoxActive(active);
            hitboxColliderScript.EnableHitboxCollider(active);
        }
    }
}
