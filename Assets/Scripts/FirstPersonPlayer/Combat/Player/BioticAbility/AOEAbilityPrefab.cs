using System;
using System.Collections;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
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
    public class AOEAbilityPrefab : MonoBehaviour, IRuntimeBioticAbility, MMEventListener<ContaminationSpikeEvent>
    {
        [SerializeField] GameObject auraObject;

        [SerializeField] GameObject muzzleFlashPrefab;
        [SerializeField] GameObject aoeEffectPrefab; // Prefab for the AOE effect (e.g., explosion)
        [SerializeField] Collider abilityCollider; // Collider that defines the AOE range
        [SerializeField] Transform rootPosition;
        [SerializeField] Transform originPosition;
        [SerializeField] FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility
            abilityData; // Reference to the ScriptableObject for range and damage info
        [SerializeField] float cooldownTime = 1f; // Cooldown time in seconds
        [SerializeField] float delayBeforeAOEAfterFeedbacks = 0.4f;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks shootFeedbacks;
        [SerializeField] MMFeedbacks hitFeedbacks;
        [SerializeField] MMFeedbacks equipFeedbacks;
        [SerializeField] MMFeedbacks unequipFeedbacks;

        GameObject _aoeEffectInstance; // Instance of the AOE effect prefab
        ParticleSystem[] _aoeParticles;
        FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility _currentAbilityData;
        GameObject _muzzleFlashInstance;
        ParticleSystem[] _muzzleParticles;
        PlayerEquippedAbility _owner;

        bool _readyToFire = true;
        PlayerMutableStatsManager _statsManager;
        float _timeSinceLastUse;
        float ContaminationCostPerNormalUse
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                if (attrMgr == null) throw new Exception("AttributesManager instance not found");
                var bioticCompetency = attrMgr.Exobiotic;

                var finalCost = abilityData.GetContaminationCostForExobioticLevel(bioticCompetency);
                return Mathf.Max(0.1f, finalCost);
            }
        }

        void Awake()
        {
            if (aoeEffectPrefab != null && rootPosition != null)
            {
                _aoeEffectInstance = Instantiate(aoeEffectPrefab, rootPosition.position, rootPosition.rotation);
                _aoeEffectInstance.transform.SetParent(rootPosition);
                _aoeParticles = _aoeEffectInstance.GetComponentsInChildren<ParticleSystem>();

                foreach (var ps in _aoeParticles)
                    if (ps.isPlaying)
                        ps.Stop();
            }


            if (muzzleFlashPrefab != null && rootPosition != null)
            {
                _muzzleFlashInstance = Instantiate(muzzleFlashPrefab, originPosition.position, originPosition.rotation);
                _muzzleFlashInstance.transform.SetParent(rootPosition);
                _muzzleParticles = _muzzleFlashInstance.GetComponentsInChildren<ParticleSystem>();

                foreach (var p in _muzzleParticles)
                    if (p.isPlaying)
                        p.Stop();
            }
        }

        void Start()
        {
            _statsManager = PlayerMutableStatsManager.Instance;

            if (_statsManager == null)
            {
                Debug.LogWarning("[StunBeam] No stats manager found");
                return;
            }

            var currentContamination = _statsManager.CurrentContamination;

            if (ContaminationCostPerNormalUse > currentContamination)
                auraObject.SetActive(false);
            else
                auraObject.SetActive(true);
        }
        void Update()
        {
            if (_timeSinceLastUse < abilityData.baseCooldownTime)
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
            if (_aoeEffectInstance != null) Destroy(_aoeEffectInstance);

            if (_muzzleFlashInstance != null) Destroy(_muzzleFlashInstance);
        }
        public void Activate(FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility abilityData,
            Transform originTransform)
        {
            // UseTool scheme doesn't use Activate - handled in Use() instead   
        }
        public IRuntimeBioticAbility.UsageScheme GetUsageScheme()
        {
            return IRuntimeBioticAbility.UsageScheme.UseTool;
        }
        public void Deactivate()
        {
            // UseTool scheme doesn't use Deactivate - handled in Use() instead
        }
        public bool IsActive()
        {
            return false; // UseTool scheme doesn't use IsActive
        }
        public void Initialize(PlayerEquippedAbility owner)
        {
            _owner = owner;
            if (_owner != null && _owner.bioticAbilityAnchor != null)
            {
                transform.SetParent(_owner.bioticAbilityAnchor);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
            }
        }
        public void Use()
        {
            if (!_readyToFire)
            {
                Debug.Log("On cooldown");
                return;
            }

            if (_currentAbilityData == null)
            {
                Debug.LogError("Ability data not set for AOEAbilityPrefab.");
                return;
            }

            if (PlayerMutableStatsManager.Instance.CurrentContamination < ContaminationCostPerNormalUse)
            {
                // Not enough stamina
                AlertEvent.Trigger(
                    AlertReason.NotEnoughContamination, "Not enough contamination to use ability.",
                    "Insufficient Contamination");


                return;
            }

            StartCoroutine(FireAOEAbility());

            _readyToFire = false;
            _timeSinceLastUse = 0f;
        }
        public void Unequip()
        {
            unequipFeedbacks?.PlayFeedbacks();
        }
        public void Equip()
        {
            gameObject.SetActive(true);
            equipFeedbacks?.PlayFeedbacks();
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
        void PlayAOEParticles()
        {
            if (_aoeParticles == null || _aoeParticles.Length == 0)
                return;

            foreach (var ps in _aoeParticles)
                if (ps != null)
                    ps.Play();
        }

        void PlayMuzzleFlash()
        {
            if (_muzzleParticles == null || _muzzleParticles.Length == 0)
                return;

            foreach (var ps in _muzzleParticles)
                if (ps != null)
                    ps.Play();
        }
        IEnumerator FireAOEAbility()
        {
            shootFeedbacks?.PlayFeedbacks();
            yield return new WaitForSeconds(delayBeforeAOEAfterFeedbacks);

            PlayAOEParticles();

            PlayMuzzleFlash();

            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentContamination,
                PlayerStatsEvent.PlayerStatChangeType.Decrease,
                ContaminationCostPerNormalUse);

            ApplyAOEEffect();

            if (_statsManager.CurrentContamination < ContaminationCostPerNormalUse)
                auraObject.SetActive(false);
        }

        void ApplyAOEEffect()
        {
            if (abilityCollider == null)
            {
                Debug.LogWarning("[AOEAbility] No abilityCollider assigned — cannot detect targets.");
                return;
            }

            // Gather all colliders that overlap with the AOE collider's world-space bounds.
            // Using OverlapBox against the collider's bounds gives us a fast broad-phase check;
            // we then verify each candidate is actually inside the collider below.
            var bounds = abilityCollider.bounds;
            var halfExtents = bounds.extents;
            var overlapping = Physics.OverlapBox(
                bounds.center,
                halfExtents,
                abilityCollider.transform.rotation,
                ~0, // all layers
                QueryTriggerInteraction.Collide // include trigger colliders (creatures may use them)
            );

            var attack = _currentAbilityData?.GetPlayerAttack();
            var specialType = _currentAbilityData?.GetSpecialType();


            foreach (var col in overlapping)
            {
                // Skip self
                if (col == abilityCollider) continue;

                // Confirm the collider's closest point is actually inside our AOE collider
                // (OverlapBox uses the AABB, so diagonal shapes can produce false positives)
                var closestPoint = abilityCollider.ClosestPoint(col.transform.position);
                if (closestPoint != col.transform.position &&
                    Vector3.Distance(closestPoint, col.transform.position) > 0.05f)
                    continue;

                if (!col.CompareTag("EnemyNPC") && !col.CompareTag("FriendlyNPC")) continue;

                var creature = col.GetComponentInParent<CreatureController>();
                if (creature == null) continue;

                // Susceptibility check: ignore dead creatures
                if (creature.CurrentCreatureState == CreatureController.CreatureState.Dead) continue;

                // Spawn hit VFX on the creature
                var vfxPrefab = creature.GetEffectsAndFeedbacks()?.basicHitVFX;
                if (vfxPrefab != null)
                {
                    var hitPos = col.ClosestPoint(bounds.center);
                    var vfxInstance = Instantiate(vfxPrefab, hitPos, Quaternion.identity);
                    var vfxFeedbacks = vfxInstance.GetComponent<MMFeedbacks>();
                    vfxFeedbacks?.PlayFeedbacks();
                    Destroy(vfxInstance, 2f);
                }

                // Apply damage
                if (attack != null)
                {
                    creature.ProcessAttackDamage(attack, bounds.center);
                    Debug.Log($"[AOEAbility] Hit {creature.name}");
                }

                if (specialType != null && specialType !=
                    FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility.SpecialEffectType.None)
                    creature.ProcessSpecialEffect(specialType.Value);

                hitFeedbacks?.PlayFeedbacks();
            }
        }

        // Public method to set ability data (called during equip)
        public void SetAbilityData(FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility abilityData)
        {
            _currentAbilityData = abilityData;
        }
    }
}
