using System;
using System.Collections.Generic;
using Helpers.Events;
using Helpers.Events.Combat;
using Helpers.Events.Status;
using Helpers.Interfaces;
using Manager.FirstPerson;
using Manager.ProgressionMangers;
using Manager.Status.Scriptable;
using MoreMountains.Tools;
using SharedUI.Alert;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

namespace Manager.Status
{
    public enum VisionEffectType
    {
        Distortion,
        Floaters
    }

    // Track applied effects with their actual stat modifications
    [Serializable]
    [ES3Serializable]
    public class AppliedStatusEffectData
    {
        public string effectID;
        public string catalogID;
        public List<StatChangeRecord> appliedChanges = new();
        [FormerlySerializedAs("enableGlitch")] public bool enableDistortion;
        public bool enableFloaters;
        public float riskOfDeath;
    }

    [Serializable]
    public class StatChangeRecord
    {
        public PlayerStatsEvent.PlayerStat statType;
        public PlayerStatsEvent.PlayerStatChangeType changeType;
        public float amount;
        public float percent;
    }


    public class PlayerStatusEffectManager : MonoBehaviour, ICoreGameService, MMEventListener<PlayerStatusEffectEvent>
    {
        const string SaveKeyAppliedStatusEffects = "AppliedStatusEffects";

        public bool autoSave;


        public List<StatusEffect> nonCatalogStatusEffects;

        [Header("Test Effect")] public StatusEffect testEffect;
        public List<StatusEffect> appliedStatusEffectObjects;


        [FormerlySerializedAs("modalArgs")] public List<AlertUIController.ModalArgs> displayData;

        // Changed to store full data instead of just IDs
        readonly Dictionary<string, AppliedStatusEffectData> _appliedStatusEffects = new();

        bool _dirty;
        bool _hasLoadedAndApplied;
        string _savePath;
        public static PlayerStatusEffectManager Instance { get; private set; }


        void Awake()
        {
            if (Instance == null) Instance = this;
            else
                Destroy(gameObject);
        }
        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Reset();
                return;
            }

            Load();

            // Apply all loaded effects to the current stats
            ReapplyAllEffects();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void Save()
        {
            var dataList = new List<AppliedStatusEffectData>(_appliedStatusEffects.Values);

            ES3.Save(SaveKeyAppliedStatusEffects, _appliedStatusEffects, _savePath ?? GetSaveFilePath());
            ES3.Save(SaveKeyAppliedStatusEffects, dataList, _savePath ?? GetSaveFilePath());
        }
        public void Load()
        {
            var savePath = _savePath ?? GetSaveFilePath();

            if (!ES3.KeyExists(SaveKeyAppliedStatusEffects, savePath))
            {
                Debug.Log("[PlayerStatusEffectManager] No saved effects found.");
                return;
            }

            var loadedEffects = ES3.Load<List<AppliedStatusEffectData>>(
                SaveKeyAppliedStatusEffects,
                savePath
            );

            _appliedStatusEffects.Clear();
            foreach (var effectData in loadedEffects) _appliedStatusEffects[effectData.effectID] = effectData;

            PopulateAppliedStatusEffectObjects();
        }


