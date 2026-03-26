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
    public class SingleBeamAbilityPrefab : MonoBehaviour, IRuntimeBioticAbility,
        MMEventListener<ContaminationSpikeEvent>
    {
        [Header("Beam Settings")] [SerializeField]
        LineRenderer[] beamLineRenderers;
        [SerializeField] Transform muzzlePosition;

        [SerializeField] float beamWidth = 0.05f;
        [SerializeField] float beamDuration = 0.15f;
        [SerializeField] Color beamColor = new(0.5f, 0.8f, 1f, 1f); // Stun beam color
        [SerializeField] int numberOfBeams = 2;
        [SerializeField] float beamVerticalSpacing = 0.03f;
        [SerializeField] FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility
            abilityData; // Reference to the ScriptableObject for range and damage info

        [SerializeField] GameObject auraObject;

        [Header("Delays")] [SerializeField] float delayBeforeBeamAfterFeedbacks;


        [SerializeField] LayerMask hitMask = ~0;

        [Header("Visual Effects")] [SerializeField]
        GameObject muzzleFlashPrefab;
        [SerializeField] GameObject hitSparksPrefab;
        [SerializeField] GameObject missSparksPrefab;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks equipFeedbacks;
        [SerializeField] MMFeedbacks unequipFeedbacks;
        [SerializeField] MMFeedbacks shootFeedbacks;
        [SerializeField] MMFeedbacks hitFeedbacks;
        [SerializeField] MMFeedbacks missFeedbacks;
        FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility _currentAbilityData;
        Camera _mainCamera;
        GameObject _muzzleFlashInstance;
        ParticleSystem[] _muzzleParticles;

        // Runtime state
        PlayerEquippedAbility _owner;
        bool _readyToFire = true;

        PlayerMutableStatsManager _statsManager;
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

        float AbilityRange
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                var bioticCompetency = attrMgr.Exobiotic;
                if (attrMgr == null) throw new Exception("AttributesManager instance not found");

                return abilityData.GetAbilityRangeForExobioticLevel(bioticCompetency);
            }
        }

        void Awake()
        {
            SetupBeamRenderers();

            // Setup persistent muzzle flash
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
            if (_muzzleFlashInstance != null)
                Destroy(_muzzleFlashInstance);
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
            // UseTool scheme doesn't use Deactivate
        }

        public bool IsActive()
        {
            return false; // UseTool scheme doesn't have an "active" state
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
                Debug.Log("[StunBeam] On cooldown");
                return;
            }

            if (_currentAbilityData == null)
            {
                Debug.LogWarning("[StunBeam] No ability data assigned");
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

            // Fire the beam
            StartCoroutine(FireBeam());

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

        IEnumerator FireBeam()
        {
            if (!_mainCamera)
                _mainCamera = Camera.main;

            if (!_mainCamera)
                yield break;


            // Play feedbacks
            shootFeedbacks?.PlayFeedbacks();
            yield return new WaitForSeconds(delayBeforeBeamAfterFeedbacks);


            PlayMuzzleFlash();


            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentContamination, PlayerStatsEvent.PlayerStatChangeType.Decrease,
                ContaminationCostPerNormalUse);

            // Perform raycast
            var ray = new Ray(_mainCamera.transform.position, _mainCamera.transform.forward);
            var range = _currentAbilityData != null ? AbilityRange : 10f;
            var didHit = Physics.Raycast(ray, out var hit, range, hitMask, QueryTriggerInteraction.Ignore);
            var endPoint = didHit ? hit.point : ray.GetPoint(range);

            // Draw beam
            StartCoroutine(DrawBeam(muzzlePosition.position, endPoint));

            // Process hit
            if (didHit)
                ProcessHit(hit);
            else
                missFeedbacks?.PlayFeedbacks();

            if (_statsManager.CurrentContamination < ContaminationCostPerNormalUse)
                auraObject.SetActive(false);
        }

        void ProcessHit(RaycastHit hit)
        {
            var go = hit.collider.gameObject;

            // Hit enemy NPC
            if (go.CompareTag("EnemyNPC"))
            {
                var creatureController = go.GetComponentInParent<CreatureController>();
                if (creatureController != null)
                {
                    // Spawn hit VFX
                    var vfx = creatureController.GetEffectsAndFeedbacks().basicHitVFX;
                    SpawnHitFX(vfx, hit.point, hit.normal);

                    // Apply stun damage
                    var attack = _currentAbilityData?.GetPlayerAttack();
                    if (attack != null)
                    {
                        creatureController.ProcessAttackDamage(attack, hit.point);
                        Debug.Log($"[StunBeam] Hit enemy: {creatureController.name}");
                    }

                    hitFeedbacks?.PlayFeedbacks();
                }
            }
            // Hit breakable object
            else if (go.TryGetComponent<IBreakable>(out var breakable))
            {
                breakable.ApplyHit(1, hit.point, hit.normal);
                SpawnHitFX(hitSparksPrefab, hit.point, hit.normal);
                hitFeedbacks?.PlayFeedbacks();
            }
            // Hit generic surface
            else
            {
                SpawnHitFX(hitSparksPrefab, hit.point, hit.normal);
                hitFeedbacks?.PlayFeedbacks();
            }
        }

        void SpawnHitFX(GameObject vfxPrefab, Vector3 position, Vector3 normal)
        {
            if (vfxPrefab == null)
                return;

            var vfxInstance = Instantiate(vfxPrefab, position, Quaternion.LookRotation(normal));
            var vfxFeedbacks = vfxInstance.GetComponent<MMFeedbacks>();
            vfxFeedbacks?.PlayFeedbacks();
            Destroy(vfxInstance, 2f);
        }

        void SetupBeamRenderers()
        {
            if (beamLineRenderers == null || beamLineRenderers.Length == 0)
            {
                Debug.LogWarning("[StunBeam] No beam LineRenderers assigned");
                return;
            }

            numberOfBeams = Mathf.Min(numberOfBeams, beamLineRenderers.Length);

            for (var i = 0; i < beamLineRenderers.Length; i++)
            {
                var beam = beamLineRenderers[i];
                if (beam != null)
                {
                    beam.enabled = false;
                    beam.startColor = beamColor;
                    beam.endColor = beamColor;
                    beam.startWidth = beamWidth;
                    beam.endWidth = beamWidth * 0.8f;
                    beam.positionCount = 2;

                    // Hide unused beams
                    if (i >= numberOfBeams)
                        beam.gameObject.SetActive(false);
                }
            }
        }

        IEnumerator DrawBeam(Vector3 start, Vector3 end)
        {
            if (beamLineRenderers == null || beamLineRenderers.Length == 0)
                yield break;

            var cameraUp = _mainCamera.transform.up;
            var totalHeight = (numberOfBeams - 1) * beamVerticalSpacing;
            var startOffset = totalHeight / 2f;

            // Draw each beam with vertical offset
            for (var i = 0; i < numberOfBeams && i < beamLineRenderers.Length; i++)
            {
                var beam = beamLineRenderers[i];
                if (beam == null)
                    continue;

                var verticalOffset = (i * beamVerticalSpacing - startOffset) * cameraUp;

                beam.enabled = true;
                beam.SetPosition(0, start + verticalOffset);
                beam.SetPosition(1, end + verticalOffset);
            }

            yield return new WaitForSeconds(beamDuration);

            // Disable all beams
            for (var i = 0; i < numberOfBeams && i < beamLineRenderers.Length; i++)
                if (beamLineRenderers[i] != null)
                    beamLineRenderers[i].enabled = false;
        }

        void PlayMuzzleFlash()
        {
            if (_muzzleParticles == null || _muzzleParticles.Length == 0)
                return;

            foreach (var ps in _muzzleParticles)
                if (ps != null)
                    ps.Play();
        }

        // Public method to set ability data (called during equip)
        public void SetAbilityData(FirstPersonPlayer.ScriptableObjects.BioticAbility.BioticAbility abilityData)
        {
            _currentAbilityData = abilityData;
        }
    }
}
