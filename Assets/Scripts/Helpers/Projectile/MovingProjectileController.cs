using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using Helpers.Events.NPCs;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Helpers.Projectile
{
    public class MovingProjectileController : MonoBehaviour
    {
        [SerializeField] GameObject burstEffectPrefab;
        [SerializeField] bool isEnemyAttack;
        [ShowIf("isEnemyAttack")] [SerializeField]
        EnemyAttack enemyAttack;
        [SerializeField] bool isPlayerAttack;
        [ShowIf("isPlayerAttack")] [SerializeField]
        PlayerAttack playerAttack;
        [SerializeField] float maxLifetime = 5f;
        [SerializeField] LayerMask ignoreLayers;

        float _lifetimeTimer;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _lifetimeTimer = maxLifetime;
        }

        // Update is called once per frame
        void Update()
        {
            _lifetimeTimer -= Time.deltaTime;
            if (_lifetimeTimer <= 0f) Destroy(gameObject);
        }

        void OnTriggerEnter(UnityEngine.Collider other)
        {
            // Ignore collisions with specified layers
            if (((1 << other.gameObject.layer) & ignoreLayers) != 0) return;
            // Instantiate burst effect at the collision point
            if (burstEffectPrefab != null) Instantiate(burstEffectPrefab, transform.position, Quaternion.identity);
            if (isEnemyAttack && other.CompareTag("FirstPersonPlayer"))
                NPCAttackEvent.Trigger(enemyAttack);

            if (isPlayerAttack && other.CompareTag("EnemyNPC"))
            {
                var enemyController = other.GetComponentInParent<CreatureController>();

                if (enemyController == null)
                {
                    Debug.LogWarning(
                        "Player projectile collided with an object tagged 'EnemyNPC' that does not have a CreatureController component.");

                    return;
                }

                enemyController.ProcessAttackDamage(
                    playerAttack,
                    other.ClosestPoint(transform.position));
            }

            Destroy(gameObject);
        }
    }
}
