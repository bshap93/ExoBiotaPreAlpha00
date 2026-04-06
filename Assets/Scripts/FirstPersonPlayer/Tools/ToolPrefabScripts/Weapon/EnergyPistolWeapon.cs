using System.Collections;
using DG.Tweening;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Interactable;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using FirstPersonPlayer.Tools.ItemObjectTypes.ToolHelper;
using Helpers.AnimancerHelper;
using Helpers.Events;
using Helpers.Events.Combat;
using Manager;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Weapon
{
    public class EnergyPistolWeapon : RangedToolPrefab
    {
        [Header("Pistol Components")] [SerializeField]
        GameObject physicalRoot;
        [SerializeField] GameObject slider;
        [SerializeField] GameObject frontEmitter;
        [SerializeField] GameObject cell;
        [SerializeField] GameObject trigger;

        [Header("Shooting Settings")] [SerializeField]
        float cooldownTime = 0.5f;
        [SerializeField] float range = 50f;
        [SerializeField] LayerMask hitMask = ~0;

        [Header("Combat Settings")] [SerializeField]
        PlayerToolAttackProfile attackProfile;
        [SerializeField] bool requiresEnergy = true;

        [Header("Visual Effects")] [SerializeField]
        Transform muzzlePosition;
        [SerializeField] GameObject muzzleFlashPrefab;
        [SerializeField] GameObject hitSparksPrefab;
        [SerializeField] GameObject missSparksPrefab;
        [FormerlySerializedAs("initialPistolMode")] [SerializeField]
        EnergyGunMode initialGunMode;

        [Header("Multi-Beam Settings")] [Tooltip("Number of beams to render (2 or 3 recommended)")] [SerializeField]
        int numberOfBeams = 3;
        [Tooltip("Vertical spacing between beams")] [SerializeField]
        float beamVerticalSpacing = 0.04f;
        [SerializeField] float beamVerticalOffset = -0.04f;


        [SerializeField] LineRenderer[] beamLineRenderers;
        [SerializeField] float beamWidth = 0.03f;
        [SerializeField] float beamDuration = 0.1f;
        [SerializeField] Color beamColor = Color.cyan;


        [Header("Feedbacks")] [SerializeField] MMFeedbacks shootFeedbacks;
        [FormerlySerializedAs("hitFeedbacks")] [SerializeField]
        MMFeedbacks nonLocalHitFeedbacks;
        [SerializeField] MMFeedbacks missFeedbacks;
        [SerializeField] MMFeedbacks outOfAmmoFeedbacks;

        [Header("Scriptable Object Reference")] [SerializeField]
        PistolToolObject pistolToolObject;
        [SerializeField] float delaySlideAnimation;

        EnergyGunMode _currentGunMode;

        Vector3 _initialLocalPos;
        EnergyGunMode _lastGunMode;
        GameObject _muzzleFlashInstance;
        ParticleSystem[] _muzzleParticles;
        bool _readyToFire = true;
        float _timeSinceLastUse;

        int AmmoUnitsPerHeavyShot => attackProfile.heavyAttack.ammoUnitCostPerAttack;

        int AmmoUnitsPerBasicShot => attackProfile.basicAttack.ammoUnitCostPerAttack;

        int AmmoUnitsPerBasicStunShot => attackProfile.basicStunAttack.ammoUnitCostPerAttack;

        int AmmoUnitsPerHeavyStunShot => attackProfile.heavyStunAttack.ammoUnitCostPerAttack;

        void Awake()
        {
            _initialLocalPos = physicalRoot.transform.localPosition;
            AnimController = FindFirstObjectByType<AnimancerArmController>();

            // Setup multiple beam renderers
            SetupBeamRenderers();
            // Setup persistent muzzle flash (Hovl style)
            if (muzzleFlashPrefab != null && muzzlePosition != null)
            {
                _muzzleFlashInstance = Instantiate(muzzleFlashPrefab, muzzlePosition.position, muzzlePosition.rotation);
                _muzzleFlashInstance.transform.SetParent(muzzlePosition);
                _muzzleParticles = _muzzleFlashInstance.GetComponentsInChildren<ParticleSystem>();

                // Stop all particles initially
                foreach (var ps in _muzzleParticles)
                    if (ps.isPlaying)
                        ps.Stop();
            }

            _currentGunMode = initialGunMode;

            // Setup persistent muzzle flash (Hovl style)
            if (muzzleFlashPrefab != null && muzzlePosition != null)
            {
                _muzzleFlashInstance = Instantiate(muzzleFlashPrefab, muzzlePosition.position, muzzlePosition.rotation);
                _muzzleFlashInstance.transform.SetParent(muzzlePosition);
                _muzzleParticles = _muzzleFlashInstance.GetComponentsInChildren<ParticleSystem>();

                // Stop all particles initially
                foreach (var ps in _muzzleParticles)
                    if (ps.isPlaying)
                        ps.Stop();
            }
        }
        void Update()
        {
            if (_timeSinceLastUse < cooldownTime)
                _timeSinceLastUse += Time.deltaTime;
            else
                _readyToFire = true;
        }

        void OnDestroy()
        {
            // Clean up persistent muzzle flash
            if (_muzzleFlashInstance != null) Destroy(_muzzleFlashInstance);
        }

        public override void Use()
        {
            PerformToolAction(HitType.Normal);
        }
        public override void Unequip()
        {
            // Remember what we were using so Equip() can restore it
            _lastGunMode = _currentGunMode == EnergyGunMode.None ? initialGunMode : _currentGunMode;
            EnergyGunStateEvent.Trigger(
                AmmoEvent.EventDirection.Inbound,
                EnergyGunMode.None, EnergyGunStateEvent.GunStateEventType.UnequippedGun,
                AmmoType.MagniumEnergyAmmoUnits);
        }
        public override void Equip()
        {
            EnergyGunStateEvent.Trigger(
                AmmoEvent.EventDirection.Inbound,
                _lastGunMode,
                EnergyGunStateEvent.GunStateEventType.EquippedGun,
                AmmoType.MagniumEnergyAmmoUnits);
        }


        void SetupBeamRenderers()
        {
            if (beamLineRenderers == null || beamLineRenderers.Length == 0)
            {
                Debug.LogWarning("No beam LineRenderers assigned. Please assign them in the Inspector.");
                return;
            }

            // Ensure we only use the specified number of beams
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
                    if (i >= numberOfBeams) beam.gameObject.SetActive(false);
                }
            }
        }

        public override void Initialize(PlayerEquipment owner)
        {
            base.Initialize(owner);

            AnimController = owner.animancerShooterHandsController;
            AnimController.currentToolAnimationSet = pistolToolObject.toolAnimationSet;


            var toolStateManager = ToolsStateManager.Instance;
            if (toolStateManager != null)
                _currentGunMode = toolStateManager.EnergyGunMode;

            EnergyGunStateEvent.Trigger(
                AmmoEvent.EventDirection.Inbound,
                _currentGunMode,
                EnergyGunStateEvent.GunStateEventType.InitializedGunState, AmmoType.MagniumEnergyAmmoUnits);
        }

        public void OnUseStarted()
        {
            if (AnimController != null && AnimController.currentToolAnimationSet != null &&
                AnimController.currentToolAnimationSet.beginUseAnimation != null)
                AnimController.PlayToolUseSequence();
        }

        public override void PerformToolAction(HitType hitType)
        {
            // Not heavy
            if (!_readyToFire) return;

            if (_currentGunMode == EnergyGunMode.Laser)
            {
                // Check energy cost
                if (requiresEnergy && !HasSufficientEnergy(EnergyGunMode.Laser, hitType))
                {
                    AlertEvent.Trigger(
                        AlertReason.NotEnoughAmmo,
                        "You ran out of ammo cartridges.",
                        "Out of Ammo");

                    outOfAmmoFeedbacks?.PlayFeedbacks();

                    return;
                }

                var ammoCost = 0;
                if (hitType == HitType.Heavy)
                    ammoCost = AmmoUnitsPerHeavyShot;
                else if (hitType == HitType.Normal)
                    ammoCost = AmmoUnitsPerBasicShot;


                // Consume energy
                if (requiresEnergy)
                    AmmoEvent.Trigger(
                        AmmoEvent.EventDirection.Inbound,
                        ammoCost,
                        AmmoEvent.AmmoEventType.ConsumedAmmo,
                        AmmoType.MagniumEnergyAmmoUnits
                    );
            }
            else if (_currentGunMode == EnergyGunMode.Stun)
            {
                // Check energy cost
                if (requiresEnergy && !HasSufficientEnergy(EnergyGunMode.Stun, hitType))
                {
                    AlertEvent.Trigger(
                        AlertReason.NotEnoughAmmo,
                        "Not enough energy energy cartridges to fire weapon.",
                        "Insufficient Energy Cartridges");

                    outOfAmmoFeedbacks?.PlayFeedbacks();

                    return;
                }

                var ammoCost = 0;
                if (hitType == HitType.Heavy)
                    ammoCost = AmmoUnitsPerHeavyStunShot;
                else if (hitType == HitType.Normal)
                    ammoCost = AmmoUnitsPerBasicStunShot;

                // Consume energy
                if (requiresEnergy)
                    AmmoEvent.Trigger(
                        AmmoEvent.EventDirection.Inbound,
                        ammoCost,
                        AmmoEvent.AmmoEventType.ConsumedAmmo,
                        AmmoType.MagniumEnergyAmmoUnits
                    );
            }


            // Visual and audio feedback
            if (AnimController.IsInAimState())
                AnimController.PlayAimShot();
            else
                AnimController.PlayNonAimShot();

            // AnimateRecoil();
            StartCoroutine(AnimateSlideOutAndBack());
            // AnimateSlideOutAndBack();
            AnimateFrontEmitterOutAndBack();
            OnUseStarted();
            shootFeedbacks?.PlayFeedbacks();

            // Play muzzle flash particles (Hovl style)
            PlayMuzzleFlash();

            // Apply hit after short delay for animation sync
            StartCoroutine(ApplyHitAfterDelay(0.05f));

            _readyToFire = false;
            _timeSinceLastUse = 0f;
        }

        void PlayMuzzleFlash()
        {
            if (_muzzleParticles == null || _muzzleParticles.Length == 0) return;

            foreach (var ps in _muzzleParticles)
                if (ps != null)
                    ps.Play();
        }

        IEnumerator ApplyHitAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ApplyHit();
        }

        public IEnumerator AnimateSlideOutAndBack()
        {
            if (slider == null) yield break;

            // Wait N seconds to sync with firing animation
            yield return new WaitForSeconds(delaySlideAnimation);

            var originalPos = slider.transform.localPosition;
            var slideOutPos = originalPos + new Vector3(0, 0, 0.2f);

            slider.transform.DOKill();
            slider.transform.localPosition = originalPos;
            slider.transform.DOLocalMove(slideOutPos, 0.1f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        void AnimateFrontEmitterOutAndBack()
        {
            if (frontEmitter == null) return;

            var originalPos = frontEmitter.transform.localPosition;
            var slideOutPos = originalPos + new Vector3(0, 0, -0.2f);

            frontEmitter.transform.DOKill();
            frontEmitter.transform.localPosition = originalPos;
            frontEmitter.transform.DOLocalMove(slideOutPos, 0.2f).SetLoops(2, LoopType.Yoyo).SetEase(Ease.InOutSine);
        }

        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return pistolToolObject.defaultReticle;
        }


        public override void ApplyHit()
        {
            if (!mainCamera) mainCamera = Camera.main;
            if (!mainCamera) return;

            // Calculate spread and apply it
            var spreadAngle = CalculateSpreadAngle();
            var baseDirection = mainCamera.transform.forward;
            var spreadDirection = ApplySpread(baseDirection, spreadAngle);
            var ray = new Ray(mainCamera.transform.position, spreadDirection);


            // Debug visualization
            if (debugAccuracy)
            {
                Debug.DrawRay(mainCamera.transform.position, baseDirection * range, Color.green, 1f);
                Debug.DrawRay(mainCamera.transform.position, spreadDirection * range, Color.red, 1f);
            }

            var didHit = Physics.Raycast(ray, out var hit, range, hitMask, QueryTriggerInteraction.Ignore);
            var endPoint = didHit ? hit.point : ray.GetPoint(range);

            // Draw multiple energy beams
            StartCoroutine(DrawMultipleBeams(muzzlePosition.position, endPoint));

            if (didHit)
                ProcessHit(hit);
            else
                missFeedbacks?.PlayFeedbacks();
        }

        IEnumerator DrawMultipleBeams(Vector3 start, Vector3 end)
        {
            if (beamLineRenderers == null || beamLineRenderers.Length == 0) yield break;

            // Calculate the camera's up vector for vertical offset
            var cameraUp = mainCamera.transform.up;

            // Calculate vertical offset for centering the beams
            var totalHeight = (numberOfBeams - 1) * beamVerticalSpacing;
            var startOffset = totalHeight / 2f;

            // Apply additional downward offset to entire beam array
            var baseOffset = beamVerticalOffset * cameraUp;

            // Draw each beam with vertical offset
            for (var i = 0; i < numberOfBeams && i < beamLineRenderers.Length; i++)
            {
                var beam = beamLineRenderers[i];
                if (beam == null) continue;

                // Calculate vertical offset for this beam
                var verticalOffset = (i * beamVerticalSpacing - startOffset) * cameraUp + baseOffset;

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

                    // Apply damage
                    if (_currentGunMode == EnergyGunMode.Laser)
                    {
                        var attack = attackProfile?.basicAttack;
                        if (attack != null) creatureController.ProcessAttackDamage(attack, hit.point);
                    }
                    else if (_currentGunMode == EnergyGunMode.Stun)
                    {
                        var attack = attackProfile?.basicStunAttack;
                        if (attack != null) creatureController.ProcessAttackDamage(attack, hit.point);
                    }


                    nonLocalHitFeedbacks?.PlayFeedbacks();
                    Debug.Log($"Energy pistol hit enemy: {creatureController.name}");
                }
            }
            // Hit breakable object
            else if (go.TryGetComponent<IBreakable>(out var breakable))
            {
                if (_currentGunMode == EnergyGunMode.Laser) breakable.ApplyHit(1, hit.point, hit.normal);
                SpawnHitFX(hitSparksPrefab, hit.point, hit.normal);
                nonLocalHitFeedbacks?.PlayFeedbacks();
            }
            // Hit generic surface
            else
            {
                SpawnHitFX(hitSparksPrefab, hit.point, hit.normal);
                nonLocalHitFeedbacks?.PlayFeedbacks();
            }
        }


        bool HasSufficientEnergy(EnergyGunMode gunMode, HitType hitType)
        {
            var toolStateManager = ToolsStateManager.Instance;
            if (toolStateManager == null) return false;

            var currentAmmoUnits = toolStateManager.MagniumEnergyUnitsAvailable;
            switch (gunMode)
            {
                case EnergyGunMode.Laser:
                    if (hitType == HitType.Heavy)
                        return currentAmmoUnits >= AmmoUnitsPerHeavyShot;

                    if (hitType == HitType.Normal)
                        return currentAmmoUnits >= AmmoUnitsPerBasicShot;

                    return false;

                case EnergyGunMode.Stun:
                    if (hitType == HitType.Heavy)
                        return currentAmmoUnits >= AmmoUnitsPerHeavyStunShot;

                    if (hitType == HitType.Normal)
                        return currentAmmoUnits >= AmmoUnitsPerBasicStunShot;

                    return false;

                default:
                    return false;
            }
        }
    }
}
