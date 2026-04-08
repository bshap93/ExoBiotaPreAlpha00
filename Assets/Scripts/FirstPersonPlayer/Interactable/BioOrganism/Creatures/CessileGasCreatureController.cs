using System;
using System.Collections;
using Animancer;
using DG.Tweening;
using FirstPersonPlayer.Combat.AINPC;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.NPCs;
using Helpers.Events.Status;
using Manager.StateManager;
using MoreMountains.Feedbacks;
using OccaSoftware.ResponsiveSmokes.Runtime;
using SharedUI.Interface;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Interactable.BioOrganism.Creatures
{
    [RequireComponent(typeof(AssignPlayerToBT))]
    [RequireComponent(typeof(CessileCreatureBBSync))]
    public class CessileGasCreatureController : CreatureController, IBillboardable,
        IHoverable
    {
        [Header("Scene References")] [Tooltip("InteractiveSmoke child that renders & times the gas cloud.")]
        public InteractiveSmoke smoke;
        public Collider gasAreaCollider;

        [SerializeField] float releaseFeedbackDelay = 1.2f;
        [SerializeField] GameObject undestroyedModelObject;
        [SerializeField] GameObject destroyedModelObject;

        [Header("Main Settings")] public float detectionRadius;
        [FormerlySerializedAs("lethalRadius")] public float contaminateRadius;

        [Header("Contamination")] public float contaminationOnEnter = 6f;
        public float contaminationPerSecond = 2f;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks releaseFeedbacks;
        [SerializeField] MMFeedbacks startReleaseFeedbacks;


        [Header("Death Effects")] [SerializeField]
        GameObject deathParticlesPrefab;


        bool _hasAppliedBurstContamination; // NEW: track if burst contamination was applied

        Tween _hitTween;

        Transform _playerTransform;


        protected SceneObjectData Data;
        protected AnimancerState DeathState;
        protected AnimancerState PuffGasState;

        public bool IsPuffingGas { get; private set; }
        public bool HazardActive { get; set; }

        protected override void Awake()
        {
            base.Awake();
            // keep smoke off until released
            if (smoke) smoke.gameObject.SetActive(false);
        }

        void Update()
        {
            if (!IsActivated) return;
            // NEW: Apply contamination while hazard is active and player is in gas area
            if (HazardActive && _playerTransform && gasAreaCollider)
            {
                var playerInGasArea = gasAreaCollider.bounds.Contains(_playerTransform.position);

                if (playerInGasArea)
                {
                    // Apply burst contamination on first entry
                    if (!_hasAppliedBurstContamination)
                    {
                        PlayerStatsEvent.Trigger(
                            PlayerStatsEvent.PlayerStat.CurrentContamination,
                            PlayerStatsEvent.PlayerStatChangeType.Increase,
                            contaminationOnEnter);

                        _hasAppliedBurstContamination = true;
                    }

                    // Apply continuous contamination
                    PlayerStatsEvent.Trigger(
                        PlayerStatsEvent.PlayerStat.CurrentContamination,
                        PlayerStatsEvent.PlayerStatChangeType.Increase,
                        contaminationPerSecond * Time.deltaTime);
                }
            }


            if (HazardActive && (!smoke || !smoke.IsAlive()))
            {
                HazardActive = false;
                _hasAppliedBurstContamination = false;
            }


            if (IsPuffingGas) return;

            if (!IdleState.IsPlaying) animancerComponent.Play(IdleState);

            if (currentHealth <= 0 && !isDead)
            {
                isDead = true;
                if (creatureType.animationSet.deathAnimation)
                {
                    DeathState = animancerComponent.Play(creatureType.animationSet.deathAnimation);
                    DeathState.Events(this).OnEnd = () => { };
                }

                OnDeath();
            }
        }
        // NEW: Track player entering/exiting trigger
        void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer")) return;
            _playerTransform = other.transform;
        }

        void OnTriggerExit(Collider other)
        {
            if (!other.CompareTag("Player") && !other.CompareTag("FirstPersonPlayer")) return;
            if (other.transform == _playerTransform)
            {
                _playerTransform = null;
                _hasAppliedBurstContamination = false;
            }
        }
        public string GetName()
        {
            return creatureType.creatureName;
        }
        public Sprite GetIcon()
        {
            return creatureType.creatureIcon;
        }
        public string ShortBlurb()
        {
            return creatureType.shortDescription;
        }
        public Sprite GetActionIcon()
        {
            return creatureType.actionIcon;
        }
        public string GetActionText()
        {
            return "Avoid or Destroy";
        }

        public bool OnHoverStart(GameObject go)
        {
            Data = new SceneObjectData(
                creatureType.creatureName,
                creatureType.creatureIcon,
                creatureType.shortDescription,
                creatureType.actionIcon,
                GetActionText()
            );

            Data.Id = uniqueID;

            BillboardEvent.Trigger(Data, BillboardEventType.Show);

            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            if (Data == null) Data = SceneObjectData.Empty();
            BillboardEvent.Trigger(Data, BillboardEventType.Hide);
            return true;
        }
        public override void PlayHitAnimation(AnimationClip value)
        {
            // none for right now
        }
        public override void OnDeath()
        {
            if (initialCreatureInitializationState !=
                CreatureStateManager.CreatureInitializationState.ShouldRespawnAndReinitialize)
                CreatureInitializationStateEvent.Trigger(
                    CreatureStateEventType.SetNewCreatureState, uniqueID,
                    CreatureStateManager.CreatureInitializationState.ShouldBeDestroyed);

            EnemyDamageEvent.Trigger(
                uniqueID,
                0f, currentHealth, creatureType.maxHealth, DamageEventType.Death, creatureType.creatureName,
                DamageType.None);

            blackboard.SetVariableValue("isDead", true);
            Debug.Log(creatureType.creatureName + " has died.");

            if (deathFeedbacks != null) deathFeedbacks.enabled = true;
            deathFeedbacks?.PlayFeedbacks();
            // if (deathParticlesPrefab != null)
            //     Instantiate(deathParticlesPrefab, transform.position + Vector3.up * 0.5f, Quaternion.identity);

            undestroyedModelObject.SetActive(false);
            destroyedModelObject.SetActive(true);
        }
        public void StartPuffGas()
        {
            if (IsPuffingGas) return;
            if (HazardActive) return; // Don't puff while smoke is still alive

            IsPuffingGas = true;


            if (attackInstances.Length < 1 ||
                attackInstances[0].attackAnimationClip == null)
                throw new Exception("CessileGasCreatureController: No attack animation clip assigned.");

            PuffGasState = animancerComponent.Play(attackInstances[0].attackAnimationClip);

            startReleaseFeedbacks?.PlayFeedbacks();

            StartCoroutine(PlayReleaseGasAfterDelay(releaseFeedbackDelay));

            PuffGasState.Events(this).OnEnd = () => { FinishPuffGas(); };
        }

        protected override IEnumerator InitializeAfterCreatureStateManager()
        {
            yield return null;

            var creatureStateManager = CreatureStateManager.Instance;
            if (creatureStateManager != null)
            {
                var creatureState = creatureStateManager.GetCreatureState(uniqueID);
                if (creatureState == CreatureStateManager.CreatureInitializationState.None)
                    creatureState = initialCreatureInitializationState;

                if (creatureState == CreatureStateManager.CreatureInitializationState.HasBeenInitialized)
                {
                    ReLoadCreatureStateData();
                }
                else if (creatureState == CreatureStateManager.CreatureInitializationState.ShouldBeDestroyed)
                {
                    isDead = true;
                    blackboard.SetVariableValue("wasAlreadyDead", true);
                    blackboard.SetVariableValue("isDead", true);

                    destroyedModelObject.SetActive(true);
                    undestroyedModelObject.SetActive(false);
                }
            }
        }

        IEnumerator PlayReleaseGasAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ReleaseGas();
            releaseFeedbacks?.PlayFeedbacks();
        }

        void ReleaseGas()
        {
            // _gasReleased = true;


            if (smoke)
            {
                smoke.gameObject.SetActive(true);
                smoke.Smoke(); // starts fade-in → active lifetime → fade-out → Cleanup
                StartCoroutine(TrackSmokeLife()); // flips _hazardActive true while smoke.IsAlive()
            }
            else
            {
                // Failsafe: if no smoke reference, still mark hazard active for a short window
                HazardActive = true;
                StartCoroutine(StopHazardNextFrame());
            }
        }

        IEnumerator StopHazardNextFrame()
        {
            yield return null;
            HazardActive = false;
        }

        IEnumerator TrackSmokeLife()
        {
            // Wait one frame so InteractiveSmoke.Init() runs
            yield return null;

            // Consider the cloud hazardous as long as InteractiveSmoke reports alive.
            // (Init sets isAlive=true; Cleanup sets isAlive=false). :contentReference[oaicite:1]{index=1}
            HazardActive = smoke && smoke.IsAlive();
            while (smoke && smoke.IsAlive())
                yield return null;

            HazardActive = false;
            // _playerInsideInner = false;
        }

        public void FinishPuffGas()
        {
            IsPuffingGas = false;
        }
    }
}
