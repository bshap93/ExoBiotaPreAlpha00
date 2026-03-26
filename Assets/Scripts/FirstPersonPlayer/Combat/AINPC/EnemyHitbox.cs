using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC
{
    public class EnemyHitbox : MonoBehaviour
    {
        public EnemyController owner;
        [SerializeField] AttackUsed attackType;
        bool _active;
        Collider _collider;
        bool _hasHit;

        void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        void Update()
        {
            // Actively check for player overlap every frame while active
            if (_active && !_hasHit) CheckForPlayerHit();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!_active || _hasHit) return;

            if (other.CompareTag("FirstPersonPlayer"))
            {
                owner.OnHitPlayer(other, attackType);
                _hasHit = true;
            }
        }

        void OnTriggerStay(Collider other)
        {
            if (!_active || _hasHit) return;

            if (other.CompareTag("FirstPersonPlayer"))
            {
                owner.OnHitPlayer(other, attackType);
                _hasHit = true;
            }
        }

        public void Activate()
        {
            _active = true;
            _hasHit = false;
        }

        public void Deactivate()
        {
            _active = false;
            _hasHit = false;
        }

        /// <summary>
        ///     Manually check for player collision every frame while hitbox is active
        ///     This ensures reliable detection regardless of trigger events
        /// </summary>
        void CheckForPlayerHit()
        {
            if (_collider == null || _hasHit) return;

            Collider[] hits;

            // Use appropriate overlap check based on collider type
            if (_collider is BoxCollider box)
                hits = Physics.OverlapBox(
                    _collider.bounds.center,
                    _collider.bounds.extents,
                    transform.rotation,
                    ~0 // Check all layers (you can refine this)
                );
            else if (_collider is SphereCollider sphere)
                hits = Physics.OverlapSphere(
                    _collider.bounds.center,
                    sphere.radius * Mathf.Max(transform.lossyScale.x, transform.lossyScale.y, transform.lossyScale.z),
                    ~0
                );
            else
                // Fallback for other collider types
                hits = Physics.OverlapBox(
                    _collider.bounds.center,
                    _collider.bounds.extents,
                    transform.rotation,
                    ~0
                );

            foreach (var hit in hits)
                if (hit.CompareTag("FirstPersonPlayer"))
                {
                    owner.OnHitPlayer(hit, attackType);
                    _hasHit = true;
                    break;
                }
        }
    }
}
