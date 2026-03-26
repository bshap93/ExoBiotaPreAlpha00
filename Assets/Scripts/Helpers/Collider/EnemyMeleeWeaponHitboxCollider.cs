using FirstPersonPlayer.Combat.AINPC.EnemyWeapon;
using Helpers.Events.NPCs;
using UnityEngine;

namespace Helpers.Collider
{
    public class EnemyMeleeWeaponHitboxCollider : MonoBehaviour
    {
        [SerializeField] EnemyMeleeWeaponPrefab weaponPrefab;

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            if (other.CompareTag("FirstPersonPlayer"))
            {
                weaponPrefab.SetHitBoxActive(false);
                NPCAttackEvent.Trigger(weaponPrefab.CurrentAttack);
            }
        }

    }
}