        public void Reset()
        {
            // throw new System.NotImplementedException();
            _appliedStatusEffects.Clear();
            _hasLoadedAndApplied = false;
            ConditionalSave();
        }
        public void ConditionalSave()
        {
            if (autoSave && _dirty)
            {
                Save();
                _dirty = false;
            }
        }
        public void MarkDirty()
        {
            _dirty = true;
        }
        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.PlayerStatusEffectSave);
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
        public void OnMMEvent(PlayerStatusEffectEvent eventType)
        {
            if (eventType.Type == PlayerStatusEffectEvent.StatusEffectEventType.RemoveAllOfAKind)
                switch (eventType.StatusEffectKind)
                {
                    case StatusEffect.StatusEffectKind.MinorInfections:
                        RemoveAllOfAKind(StatusEffect.StatusEffectKind.MinorInfections);
                        break;
                }

            if (eventType.Direction == PlayerStatusEffectEvent.DirectionOfEvent.Outbound) return;
            if (eventType.Type == PlayerStatusEffectEvent.StatusEffectEventType.Apply)
            {
                ApplyStatusEffect(eventType.EffectID, eventType.CatalogID);
            }
            else if (eventType.Type == PlayerStatusEffectEvent.StatusEffectEventType.Remove)
            {
                RemoveStatusEffect(eventType.EffectID);
            }
            else if (eventType.Type == PlayerStatusEffectEvent.StatusEffectEventType.RemoveAllFromCatalog)
            {
                if (eventType.CatalogID == "StatusEffectCatalog") ApplyDecontaminationEffects(eventType.CatalogID);
                RemoveAllEffectsFromCatalog(eventType.CatalogID);
            }
        }

        // IEnumerator WaitThenTriggerModal(string modalId, float waitSeconds)
        // {
        //     yield return new WaitForSeconds(waitSeconds);
        //     TriggerModalById(modalId);
        // }


        public void TriggerShowAlertForStatusEffect(string modalId)
        {
            // Find modal data by ID (case-insensitive match)
            var args = displayData.Find(m => string.Equals(m.ID, modalId, StringComparison.OrdinalIgnoreCase));
            if (args.ID == null)
            {
                Debug.LogWarning($"[ContaminationManager] No ModalArgs found for ID: {modalId}");
                return;
            }

            // Build and trigger the AlertEvent
            AlertEvent.Trigger(
                AlertReason.StatusEffectApplied,
                args.description,
                args.title,
                AlertType.PauseAndGiveInfo,
                alertImage: args.icon,
                alertIcon: args.icon
            );
        }

        public void ApplyDecontaminationEffects(string catalogID)
        {
            // var effectsToUse = new List<string>();
            // foreach (var effectData in _appliedStatusEffects.Values)
            //     if (effectData.catalogID == catalogID)
            //         effectsToUse.Add(effectData.effectID);

            // foreach (var effectID in effectsToUse)
            // {
            //     var effect = GetStatusEffectByID(catalogID, effectID);
            //     if (effect != null)
            //     {
            //         var secondEffect = GetSecond(effect);
            //         if (secondEffect != null)
            //             ApplyStatusEffect("DecontaminationEffectCatalog", secondEffect.effectID);
            //     }
            // }
        }


        public void RemoveAllEffectsFromCatalog(string catalogID)
        {
            var effectsToRemove = new List<string>();
            foreach (var effectData in _appliedStatusEffects.Values)
                if (effectData.catalogID == catalogID)
                    effectsToRemove.Add(effectData.effectID);

            foreach (var effectID in effectsToRemove)
                RemoveStatusEffect(effectID);
        }

        public void RemoveAllOfAKind(StatusEffect.StatusEffectKind kind)
        {
            var effectsToRemove = new List<string>();
            foreach (var effectData in _appliedStatusEffects.Values)
            {
                var effect = GetStatusEffectByID(effectData.effectID);
                if (effect != null && effect.statusEffectKind == kind)
                    effectsToRemove.Add(effectData.effectID);
            }

            foreach (var effectID in effectsToRemove)
                RemoveStatusEffect(effectID);
        }

        /// <summary>
        ///     Reapply all loaded status effects to current stats.
        ///     Called after loading to restore effect modifications.
        /// </summary>
        void ReapplyAllEffects()
        {
            if (_hasLoadedAndApplied)
            {
                Debug.LogWarning("[PlayerStatusEffectManager] Effects already applied this session.");
                return;
            }

            foreach (var effectData in _appliedStatusEffects.Values)
            {
                // Apply each recorded stat change
                foreach (var statChange in effectData.appliedChanges)
                    PlayerStatsEvent.Trigger(
                        statChange.statType,
                        statChange.changeType,
                        statChange.amount,
                        0f,
                        PlayerStatsEvent.StatChangeCause.Other,
                        statChange.percent
                    );

                var effect = GetStatusEffectByID(effectData.effectID);
                if (effect != null)
                    SetVisionFX(effect, true);
            }


            _hasLoadedAndApplied = true;
            // Debug.Log($"[PlayerStatusEffectManager] Reapplied {_appliedStatusEffects.Count} status effects.");
        }

        public void RemoveStatusEffect(string effectID)
        {
            if (!_appliedStatusEffects.TryGetValue(effectID, out var effectData))
            {
                Debug.LogWarning($"[PlayerStatusEffectManager] Effect {effectID} not currently applied.");
                return;
            }

            // Reverse each stat change
            foreach (var statChange in effectData.appliedChanges)
            {
                // Reverse the change type
                var reverseChangeType = statChange.changeType == PlayerStatsEvent.PlayerStatChangeType.Increase
                    ? PlayerStatsEvent.PlayerStatChangeType.Decrease
                    : PlayerStatsEvent.PlayerStatChangeType.Increase;

                PlayerStatsEvent.Trigger(
                    statChange.statType,
                    reverseChangeType,
                    statChange.amount,
                    0f,
                    PlayerStatsEvent.StatChangeCause.Other,
                    statChange.percent
                );
            }

            var effect = GetStatusEffectByID(effectID);
            if (effect == null)
            {
                Debug.LogError($"[PlayerStatusEffectManager] Effect {effectID} not found.");
                return;
            }

            SetVisionFX(effect, false);


            _appliedStatusEffects.Remove(effectID);
            PopulateAppliedStatusEffectObjects();
            MarkDirty();
            ConditionalSave();
            PlayerStatusEffectEvent.Trigger(
                PlayerStatusEffectEvent.StatusEffectEventType.Remove, effectID, effectData.catalogID,
                PlayerStatusEffectEvent.DirectionOfEvent.Outbound, StatusEffect.StatusEffectKind.None);

            Debug.Log($"[PlayerStatusEffectManager] Removed effect: {effectID}");
        }


        public void ApplyStatusEffect(string effectID, string catalogID = null)
        {
            var effectiveMultiplier = AttributesManager.Instance.GetStatusEffectSeverityMultiplier(effectID);
            // Check if already applied
            if (_appliedStatusEffects.ContainsKey(effectID))
            {
                Debug.LogWarning($"[PlayerStatusEffectManager] Effect {effectID} already applied.");
                return;
            }

            var effect = GetStatusEffectByID(effectID);
            if (effect == null)
            {
                Debug.LogError($"[PlayerStatusEffectManager] Effect {effectID} not found in catalog {catalogID}.");
                return;
            }

            // Create record of this application
            var effectData = new AppliedStatusEffectData
            {
                effectID = effectID,
                catalogID = catalogID,
                enableDistortion = effect.distortion,
                riskOfDeath = effect.riskOfDeath,
                enableFloaters = effect.floaters
            };

            if (effect.riskOfDeath > 0f)
            {
                var roll = Random.Range(0f, 1f);
                if (roll < effect.riskOfDeath)
                {
                    var newDeath = new DeathInformation(PlayerStatsEvent.StatChangeCause.DecontaminationChamber);

                    PlayerDeathEvent.Trigger(newDeath);

                    return;
                }
            }

            // Apply and record each stat change
            foreach (var statChange in effect.statsChanges)
            {
                // Record what we're applying
                effectData.appliedChanges.Add(
                    new StatChangeRecord
                    {
                        statType = statChange.statType,
                        changeType = statChange.changeType,
                        amount = statChange.amount,
                        percent = statChange.percent
                    });

                // Apply the change
                PlayerStatsEvent.Trigger(
                    statChange.statType,
                    statChange.changeType,
                    statChange.amount,
                    0f,
                    PlayerStatsEvent.StatChangeCause.Other,
                    statChange.percent
                );
            }

            // Find with tag "MainSceneVolume"
            // if (effect.enableBloom && effect.enableGlitch)
            SetVisionFX(effect, true);


            _appliedStatusEffects[effectID] = effectData;
            PopulateAppliedStatusEffectObjects();


            PlayerStatusEffectEvent.Trigger(
                PlayerStatusEffectEvent.StatusEffectEventType.Apply, effectID, catalogID,
                PlayerStatusEffectEvent.DirectionOfEvent.Outbound, StatusEffect.StatusEffectKind.None);

            // TriggerShowAlertForStatusEffect(effectID);


            MarkDirty();
            ConditionalSave();

            Debug.Log($"[PlayerStatusEffectManager] Applied effect: {effectID}");
        }
        static void SetVisionFX(StatusEffect effect, bool apply)
        {
            if (effect.distortion)
                VisionAffectingStatusEffEvent.Trigger(VisionAffectingStatusEffType.Distortion, apply);


            if (effect.floaters) VisionAffectingStatusEffEvent.Trigger(VisionAffectingStatusEffType.Floaters, apply);
        }

        public void PopulateAppliedStatusEffectObjects()
        {
            appliedStatusEffectObjects.Clear();
            foreach (var effectData in _appliedStatusEffects.Values)
            {
                var effect = GetStatusEffectByID(effectData.effectID);
                if (effect != null) appliedStatusEffectObjects.Add(effect);
            }
        }
        [Button("Apply Test Effect")]
        public void TestApplyEffect()
        {
            if (testEffect == null) return;
            PlayerStatusEffectEvent.Trigger(
                PlayerStatusEffectEvent.StatusEffectEventType.Apply, testEffect.effectID, "TestCatalog",
                PlayerStatusEffectEvent.DirectionOfEvent.Inbound, StatusEffect.StatusEffectKind.None);

            // ApplyStatusEffect("TestCatalog", testEffect.effectID);
        }

        [Button("Remove Test Effect")]
        public void TestRemoveEffect()
        {
            if (testEffect == null) return;
            PlayerStatusEffectEvent.Trigger(
                PlayerStatusEffectEvent.StatusEffectEventType.Remove, testEffect.effectID, "TestCatalog",
                PlayerStatusEffectEvent.DirectionOfEvent.Inbound, StatusEffect.StatusEffectKind.None);

            // RemoveStatusEffect(testEffect.effectID);
        }

        public bool IsPlayerAffectedByStatusKind(StatusEffect.StatusEffectKind kind)
        {
            foreach (var effectData in _appliedStatusEffects.Values)
            {
                var effect = GetStatusEffectByID(effectData.effectID);
                if (effect != null && effect.statusEffectKind == kind)
                    return true;
            }

            return false;
        }

        public StatusEffect GetStatusEffectByID(string effectID)
        {
            // Search non-catalog effects
            var nonCatalogEffect = nonCatalogStatusEffects.Find(e => e.effectID == effectID);
            if (nonCatalogEffect != null) return nonCatalogEffect;

            // Search all catalogs
            // foreach (var catalog in statusEffectCatalogs)
            // {
            //     var effect = catalog.GetStatusEffectByID(effectID);
            //     if (effect != null) return effect;
            // }

            Debug.LogError($"Effect with ID {effectID} not found in any catalog.");
            return null;
        }


        public bool HasEffect(string effectID)
        {
            return _appliedStatusEffects.ContainsKey(effectID);
        }
        public string GetAbbreviation(PlayerStatsEvent.PlayerStat statChangeStatType)
        {
            switch (statChangeStatType)
            {
                case PlayerStatsEvent.PlayerStat.ContaminationPointsPerCU:
                    return "CONT/CU";
                case PlayerStatsEvent.PlayerStat.BaseMaxHealth:
                    return "BA HLT MAX";
                case PlayerStatsEvent.PlayerStat.BaseMaxStamina:
                    return "BA STM MAX";
                case PlayerStatsEvent.PlayerStat.BaseMaxVision:
                    return "BA VIS MAX";
                case PlayerStatsEvent.PlayerStat.BaseMaxContamination:
                    return "BA CONT MAX";
                case PlayerStatsEvent.PlayerStat.CurrentMaxHealth:
                    return "CUR HLT MAX";
                case PlayerStatsEvent.PlayerStat.CurrentStaminaRestoreRate:
                    return "CUR STM MAX";
                case PlayerStatsEvent.PlayerStat.CurrentMaxVision:
                    return "CUR VIS MAX";
                case PlayerStatsEvent.PlayerStat.CurrentMaxContamination:
                    return "CUR CONT MAX";
                default:
                    return statChangeStatType.ToString();
            }
        }
        public StatusEffect[] GetAllCurrentStatusEffects()
        {
            return appliedStatusEffectObjects.ToArray();
        }
    }
}
