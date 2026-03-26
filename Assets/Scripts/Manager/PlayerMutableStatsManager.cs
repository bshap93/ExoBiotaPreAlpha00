using System;
using System.Collections;
using System.Collections.Generic;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.Tools;
using FirstPersonPlayer.UI.Stats;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.NPCs;
using Helpers.Events.Progression;
using Helpers.Events.Status;
using Helpers.Interfaces;
using Manager.DialogueScene;
using Manager.FirstPerson;
using Manager.ProgressionMangers;
using Manager.Status.Scriptable;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using OWPData.ScriptableObjects;
using SharedUI.Alert;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Manager
{
    public class PlayerMutableStatsManager : MonoBehaviour, ICoreGameService, MMEventListener<PlayerStatsEvent>,
        MMEventListener<StaminaAffectorEvent>, MMEventListener<MyUIEvent>, MMEventListener<InGameTimeActionEvent>,
        MMEventListener<NPCAttackEvent>, MMEventListener<LoadedManagerEvent>,
        MMEventListener<NotifyAttributesNewlySetEvent>
    {
        const string KeyBaseMaxHealth = "PlayerBaseMaxHealth";
        const string KeyBaseMaxStamina = "PlayerBaseMaxStamina";
        const string KeyBaseMaxContamination = "PlayerBaseMaxContamination";
        const string KeyBaseMaxVision = "PlayerBaseMaxVision";

        const string KeyCurrentHealth = "PlayerCurrentHealth";
        const string KeyCurrentStamina = "PlayerCurrentStamina";
        const string KeyCurrentVision = "PlayerCurrentVision";
        const string KeyCurrentContamination = "PlayerCurrentContamination";

        const string KeyContaminationPointsPerCU = "PlayerContaminationPointsPerCU";

        const string KeyCurrentMaxHealth = "PlayerCurrentMaxHealth";
        const string KeyCurrentMaxStamina = "PlayerCurrentMaxStamina";
        const string KeyCurrentMaxContamination = "PlayerCurrentMaxContamination";
        const string KeyCurrentMaxVision = "PlayerCurrentMaxVision";

        const string KeyIsPlayerExoBiote = "IsPlayerExoBiote";

        [Header("References")] [SerializeField]
        bool autoSave; // checkpoint-only by default
        [SerializeField] LevelingManager levelingManager;

        [SerializeField] PlayerStatsSheet defaultPlayerStatsSheet;
        [SerializeField] PlayerStatsBars playerStatsBars;
        [SerializeField] AttributesManager attributesManager;

        [Header("Feedbacks")] [SerializeField] MMFeedbacks thresholdChangesFeedbacks;
        [SerializeField] MMFeedbacks restoreHealthFeedbacks;

        [SerializeField] MMFeedbacks restoreStaminaFeedbacks;

        [SerializeField] MMFeedbacks loseHealthFeedbacks;
        [SerializeField] MMFeedbacks loseStaminaFeedbacks;
        [SerializeField] MMFeedbacks drainHealthOverTimeFeedbacks;
        // Hit feedbacks
        [SerializeField] MMFeedbacks hitWithMeleeFeedbacks;
        [SerializeField] MMFeedbacks hitWithProjectileFeedbacks;
        [SerializeField] MMFeedbacks hitWithContaminantAOE;
        [SerializeField] MMFeedbacks hitWithCriticalMeleeFeedbacks;


        [SerializeField] float healthDrainRateWhenMaxContaminated = 0.5f;

        [Header("Modal Settings")] public List<AlertUIController.ModalArgs> modalArgs;

        [FormerlySerializedAs("StatBasedDialogueNodes")]
        public List<StatBasedDialogueNode> statBasedDialogueNodes = new();

        readonly float _preArrivalFeedbackInterval = 1.2f;

        // bool _contaminationMaxed;
        Coroutine _decreaseContaminationRoutine;


        [Header("Current Max Stat")] bool _dirty;

        bool _inGameTimePaused;

        Coroutine _restoreStaminaRoutine;
        // int _lastCU = -1;

        string _savePath;
        float _weaponModifiedStaminaRestoreRate;

        // public StaminaRestoreRateMultiplierByAttribute[] RestoreRateMultipliersByAttribute;
        public StaminaRestoreRateMultiplierByTool[] RestoreRateMultipliersByTool;

        public float BaseStaminaRestoreRate { get; private set; }

        // bool _sprinting;

        public bool IsPlayerExoBiote { get; private set; }
        public float BaseMaxContamination { get; private set; }

        public float CurrentContamination { get; private set; }

        public float BaseMaxHealth { get; private set; }
        public float BaseMaxStamina { get; private set; }
        public float BaseMaxVision { get; private set; }
        public float ContaminationPointsPerCU { get; private set; }
        public float CurrentHealth { get; private set; }
        public float CurrentVision { get; private set; }
        public float CurrentMaxContamination { get; private set; }
        public float CurrentMaxHealth { get; private set; }
        // public float CurrentMaxStamina { get; private set; }
        public float CurrentMaxVision { get; private set; }


        public float CurrentStamina { get; private set; }

        public float CurrentStaminaRestoreRate { get; private set; }


        public static PlayerMutableStatsManager Instance { get; private set; }
        // public int CurrentCU => Mathf.FloorToInt(CurrentContamination / ContaminationPointsPerCU);
        // public float CurrentCUFraction => CurrentContamination % ContaminationPointsPerCU / ContaminationPointsPerCU;
        // public float SprintStaminaDrainPerSecond => defaultPlayerStatsSheet.sprintStaminaDrainPerSecond;


        void Awake()
        {
            if (Instance == null)
                Instance = this;

            else
                Destroy(gameObject);
        }
        void Start()
        {
            _savePath = GetSaveFilePath();
            if (!ES3.FileExists(_savePath))
            {
                Debug.Log("[PlayerSaveManager] No save file found, forcing initial save...");
                Reset();
            }

            Load();
        }

        void Update()
        {
            if (CurrentStamina < BaseMaxStamina)
            {
                if (_restoreStaminaRoutine == null && PlayerEquipment.Instance != null &&
                    PlayerEquipment.Instance.CurrentToolSo != null)
                {
                    var agility = attributesManager.Agility;
                    var toolMultiplier = PlayerEquipment.Instance.CurrentToolSo.staminaRestoreRateMultiplier;

                    CurrentStaminaRestoreRate =
                        levelingManager.GetBaseStaminaRestoreRateForAgility(agility) * toolMultiplier;

                    StaminaRestoreRateEvent.Trigger(CurrentStaminaRestoreRate);

                    _restoreStaminaRoutine = StartCoroutine(
                        RestoreStaminaOverTime(CurrentStaminaRestoreRate)
                    );
                }
                else if (CurrentStamina > BaseMaxStamina - _preArrivalFeedbackInterval)
                {
                    CombatFeedbacksEvent.Trigger(CombatFeedbackType.StaminaIsFullAgain);
                }
            }
            else
            {
                if (_restoreStaminaRoutine != null)
                {
                    StopCoroutine(_restoreStaminaRoutine);
                    _restoreStaminaRoutine = null;
                }
            }

            if (!IsPlayerExoBiote && CurrentContamination > 0)
            {
                if (_decreaseContaminationRoutine == null)
                    _decreaseContaminationRoutine = StartCoroutine(
                        DecreaseContaminationOverTime(defaultPlayerStatsSheet.baseContaminationDecreasePerSecond)
                    );
            }
            else
            {
                if (_decreaseContaminationRoutine != null)
                {
                    StopCoroutine(_decreaseContaminationRoutine);
                    _decreaseContaminationRoutine = null;
                }
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<PlayerStatsEvent>();
            this.MMEventStartListening<StaminaAffectorEvent>();
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<InGameTimeActionEvent>();
            this.MMEventStartListening<NPCAttackEvent>();
            this.MMEventStartListening<LoadedManagerEvent>();
            this.MMEventStartListening<InGameTimeActionEvent>();
            this.MMEventStopListening<NotifyAttributesNewlySetEvent>();
        }


        void OnDisable()
        {
            this.MMEventStopListening<PlayerStatsEvent>();
            this.MMEventStopListening<StaminaAffectorEvent>();
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<InGameTimeActionEvent>();
            this.MMEventStopListening<NPCAttackEvent>();
            this.MMEventStopListening<LoadedManagerEvent>();
            this.MMEventStopListening<InGameTimeActionEvent>();
            this.MMEventStopListening<NotifyAttributesNewlySetEvent>();
        }


        public void MarkDirty()
        {
            _dirty = true;
        }

        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.PlayerSave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty)
            {
                Save();
                _dirty = false;
            }
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void Save()
        {
            var savePath = GetSaveFilePath();

            ES3.Save(KeyBaseMaxHealth, BaseMaxHealth, savePath);
            ES3.Save(KeyBaseMaxStamina, BaseMaxStamina, savePath);
            ES3.Save(KeyBaseMaxContamination, BaseMaxContamination, savePath);
            ES3.Save(KeyBaseMaxVision, BaseMaxVision, savePath);

            ES3.Save(KeyCurrentHealth, CurrentHealth, savePath);
            ES3.Save(KeyCurrentStamina, CurrentStamina, savePath);
            ES3.Save(KeyCurrentVision, CurrentVision, savePath);
            ES3.Save(KeyCurrentContamination, CurrentContamination, savePath);

            ES3.Save(KeyContaminationPointsPerCU, ContaminationPointsPerCU, savePath);

            ES3.Save(KeyCurrentMaxHealth, CurrentMaxHealth, savePath);
            // ES3.Save(KeyCurrentMaxStamina, CurrentMaxStamina, savePath);
            ES3.Save(KeyCurrentMaxContamination, CurrentMaxContamination, savePath);
            ES3.Save(KeyCurrentMaxVision, CurrentMaxVision, savePath);

            ES3.Save(KeyIsPlayerExoBiote, IsPlayerExoBiote, savePath);
        }


        public void Reset()
        {
            if (defaultPlayerStatsSheet == null)
            {
                Debug.LogError("[PlayerStatsManager] No default player stats sheet assigned!");
                return;
            }

            BaseMaxHealth = defaultPlayerStatsSheet.baseMaxHealth;
            BaseMaxStamina = defaultPlayerStatsSheet.baseMaxStamina;
            BaseMaxContamination = defaultPlayerStatsSheet.baseMaxContamination;
            BaseMaxVision = defaultPlayerStatsSheet.baseMaxVision;

            CurrentHealth = defaultPlayerStatsSheet.currentHealth;
            CurrentStamina = defaultPlayerStatsSheet.currentStamina;
            CurrentContamination = defaultPlayerStatsSheet.currentContamination;
            CurrentVision = defaultPlayerStatsSheet.currentVision;

            ContaminationPointsPerCU = defaultPlayerStatsSheet.contaminationPointsPerCU;

            CurrentMaxHealth = defaultPlayerStatsSheet.currentMaxHealth;
            // CurrentMaxStamina = defaultPlayerStatsSheet.currentMaxStamina;
            CurrentMaxContamination = defaultPlayerStatsSheet.currentMaxContamination;
            CurrentMaxVision = defaultPlayerStatsSheet.currentMaxVision;
            IsPlayerExoBiote = defaultPlayerStatsSheet.isPlayerExoBiote;
            playerStatsBars?.UpdateAllBars();
            MarkDirty();
            ConditionalSave();
            // NotifyCUChanges();
            PlayerStatsSyncEvent.Trigger();
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty)
            {
                Save();
                _dirty = false;
            }
        }

        public void Load()
        {
            var savePath = GetSaveFilePath();
            // if no save file, return
            if (!ES3.FileExists(savePath)) return;

            if (ES3.KeyExists(KeyBaseMaxHealth, savePath))
                BaseMaxHealth = ES3.Load<float>(KeyBaseMaxHealth, savePath);
            else
                BaseMaxHealth = defaultPlayerStatsSheet.baseMaxHealth;

            if (ES3.KeyExists(KeyBaseMaxStamina, savePath))
                BaseMaxStamina = ES3.Load<float>(KeyBaseMaxStamina, savePath);
            else
                BaseMaxStamina = defaultPlayerStatsSheet.baseMaxStamina;

            if (ES3.KeyExists(KeyBaseMaxContamination, savePath))
                BaseMaxContamination = ES3.Load<float>(KeyBaseMaxContamination, savePath);
            else
                BaseMaxContamination = defaultPlayerStatsSheet.baseMaxContamination;

            if (ES3.KeyExists(KeyBaseMaxVision, savePath))
                BaseMaxVision = ES3.Load<float>(KeyBaseMaxVision, savePath);
            else
                BaseMaxVision = defaultPlayerStatsSheet.baseMaxVision;

            if (ES3.KeyExists(KeyCurrentHealth, savePath))
                CurrentHealth = ES3.Load<float>(KeyCurrentHealth, savePath);
            else
                CurrentHealth = defaultPlayerStatsSheet.currentHealth;

            if (ES3.KeyExists(KeyCurrentStamina, savePath))
                CurrentStamina = ES3.Load<float>(KeyCurrentStamina, savePath);
            else
                CurrentStamina = defaultPlayerStatsSheet.currentStamina;

            if (ES3.KeyExists(KeyCurrentVision, savePath))
                CurrentVision = ES3.Load<float>(KeyCurrentVision, savePath);
            else
                CurrentVision = defaultPlayerStatsSheet.currentVision;

            if (ES3.KeyExists(KeyCurrentContamination, savePath))
                CurrentContamination = ES3.Load<float>(KeyCurrentContamination, savePath);
            else
                CurrentContamination = defaultPlayerStatsSheet.currentContamination;

            if (ES3.KeyExists(KeyContaminationPointsPerCU, savePath))
                ContaminationPointsPerCU = ES3.Load<float>(KeyContaminationPointsPerCU, savePath);
            else
                ContaminationPointsPerCU = defaultPlayerStatsSheet.contaminationPointsPerCU;

            if (ES3.KeyExists(KeyCurrentMaxHealth, savePath))
                CurrentMaxHealth = ES3.Load<float>(KeyCurrentMaxHealth, savePath);
            else
                CurrentMaxHealth = defaultPlayerStatsSheet.currentMaxHealth;

            // if (ES3.KeyExists(KeyCurrentMaxStamina, savePath))
            //     CurrentMaxStamina = ES3.Load<float>(KeyCurrentMaxStamina, savePath);
            // else
            //     CurrentMaxStamina = defaultPlayerStatsSheet.currentMaxStamina;

            if (ES3.KeyExists(KeyCurrentMaxContamination, savePath))
                CurrentMaxContamination = ES3.Load<float>(KeyCurrentMaxContamination, savePath);
            else
                CurrentMaxContamination = defaultPlayerStatsSheet.currentMaxContamination;

            if (ES3.KeyExists(KeyCurrentMaxVision, savePath))
                CurrentMaxVision = ES3.Load<float>(KeyCurrentMaxVision, savePath);
            else
                CurrentMaxVision = defaultPlayerStatsSheet.currentMaxVision;

            if (ES3.KeyExists(KeyIsPlayerExoBiote, savePath))
                IsPlayerExoBiote = ES3.Load<bool>(KeyIsPlayerExoBiote, savePath);
            else
                IsPlayerExoBiote = defaultPlayerStatsSheet.isPlayerExoBiote;

            playerStatsBars?.UpdateAllBars();
            // NotifyCUChanges();
            PlayerStatsSyncEvent.Trigger();
        }

        public void OnMMEvent(InGameTimeActionEvent eventType)
        {
            switch (eventType.ActionTypeIG)
            {
                case InGameTimeActionEvent.ActionType.Pause:
                    _inGameTimePaused = true;
                    break;
                case InGameTimeActionEvent.ActionType.Resume:
                case InGameTimeActionEvent.ActionType.StopLapseTime:
                    _inGameTimePaused = false;
                    break;
            }
        }

        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            // var agility = attributesManager.Agility;
            if (levelingManager == null)
            {
                Debug.LogError(
                    "[PlayerMutableStatsManager] No reference to LevelingManager, cannot initialize stamina restore rate!");

                return;
            }

            var agility = attributesManager.Agility;
            if (levelingManager != null)
                BaseStaminaRestoreRate = levelingManager.GetBaseStaminaRestoreRateForAgility(agility);

            var toughness = attributesManager.Toughness;
            if (levelingManager != null)
                BaseMaxHealth = levelingManager.GetBaseMaxHealthForToughness(toughness);

            PlayerStatsSyncEvent.Trigger();
        }
        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiActionType == UIActionType.Open)
                StaminaAffectorEvent.Trigger(
                    StaminaAffectorEventType.StaminaDrainActivityStopped, 0f);
        }

        public void OnMMEvent(NotifyAttributesNewlySetEvent eventType)
        {
            if (eventType.Toughness > 0)
            {
                var toughness = attributesManager.Toughness;
                if (levelingManager != null)
                    BaseMaxHealth = levelingManager.GetBaseMaxHealthForToughness(toughness);

                PlayerStatsSyncEvent.Trigger();
            }
        }

        public void OnMMEvent(NPCAttackEvent eventType)
        {
            switch (eventType.Attack.attackType)
            {
                case NPCAttackType.Melee:
                    CurrentHealth -= ProcessAttackDamage(eventType.Attack);
                    DieIfDead();
                    break;
                case NPCAttackType.Ranged:
                    CurrentHealth -= ProcessAttackDamage(eventType.Attack);
                    DieIfDead();
                    break;
                case NPCAttackType.ContaminantPOE:
                    CurrentContamination += eventType.Attack.contaminationAmount;
                    break;
            }
        }


        public void OnMMEvent(PlayerStatsEvent e)
        {
            switch (e.StatType)
            {
                case PlayerStatsEvent.PlayerStat.CurrentHealth:
                    if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease)
                    {
                        CurrentHealth -= e.Amount;
                        CurrentHealth -= e.Percent * BaseMaxHealth;
                        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, BaseMaxHealth);
                        DieIfDead();
                    }
                    else if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Increase)
                    {
                        CurrentHealth += e.Amount;
                        CurrentHealth += e.Percent * BaseMaxHealth;
                        CurrentHealth = Mathf.Clamp(CurrentHealth, 0, CurrentMaxHealth);

                        restoreHealthFeedbacks?.PlayFeedbacks();

                        PlayerStatsSyncEvent.Trigger();
                    }

                    break;
                case PlayerStatsEvent.PlayerStat.CurrentMaxContamination:
                    if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Increase)
                    {
                        CurrentMaxContamination += e.Amount;
                        CurrentMaxContamination += e.Percent * BaseMaxContamination;
                        PlayerStatsSyncEvent.Trigger();
                    }

                    break;

                case PlayerStatsEvent.PlayerStat.CurrentContamination:
                    if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease)
                    {
                        CurrentContamination -= e.Amount;
                        CurrentContamination -= e.Percent * CurrentMaxContamination;
                        CurrentContamination = Mathf.Clamp(CurrentContamination, 0, CurrentMaxContamination);

                        // if (_contaminationMaxed)
                        // {
                        //     _contaminationMaxed = false;
                        //     StatsStatusEvent.Trigger(
                        //         false, StatsStatusEvent.StatsStatus.IsMax,
                        //         StatsStatusEvent.StatsStatusType.Contamination);
                        // }
                        // Player has reduced contamination below max, reset flag
                        // StopDrainingHealth();
                        // if (CurrentContamination < CurrentMaxContamination)
                        //     playerStatsBars?.AlertPlayerToStatRisk(
                        //         false, PlayerStatsEvent.PlayerStat.CurrentContamination,
                        //         maxContaminationDialogueNode, "VitalSystems");
                    }
                    else if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Increase)
                    {
                        if (!_inGameTimePaused)
                        {
                            CurrentContamination += e.Amount;
                            CurrentContamination += e.Percent * CurrentMaxContamination;
                            CurrentContamination = Mathf.Clamp(CurrentContamination, 0, CurrentMaxContamination);
                        }


                        // if (CurrentContamination >= CurrentMaxContamination && !_contaminationMaxed)
                        // {
                        //     playerStatsBars?.AlertPlayerToStatRisk(
                        //         true, PlayerStatsEvent.PlayerStat.CurrentContamination, maxContaminationDialogueNode,
                        //         maxContaminationDialogueNode);
                        //
                        //     StatsStatusEvent.Trigger(
                        //         true, StatsStatusEvent.StatsStatus.IsMax,
                        //         StatsStatusEvent.StatsStatusType.Contamination);
                        //
                        //     _contaminationMaxed = true;
                        //
                        //     // StartDrainingHealth();
                        // }
                    }

                    // NotifyCUChanges();
                    PlayerStatsSyncEvent.Trigger();


                    break;
                case PlayerStatsEvent.PlayerStat.CurrentMaxHealth:
                    if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease)
                    {
                        CurrentMaxHealth -= e.Amount;
                        CurrentMaxHealth -= e.Percent * BaseMaxHealth;
                        if (CurrentHealth > CurrentMaxHealth)
                            CurrentHealth = CurrentMaxHealth;

                        PlayerStatsSyncEvent.Trigger();
                    }
                    else if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Increase)
                    {
                        CurrentMaxHealth += e.Amount;
                        CurrentMaxHealth += e.Percent * BaseMaxHealth;
                        PlayerStatsSyncEvent.Trigger();
                    }

                    break;
                case PlayerStatsEvent.PlayerStat.CurrentStamina:
                    if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease)
                    {
                        CurrentStamina -= e.Amount;
                        CurrentStamina -= e.Percent * BaseMaxStamina;
                        CurrentStamina = Mathf.Clamp(CurrentStamina, 0, BaseMaxStamina);

                        PlayerStatsSyncEvent.Trigger();
                    }
                    else if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Increase)
                    {
                        CurrentStamina += e.Amount;
                        CurrentStamina += e.Percent * BaseMaxStamina;
                        // CurrentStamina = Mathf.Clamp(CurrentStamina, 0, BaseMaxStamina);
                        restoreStaminaFeedbacks?.PlayFeedbacks();
                        PlayerStatsSyncEvent.Trigger();
                    }

                    break;

                case PlayerStatsEvent.PlayerStat.CurrentVision:
                    if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Decrease)
                    {
                        CurrentVision -= e.Amount;
                        CurrentVision -= e.Percent * BaseMaxVision;
                        CurrentVision = Mathf.Clamp(CurrentVision, 0, BaseMaxVision);
                    }
                    else if (e.ChangeType == PlayerStatsEvent.PlayerStatChangeType.Increase)
                    {
                        CurrentVision += e.Amount;
                        CurrentVision += e.Percent * BaseMaxVision;
                        CurrentVision = Mathf.Clamp(CurrentVision, 0, BaseMaxVision);
                    }


                    break;
            }
        }
        public void OnMMEvent(StaminaAffectorEvent eventType)
        {
            // if (eventType.EventType == StaminaAffectorEventType.StaminaDrainActivityStarted)
            // {
            //     // Start draining stamina
            //     // _sprinting = true;
            //     StartCoroutine(DrainStaminaOverTime(eventType.ValuePerSecond));
            // }
            // else if (eventType.EventType == StaminaAffectorEventType.StaminaDrainActivityStopped)
            // {
            //     // _sprinting = false;
            //     // Stop draining stamina
            //     StopCoroutine(DrainStaminaOverTime(eventType.ValuePerSecond));
            // }
        }


        void DieIfDead()
        {
            if (CurrentHealth <= 0)
            {
                var deathInfo = new DeathInformation
                {
                    causeOfDeath = PlayerStatsEvent.StatChangeCause.JabbarCreche
                };

                // Trigger player death
                PlayerDeathEvent.Trigger(deathInfo);
            }
        }
        void StopDrainingHealth()
        {
            drainHealthOverTimeFeedbacks?.StopFeedbacks();
            StopCoroutine(DrainHealthOverTime(healthDrainRateWhenMaxContaminated));
        }
        void StartDrainingHealth()
        {
            drainHealthOverTimeFeedbacks?.PlayFeedbacks();
            StartCoroutine(DrainHealthOverTime(healthDrainRateWhenMaxContaminated));
        }

        public StatBasedDialogueNode GetDialogueNodeInfoByStatType(PlayerStatsEvent.PlayerStat statType)
        {
            return statBasedDialogueNodes.Find(node => node.statType == statType);
        }

        // IEnumerator DrainStaminaOverTime(float drainRate)
        // {
        //     while (_sprinting)
        //     {
        //         if (!_inGameTimePaused)
        //         {
        //             CurrentStamina -= drainRate * Time.deltaTime;
        //             CurrentStamina = Mathf.Clamp(CurrentStamina, 0, BaseMaxStamina);
        //             PlayerStatsSyncEvent.Trigger();
        //         }
        //
        //         yield return null;
        //     }
        // }

        IEnumerator RestoreStaminaOverTime(float restoreRate)
        {
            yield return new WaitForSeconds(0.3f);

            while (CurrentStamina < BaseMaxStamina)
            {
                if (!_inGameTimePaused)
                {
                    CurrentStamina += restoreRate * Time.deltaTime;
                    CurrentStamina = Mathf.Clamp(CurrentStamina, 0, BaseMaxStamina);
                    PlayerStatsSyncEvent.Trigger();
                }

                yield return null;
            }

            _restoreStaminaRoutine = null;
        }

        IEnumerator DecreaseContaminationOverTime(float decreaseRate)
        {
            while (CurrentContamination > 0)
            {
                if (!_inGameTimePaused)
                {
                    CurrentContamination -= decreaseRate * Time.deltaTime;
                    CurrentContamination = Mathf.Clamp(CurrentContamination, 0, CurrentMaxContamination);
                    PlayerStatsSyncEvent.Trigger();
                }

                yield return null;
            }
        }

        IEnumerator DrainHealthOverTime(float drainRate)
        {
            while (true)
            {
                if (!_inGameTimePaused)
                {
                    CurrentHealth -= drainRate * Time.deltaTime;
                    CurrentHealth = Mathf.Clamp(CurrentHealth, 0, BaseMaxHealth);

                    PlayerStatsSyncEvent.Trigger();
                }

                yield return null;
            }
        }


        IEnumerator WaitThenTriggerModal(string modalId, float waitSeconds)
        {
            yield return new WaitForSeconds(waitSeconds);
            TriggerModalById(modalId);
        }

        public void TriggerModalById(string modalId)
        {
            // Find modal data by ID (case-insensitive match)
            var args = modalArgs.Find(m => string.Equals(m.ID, modalId, StringComparison.OrdinalIgnoreCase));
            if (args.ID == null)
            {
                Debug.LogWarning($"[ContaminationManager] No ModalArgs found for ID: {modalId}");
                return;
            }

            // Build and trigger the AlertEvent
            AlertEvent.Trigger(
                AlertReason.ContaminationWarning,
                args.description,
                args.title,
                AlertType.ChoiceModal,
                alertImage: args.icon,
                alertIcon: args.icon,
                onConfirm: args.OnConfirm,
                onCancel: args.OnCancel
            );
        }


        public float GetHealthFraction()
        {
            return CurrentHealth / BaseMaxHealth;
        }

        // public bool IsContaminationMaxed()
        // {
        //     _contaminationMaxed = CurrentContamination >= CurrentMaxContamination;
        //     return _contaminationMaxed;
        // }
        float ProcessAttackDamage(EnemyAttack eventTypeAttack)
        {
            var damage = eventTypeAttack.rawDamage; // - (playerToughness * 0.5f);
            damage = Mathf.Max(damage, 0f); // Ensure damage is not negative
            var playerToolSo = PlayerEquipment.InstanceRight.CurrentToolSo;
            if (playerToolSo != null)
            {
                if (PlayerEquipment.InstanceRight.IsBlocking && eventTypeAttack.attackType == NPCAttackType.Melee)
                    damage *= playerToolSo.blockDamageMultiplierAgainstMelee;
                else if (PlayerEquipment.InstanceRight.IsBlocking && eventTypeAttack.attackType == NPCAttackType.Ranged)
                    damage *= playerToolSo.blockDamageMultiplierAgainstRanged;
            }

            if (eventTypeAttack.causesPoisoning && Random.value < eventTypeAttack.chanceToCausePoisoning)
                PlayerStatusEffectEvent.Trigger(
                    PlayerStatusEffectEvent.StatusEffectEventType.Apply,
                    eventTypeAttack.poisonEffectId,
                    eventTypeAttack.poisonEffectCatalogId,
                    PlayerStatusEffectEvent.DirectionOfEvent.Inbound,
                    StatusEffect.StatusEffectKind.None
                );

            // Calculate critical hit
            if (Random.value < eventTypeAttack.critChance)
            {
                damage *= eventTypeAttack.critMultiplier;
                hitWithCriticalMeleeFeedbacks?.PlayFeedbacks();
                PlayerDamageEvent.Trigger(PlayerDamageEvent.DamageTypes.Melee, PlayerDamageEvent.HitTypes.CriticalHit);
            }
            else
            {
                switch (eventTypeAttack.attackType)
                {
                    case NPCAttackType.Melee:
                        hitWithMeleeFeedbacks?.PlayFeedbacks();
                        PlayerDamageEvent.Trigger(
                            PlayerDamageEvent.DamageTypes.Melee, PlayerDamageEvent.HitTypes.Normal);

                        break;
                    case NPCAttackType.Ranged:
                        hitWithProjectileFeedbacks?.PlayFeedbacks();
                        PlayerDamageEvent.Trigger(
                            PlayerDamageEvent.DamageTypes.Ranged, PlayerDamageEvent.HitTypes.Normal);

                        break;
                }
            }

            loseHealthFeedbacks?.PlayFeedbacks();
            return damage;
        }

        public struct StaminaRestoreRateMultiplierByTool
        {
            public string ToolID;
            public float RestoreRateMultiplier;
        }

        // public struct StaminaRestoreRateMultiplierByAttribute
        // {
        //     public int Agility;
        //     public float RestoreRateMultiplier;
        // }

        [Serializable]
        public class StatBasedDialogueNode
        {
            [FormerlySerializedAs("DialogueNodeToStart")]
            public string dialogueNodeToStart;
            [ValueDropdown("GetNpcIdOptions")] [FormerlySerializedAs("NPCId")]
            public string npcId;
            [FormerlySerializedAs("StatType")] public PlayerStatsEvent.PlayerStat statType;

            static string[] GetNpcIdOptions()
            {
                return DialogueManager.GetAllNpcIdOptions();
            }
        }
    }
}
