using System;
using System.Collections;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.InputHandling;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events;
using Helpers.Events.Spawn;
using Helpers.Events.Status;
using Manager;
using Manager.ProgressionMangers;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;

namespace FirstPersonPlayer.Combat.Player.BioticAbility
{
    public class SingleProjectileAbilityPrefab : MonoBehaviour, IRuntimeBioticAbility,
        MMEventListener<ContaminationSpikeEvent>
    {
        [SerializeField] GameObject projectilePrefab;
        [SerializeField] GameObject muzzleFlashPrefab;
        [SerializeField] float projectileSpeed = 15f;
        [SerializeField] FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility
            abilityData; // Reference to the ScriptableObject for range and damage info
        [Header("Muzzle / Spawn Point")] [SerializeField]
        Transform muzzlePosition;
        [Header("Aiming")] [SerializeField] LayerMask aimRaycastMask = ~0;
        [SerializeField] float maxRange = 100f;
        [Header("Feedbacks")] [SerializeField] MMFeedbacks equipFeedbacks;
        [SerializeField] MMFeedbacks unequipFeedbacks;
        [SerializeField] MMFeedbacks shootFeedbacks;
        [Header("Cooldown")] [SerializeField] float cooldownTime = 0.5f;
        [SerializeField] GameObject auraObject;


        [SerializeField] LayerMask projectileLayerMask = -1;
        FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility _currentAbilityData;
        Camera _mainCamera;
        GameObject _muzzleFlashInstance;
        ParticleSystem[] _muzzleParticles;
        PlayerEquippedAbility _owner;
        PlayerMutableStatsManager _playerMutableStatsManager;
        bool _readyToFire = true;
        float _timeSinceLastUse;

        float ContaminationCostPerNormalUse
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                var bioticCompetency = attrMgr.Exobiotic;
                if (attrMgr == null) throw new Exception("AttributesManager instance not found");

                var finalCost = abilityData.GetContaminationCostForExobioticLevel(bioticCompetency);
                return Mathf.Max(0.1f, finalCost);
            }
        }

        void Awake()
        {
            // Setup persistent muzzle flash (reusable particle system)
            if (muzzleFlashPrefab != null && muzzlePosition != null)
            {
                _muzzleFlashInstance = Instantiate(muzzleFlashPrefab, muzzlePosition.position, muzzlePosition.rotation);
                _muzzleFlashInstance.transform.SetParent(muzzlePosition);
                _muzzleParticles = _muzzleFlashInstance.GetComponentsInChildren<ParticleSystem>();

                foreach (var ps in _muzzleParticles)
                    if (ps.isPlaying)
                        ps.Stop();
            }
        }
        void Start()
        {
            _playerMutableStatsManager = PlayerMutableStatsManager.Instance;

            if (_playerMutableStatsManager == null)
            {
                Debug.LogWarning("[StunBeam] No stats manager found");
                return;
            }


            var currentContamination = _playerMutableStatsManager.CurrentContamination;

            if (ContaminationCostPerNormalUse > currentContamination)
                auraObject.SetActive(false);
            else
                auraObject.SetActive(true);
        }
        void Update()
        {
            if (_timeSinceLastUse < cooldownTime)
                _timeSinceLastUse += Time.deltaTime;
            else
                _readyToFire = true;
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        void OnDestroy()
        {
            if (_muzzleFlashInstance != null)
                Destroy(_muzzleFlashInstance);
        }


        public void Activate(FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility abilityData,
            Transform originTransform)
        {
        }
        public IRuntimeBioticAbility.UsageScheme GetUsageScheme()
        {
            return IRuntimeBioticAbility.UsageScheme.UseTool;
        }
        public void Deactivate()
        {
        }
        public bool IsActive()
        {
            return false;
        }
        public void Initialize(PlayerEquippedAbility owner)
        {
            _owner = owner;
            _mainCamera = Camera.main;

            if (_owner != null && _owner.bioticAbilityAnchor != null)
            {
                // Position the ability prefab correctly
                transform.SetParent(_owner.bioticAbilityAnchor);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
        public void Use()
        {
            if (!_readyToFire)
            {
                Debug.Log("[ProjectileAbility] On cooldown");
                return;
            }

            if (_currentAbilityData == null)
            {
                Debug.LogWarning("[ProjectileAbility] No ability data assigned");
                return;
            }

            if (PlayerMutableStatsManager.Instance.CurrentContamination < ContaminationCostPerNormalUse)
            {
                AlertEvent.Trigger(
                    AlertReason.NotEnoughContamination,
                    "Not enough contamination to use ability.",
                    "Insufficient Contamination");

                return;
            }

            StartCoroutine(FireProjectile());

            _readyToFire = false;
            _timeSinceLastUse = 0f;
        }

        public void Equip()
        {
            gameObject.SetActive(true);
            equipFeedbacks?.PlayFeedbacks();
        }

        public void Unequip()
        {
            unequipFeedbacks?.PlayFeedbacks();
        }
        public bool CanInteractWithObject(GameObject colliderGameObject)
        {
            return true;
        }

        public bool AbilityMustBeHeldToUse()
        {
            return false;
        }

        public bool CanAbortAction()
        {
            return true;
        }

        public MMFeedbacks GetEquipFeedbacks()
        {
            return equipFeedbacks;
        }

        public MMFeedbacks GetUnequipFeedbacks()
        {
            return unequipFeedbacks;
        }
        public void OnMMEvent(ContaminationSpikeEvent eventType)
        {
            var newContaminationAmt = eventType.NewContaminationAmt;

            if (newContaminationAmt >= ContaminationCostPerNormalUse)
                auraObject.SetActive(true);
        }

        public void SetAbilityData(FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility abilityData)
        {
            _currentAbilityData = abilityData;
        }

        IEnumerator ApplyAttackLunge(PlayerAttack attack, float delay)
        {
            if (attack == null || !attack.playerMovesWithAttack) yield break;

            yield return new WaitForSeconds(delay * 0.3f);

            // Cache this reference in Initialize() instead of finding it each time
            var movement = FindFirstObjectByType<MyNormalMovement>();
            if (movement == null) yield break;

            var forward = _mainCamera.transform.forward;
            forward.y = 0f;
            forward.Normalize();

            var lungeDuration = 0.15f;
            var elapsed = 0f;
            var lungeSpeed = attack.movementAmount;

            while (elapsed < lungeDuration)
            {
                var t = elapsed / lungeDuration;
                var factor = 1f - t * t;

                movement.SetAttackLungeVelocity(forward * (lungeSpeed * factor));

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Clear it when done
            movement.SetAttackLungeVelocity(Vector3.zero);
        }

        IEnumerator FireProjectile()
        {
            if (!_mainCamera)
                _mainCamera = Camera.main;

            if (!_mainCamera)
                yield break;

            // Play shoot feedbacks (animation, sound, recoil, etc.)
            shootFeedbacks?.PlayFeedbacks();

            // Deduct contamination cost
            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentContamination,
                PlayerStatsEvent.PlayerStatChangeType.Decrease,
                ContaminationCostPerNormalUse);

            StartCoroutine(ApplyAttackLunge(_currentAbilityData.GetPlayerAttack(), 0.1f));


            // Play muzzle flash
            PlayMuzzleFlash();

            // ── Determine aim target point ──
            // Raycast from screen center to find what the player is looking at
            var camTransform = _mainCamera.transform;
            var ray = new Ray(camTransform.position, camTransform.forward);
            var targetPoint = Physics.Raycast(
                ray, out var hit, maxRange, aimRaycastMask, QueryTriggerInteraction.Ignore)
                ? hit.point
                : ray.GetPoint(maxRange);

            // ── Spawn position & direction ──
            var spawnPos = muzzlePosition != null
                ? muzzlePosition.position
                : camTransform.position + camTransform.forward * 0.5f;

            var shootDirection = (targetPoint - spawnPos).normalized;

            // Spawn muzzle flash prefab at spawn point (one-shot, separate from the persistent one)
            // The persistent muzzle particles are played above via PlayMuzzleFlash()

            // ── Instantiate projectile ──
            var projectile = Instantiate(projectilePrefab, spawnPos, Quaternion.LookRotation(shootDirection));

            // ── Configure Rigidbody velocity ──
            var rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = false;
                rb.useGravity = false;

#if UNITY_6000_0_OR_NEWER
                rb.linearVelocity = shootDirection * projectileSpeed;
#else
                rb.velocity = shootDirection * projectileSpeed;
#endif
            }
            else
            {
                Debug.LogWarning(
                    $"[ProjectileAbility] Projectile prefab '{projectilePrefab.name}' has no Rigidbody. " +
                    "Add a Rigidbody for physics-based movement, or ensure the prefab moves itself.");
            }

            // ── Set projectile layer ──
            if (projectileLayerMask != -1)
                projectile.layer = GetLayerFromMask(projectileLayerMask);

            Debug.Log($"[ProjectileAbility] Fired projectile at {projectileSpeed} m/s, direction: {shootDirection}");

            if (_playerMutableStatsManager.CurrentContamination < ContaminationCostPerNormalUse)
                auraObject.SetActive(false);
        }

        static int GetLayerFromMask(LayerMask mask)
        {
            var layerNumber = 0;
            var layer = mask.value;
            while (layer > 1)
            {
                layer >>= 1;
                layerNumber++;
            }

            return layerNumber;
        }
        void PlayMuzzleFlash()
        {
            if (_muzzleParticles == null || _muzzleParticles.Length == 0)
                return;

            foreach (var ps in _muzzleParticles)
                if (ps != null)
                    ps.Play();
        }
    }
}
