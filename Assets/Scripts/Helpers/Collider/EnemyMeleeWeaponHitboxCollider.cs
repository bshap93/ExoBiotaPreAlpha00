using FirstPersonPlayer.Combat.AINPC.EnemyWeapon;
using Helpers.Events.NPCs;
using UnityEngine;

namespace Helpers.Collider
{
    public class EnemyMeleeWeaponHitboxCollider : MonoBehaviour
    {
        [SerializeField] EnemyMeleeWeaponPrefab weaponPrefab;
        [SerializeField] protected UnityEngine.Collider weaponHitboxCollider;

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer"))
            {
                weaponPrefab.SetHitBoxActive(false);
                weaponPrefab.HasHitThisSwing = true;
                NPCAttackEvent.Trigger(weaponPrefab.CurrentAttack);
            }
        }

        public void EnableHitboxCollider(bool enable)
        {
            weaponHitboxCollider.enabled = enable;
            if (enable) weaponPrefab.HasHitThisSwing = false; // Reset only when arming a new swing
        }
    }
}
