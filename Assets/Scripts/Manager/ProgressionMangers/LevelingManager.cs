using System;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Progression;
using Helpers.Events.Status;
using Helpers.Interfaces;
using Helpers.StaticHelpers;
using Inventory;
using MoreMountains.Tools;
using SharedUI.Progression;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.ProgressionMangers
{
    [Serializable]
    public class PlayerStartingClass
    {
        public int id;
        [FormerlySerializedAs("ClassName")] public string className;
        [FormerlySerializedAs("StartingStrength")]
        public int startingStrength;
        [FormerlySerializedAs("StartingAgility")]
        public int startingAgility;
        [FormerlySerializedAs("StartingDexterity")]
        public int startingDexterity;
        public int startingToughness;
        [FormerlySerializedAs("StartingBioticLevel")]
        public int startingBioticLevel;
        public int startingWillpowerLevel;
    }

    public class LevelingManager : MonoBehaviour, ICoreGameService,
        MMEventListener<BioticCoreXPConversionEvent>, MMEventListener<EnemyXPRewardEvent>,
        MMEventListener<PlayerSetsClassEvent>,
        MMEventListener<IncrementAttributeEvent>
    {
        [Header("References")] [SerializeField]
        AttributesManager attributesManager;
        [SerializeField] PlayerMutableStatsManager playerMutableStatsManager;
        [SerializeField] GlobalInventoryManager globalInventoryManager;

        [Header("Leveling Stats")] [SerializeField]
        LevelStats[] levelStats;
        [SerializeField] int levelCap = 20;


        [FormerlySerializedAs("contaminationAmountByUpgrade")] [SerializeField]
        ContaminationAmountByExobiotic[] contaminationAmountPerExoBioticLevel;
        [FormerlySerializedAs("baseStaminaRestoreRateByUpgrade")] [SerializeField]
        BaseStaminaRestoreRateByAgility[] baseStaminaRestoreRateByAgility;
        [SerializeField] bool autoSave;

        [SerializeField] int attributePointsStartWith;

        [SerializeField] public PlayerStartingClass[] availablePresetClasses;

        int _currentPlayerClassId;
        bool _dirty;

        string _savePath;

        int _unspentAttribuePoints;
        int _unspentStatUpgrades;

        public PlayerStartingClass CurrentPlayerClass => availablePresetClasses[_currentPlayerClassId];

        public int UnspentAttributePoints
        {
            get => _unspentAttribuePoints;
            set
            {
                _unspentAttribuePoints = value;
                MarkDirty();
            }
        }


        public int CurrentLevel { get; set; }
        public int CurrentTotalXP { get; set; }


        public int TotalXpNeededForNextLevel
        {
            get
            {
                if (CurrentLevel >= levelCap)
                    return 0; // No more XP needed if at or above level cap

                return GetLevelStats(CurrentLevel + 1).totalXPRequired;
            }
        }
        public int MoreXpNeededForNextLevel => TotalXpNeededForNextLevel - CurrentTotalXP;

        public static LevelingManager Instance { get; private set; }

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
            if (!HasSavedData())
            {
                Reset();
                return;
            }

            Load();
        }

        void OnEnable()
        {
            this.MMEventStartListening<BioticCoreXPConversionEvent>();
            this.MMEventStartListening<EnemyXPRewardEvent>();
            this.MMEventStartListening<PlayerSetsClassEvent>();
            this.MMEventStartListening<IncrementAttributeEvent>();
        }
        void OnDisable()
        {
            this.MMEventStopListening<BioticCoreXPConversionEvent>();
            this.MMEventStopListening<EnemyXPRewardEvent>();
            this.MMEventStopListening<PlayerSetsClassEvent>();
            this.MMEventStopListening<IncrementAttributeEvent>();
        }

        public void Save()
        {
            var path = GetSaveFilePath();

            ES3.Save("CurrentLevel", CurrentLevel, path);
            ES3.Save("CurrentTotalXP", CurrentTotalXP, path);
            // ES3.Save("HealthUpgradeLevel", HealthUpgradeLevel, path);
            // ES3.Save("StaminaUpgradeLevel", StaminaUpgradeLevel, path);
            // ES3.Save("ContaminationUpgradeLevel", ContaminationUpgradeLevel, path);
            ES3.Save("UnspentAttributePoints", UnspentAttributePoints, path);
            // ES3.Save("UnspentStatUpgrades", UnspentStatUpgrades, path);
            ES3.Save("CurrentPlayerClassId", _currentPlayerClassId, path);
            _dirty = false;
        }
        public void Load()
        {
            var path = GetSaveFilePath();

            if (ES3.KeyExists("CurrentLevel", path))
                CurrentLevel = ES3.Load<int>("CurrentLevel", path);

            if (ES3.KeyExists("CurrentTotalXP", path))
                CurrentTotalXP = ES3.Load<int>("CurrentTotalXP", path);

            // if (ES3.KeyExists("HealthUpgradeLevel", path))
            //     HealthUpgradeLevel = ES3.Load<int>("HealthUpgradeLevel", path);
            //
            // if (ES3.KeyExists("StaminaUpgradeLevel", path))
            //     StaminaUpgradeLevel = ES3.Load<int>("StaminaUpgradeLevel", path);
            //
            //
            // if (ES3.KeyExists("ContaminationUpgradeLevel", path))
            //     ContaminationUpgradeLevel = ES3.Load<int>("ContaminationUpgradeLevel", path);

            if (ES3.KeyExists("UnspentAttributePoints", path))
                UnspentAttributePoints = ES3.Load<int>("UnspentAttributePoints", path);

            // if (ES3.KeyExists("UnspentStatUpgrades", path))
            //     UnspentStatUpgrades = ES3.Load<int>("UnspentStatUpgrades", path);

            if (ES3.KeyExists("CurrentPlayerClassId", path))
                _currentPlayerClassId = ES3.Load<int>("CurrentPlayerClassId", path);
        }
        public void Reset()
        {
            CurrentLevel = 1;
            CurrentTotalXP = 0;
            // HealthUpgradeLevel = 1;
            // StaminaUpgradeLevel = 1;
            // ContaminationUpgradeLevel = 1;
            UnspentAttributePoints = attributePointsStartWith;
            // UnspentStatUpgrades = 0;
            _currentPlayerClassId = 0;
            MarkDirty();
        }
        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }
        public void MarkDirty()
        {
            _dirty = true;
        }
        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.LevelingSave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void OnMMEvent(BioticCoreXPConversionEvent conversionEventType)
        {
            if (conversionEventType.EventType == BioticCoreXPEventType.ConvertCoreToXP)
                ConvertCoreToXP(conversionEventType.CoreGrade);
        }


        public void OnMMEvent(EnemyXPRewardEvent eventType)
        {
            var xpToAward = eventType.XPReward;
            AwardXPToPlayer(xpToAward);
        }

        public void OnMMEvent(IncrementAttributeEvent eventType)
        {
            switch (eventType.AttributeType)
            {
                case AttributeType.Strength:
                    attributesManager.Strength += 1;
                    break;
                case AttributeType.Agility:
                    attributesManager.Agility += 1;
                    break;
                case AttributeType.Dexterity:
                    attributesManager.Dexterity += 1;
                    break;
                case AttributeType.Toughness:
                    attributesManager.Toughness += 1;
                    break;
                case AttributeType.Exobiotic:
                    attributesManager.Exobiotic += 1;
                    break;
                case AttributeType.Willpower:
                    attributesManager.Willpower += 1;
                    break;
            }

            UnspentAttributePoints -= 1;

            NotifyAttributesNewlySetEvent.Trigger(
                attributesManager.Strength, attributesManager.Agility, attributesManager.Dexterity,
                attributesManager.Exobiotic, attributesManager.Toughness, attributesManager.Willpower);

            if (eventType.AttributeType == AttributeType.Toughness)
            {
                var newMaxHealth = GetBaseMaxHealthForToughness(attributesManager.Toughness);
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentMaxHealth, PlayerStatsEvent.PlayerStatChangeType.Increase,
                    newMaxHealth - playerMutableStatsManager.CurrentMaxHealth);

                // Also increase current health by that diff
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentHealth, PlayerStatsEvent.PlayerStatChangeType.Increase,
                    newMaxHealth - playerMutableStatsManager.CurrentHealth);
            }

            ProgressionUpdateListenerNotifier.Trigger(
                CurrentTotalXP, CurrentLevel, // UnspentStatUpgrades,
                UnspentAttributePoints);

            AlertEvent.Trigger(
                AlertReason.AttributePointSpent, $"{eventType.AttributeType} increased by 1!", "Attribute Increased");
        }

        public void OnMMEvent(PlayerSetsClassEvent eventType)
        {
            if (eventType.ClassId < 1 || eventType.ClassId >= availablePresetClasses.Length)
            {
                Debug.LogWarning($"Invalid class id: {eventType.ClassId}");
                return;
            }

            _currentPlayerClassId = eventType.ClassId;

            AlertEvent.Trigger(
                AlertReason.ClassSelected, $"Class set to {availablePresetClasses[eventType.ClassId].className}.",
                "Class Selected");

            var startingClass = availablePresetClasses[eventType.ClassId];

            var newMaxHealth = GetBaseMaxHealthForToughness(startingClass.startingToughness);
            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentMaxHealth, PlayerStatsEvent.PlayerStatChangeType.Increase,
                newMaxHealth - playerMutableStatsManager.CurrentMaxHealth);

            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentHealth, PlayerStatsEvent.PlayerStatChangeType.Increase,
                newMaxHealth - playerMutableStatsManager.CurrentHealth);

            NotifyAttributesNewlySetEvent.Trigger(
                startingClass.startingStrength, startingClass.startingAgility, startingClass.startingDexterity,
                startingClass.startingBioticLevel, startingClass.startingToughness,
                startingClass.startingWillpowerLevel);
        }


        public float GetBaseStaminaRestoreRateForAgility(int upgradeLevel)
        {
            foreach (var entry in baseStaminaRestoreRateByAgility)
                if (entry.agility == upgradeLevel)
                    return entry.restoreRate;

            throw new Exception(
                "Agility level " + upgradeLevel + " not found in baseStaminaRestoreRateByAgility array.");
        }
        /// <summary>
        ///     Adds to total XP, and triggers level up if earned.
        /// </summary>
        /// <param name="xpToAward"></param>
        void AwardXPToPlayer(int xpToAward)
        {
            CurrentTotalXP += xpToAward;

            var causedLevelUp = false;

            // Check for level up
            while (CurrentLevel < levelCap && CurrentTotalXP >= TotalXpNeededForNextLevel)
            {
                var newLevel = CurrentLevel + 1;
                LevelUpPlayer(newLevel);

                causedLevelUp = true;
            }

            XPEvent.Trigger(XPEventType.AwardXPToPlayer, xpToAward, causedLevelUp);

            ProgressionUpdateListenerNotifier.Trigger(
                CurrentTotalXP, CurrentLevel, // UnspentStatUpgrades,
                UnspentAttributePoints);
        }

        /// <summary>
        /// </summary>
        /// <param name="newLevel"></param>
        void LevelUpPlayer(int newLevel)
        {
            if (newLevel != CurrentLevel + 1)
                throw new ArgumentException("New level must be exactly one greater than current level.");

            CurrentLevel = newLevel;
            // AwardStatUpgradeToPlayer(newLevel);
            AwardHPIncreaseUpgradeToPlayer(newLevel);
            AwardAttributePointsToPlayer(newLevel);

            LevelingEvent.Trigger(LevelingEventType.LevelUp, newLevel);
        }

        void AwardHPIncreaseUpgradeToPlayer(int newLevel)
        {
            var stats = GetLevelStats(newLevel);
            var diff = GetBaseMaxHealthForToughness(CurrentLevel) - playerMutableStatsManager.CurrentMaxHealth;

            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentMaxHealth, PlayerStatsEvent.PlayerStatChangeType.Increase, diff);

            // Also increase current health by that diff
            PlayerStatsEvent.Trigger(
                PlayerStatsEvent.PlayerStat.CurrentHealth, PlayerStatsEvent.PlayerStatChangeType.Increase, diff);

            AlertEvent.Trigger(
                AlertReason.HealtMaxIncrease, "Health Max increased!", "Health Max Upgrade");
        }

        /// <summary>
        ///     For now, awards one upgrade per level.
        /// </summary>
        /// <param name="level"></param>
        // void AwardStatUpgradeToPlayer(int level)
        // {
        //     UnspentStatUpgrades += 1;
        //
        //     ProgressionUpdateListenerNotifier.Trigger(
        //         CurrentTotalXP, CurrentLevel, UnspentStatUpgrades,
        //         UnspentAttributePoints);
        //
        //     AlertEvent.Trigger(
        //         AlertReason.NewStatUpgrade, "Leveled up and unlocked vitals upgrade! Get to a Terminal to apply.",
        //         "Vitals Upgrade Gained");
        // }

        /// <summary>
        ///     If the level reached grants attribute points, adds them.
        /// </summary>
        /// <param name="level"></param>
        void AwardAttributePointsToPlayer(int level)
        {
            var stats = GetLevelStats(level);
            if (stats.attributePointsGranted > 0)
            {
                UnspentAttributePoints += stats.attributePointsGranted;

                ProgressionUpdateListenerNotifier.Trigger(
                    CurrentTotalXP, CurrentLevel,
                    UnspentAttributePoints);

                AlertEvent.Trigger(
                    AlertReason.NewAttributePoints,
                    $"{stats.attributePointsGranted} new attribute points! Get to a Terminal to apply.",
                    "Attribute Points Gained");
            }
        }

        LevelStats GetLevelStats(int level)
        {
            if (level < 1 || level > levelCap)
                throw new ArgumentException("Level must be greater than or equal to 1.", nameof(level));

            foreach (var stats in levelStats)
                if (stats.level == level)
                    return stats;

            throw new Exception($"Level {level} not found in levelStats array.");
        }

        public int GetXPGainedForCoreGrade(OuterCoreItemObject.CoreObjectValueGrade eventTypeCoreGrade)
        {
            switch (eventTypeCoreGrade)
            {
                case OuterCoreItemObject.CoreObjectValueGrade.StandardGrade:
                    return 10;
                case OuterCoreItemObject.CoreObjectValueGrade.Radiant:
                    return 20;
                case OuterCoreItemObject.CoreObjectValueGrade.Stellar:
                    return 30;
                case OuterCoreItemObject.CoreObjectValueGrade.Unreasonable:
                    return 50;
                case OuterCoreItemObject.CoreObjectValueGrade.MiscExotic:
                    return 0;
                default:
                    return 0;
            }
        }

        public void ConvertCoreToXP(
            OuterCoreItemObject.CoreObjectValueGrade coreGrade)
        {
            // remove one core from inventory
            InventoryHelperCommands.RemoveOuterCore(coreGrade);

            // add the XP
            var amount = 0;
            switch (coreGrade)
            {
                case OuterCoreItemObject.CoreObjectValueGrade.StandardGrade:
                    amount = 10;
                    break;
                case OuterCoreItemObject.CoreObjectValueGrade.Radiant:
                    amount = 20;
                    break;
                case OuterCoreItemObject.CoreObjectValueGrade.Stellar:
                    amount = 30;
                    break;
                case OuterCoreItemObject.CoreObjectValueGrade.Unreasonable:
                    amount = 50;
                    break;
                case OuterCoreItemObject.CoreObjectValueGrade.MiscExotic:
                    amount = 0;
                    break;
            }


            AwardXPToPlayer(amount);

            MarkDirty();
        }

        public float GetHealthAmountForUpgradeLevel(int playerLevel)
        {
            foreach (var entry in levelStats)
                if (entry.level == playerLevel)
                    return entry.healthAmount;

            throw new Exception($"Upgrade level {playerLevel} not found in healthAmountByUpgrade array.");
        }

        public object CurentNumberOfCores()
        {
            return globalInventoryManager.GetTotalNumberOfCores();
        }

        public float GetBaseMaxHealthForToughness(int toughness)
        {
            // Calculate base Max Health appropriate for Current level
            // and current Toughness Attribute score
            var healthAmountByPlayerLevel = GetHealthAmountForUpgradeLevel(CurrentLevel);
            var baseMaxHealth = healthAmountByPlayerLevel +
                                (toughness - 1) * attributesManager.healthPerToughnessIncrease;

            return baseMaxHealth;
        }

        [Serializable]
        public struct BaseStaminaRestoreRateByAgility
        {
            [FormerlySerializedAs("upgradeLevel")] public int agility;
            public float restoreRate;
        }

        [Serializable]
        public class LevelStats
        {
            [FormerlySerializedAs("Level")] public int level;
            [FormerlySerializedAs("TotalXPRequired")]
            public int totalXPRequired;
            [FormerlySerializedAs("AttributePointsGranted")]
            public int attributePointsGranted;
            public int healthAmount;
        }

        [Serializable]
        public class HealthAmountByPlayerLevel
        {
            [FormerlySerializedAs("UpgradeLevel")] public int playerLevel;
            [FormerlySerializedAs("HealthAmount")] public float healthAmount;
        }


        [Serializable]
        public class ContaminationAmountByExobiotic
        {
            [FormerlySerializedAs("upgradeLevel")] public int exoBioticLevel;
            public float contaminationAmount;
        }
    }
}
