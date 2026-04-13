using Helpers.Events;
using Helpers.Events.Gated;
using Helpers.Events.Progression;
using Helpers.Interfaces;
using MoreMountains.Tools;
using OWPData.ScriptableObjects;
using SharedUI.Progression;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.ProgressionMangers
{
    public class AttributesManager : MonoBehaviour, ICoreGameService,
        MMEventListener<GatedLevelingEvent>, MMEventListener<NotifyAttributesNewlySetEvent>
    {
        const float baseCost = 20f; // cost for first level
        const float growth = 1.4f; // how fast it scales
        public bool autoSave;

        [Header("References")] [SerializeField]
        PlayerStatsSheet playerStatsSheet;
        [SerializeField] LevelingManager levelingManager;


        [Header("Overrides")] public bool overrideAttributesOnLoad;
        [ShowIf("overrideAttributesOnLoad")] [SerializeField]
        int overrideStrength = 2;
        [ShowIf("overrideAttributesOnLoad")] [SerializeField]
        int overrideAgility = 2;
        [ShowIf("overrideAttributesOnLoad")] [SerializeField]
        int overrideDexterity = 2;
        [FormerlySerializedAs("overrideMentalToughness")] [ShowIf("overrideAttributesOnLoad")] [SerializeField]
        int overrideToughness = 2;
        [ShowIf("overrideAttributesOnLoad")] [SerializeField]
        int overrideExobiotic = 2;
        [ShowIf("overrideAttributesOnLoad")] [SerializeField]
        int overrideWillpower = 2;

        [SerializeField] float staminaPerAgilityIncrease = 5;
        [FormerlySerializedAs("staminaPerToughnessIncrease")]
        [FormerlySerializedAs("staminaPerMentalToughnessIncrease")]
        [SerializeField]
        public float healthPerToughnessIncrease = 7f;
        [SerializeField] float contaminationResistPerMentalToughnessIncrease = 2.5f;
        [SerializeField] float contaminationResistPerExobioticIncrease = 5f;
        // has endurance and agility's traditional 
        // functions been merged into a single stat...for now
        int _agility;


        // has perception and dexterity's traditional (and possibly thief)
        // functions been merged into a single stat...for now
        int _dexterity;
        bool _dirty;

        // stat for assimilation of exobiota
        int _exobiotic;


        string _savePath;
        // just strength as normal
        int _strength;

        int _toughness;

        int _willpower;


        public int Agility
        {
            get => _agility;
            set
            {
                _agility = value;
                MarkDirty();
            }
        }

        public int Toughness
        {
            get => _toughness;
            set
            {
                _toughness = value;
                MarkDirty();
            }
        }

        public int Dexterity
        {
            get => _dexterity;
            set
            {
                _dexterity = value;
                MarkDirty();
            }
        }

        public int Exobiotic
        {
            get => _exobiotic;
            set
            {
                _exobiotic = value;
                MarkDirty();
            }
        }


        public int Strength
        {
            get => _strength;
            set
            {
                _strength = value;
                MarkDirty();
            }
        }
        public int Willpower
        {
            get => _willpower;
            set
            {
                _willpower = value;
                MarkDirty();
            }
        }


        public static AttributesManager Instance { get; private set; }

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
            this.MMEventStartListening<GatedLevelingEvent>();
            this.MMEventStartListening<NotifyAttributesNewlySetEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<GatedLevelingEvent>();
            this.MMEventStopListening<NotifyAttributesNewlySetEvent>();
        }
        public void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("Strength", _strength, path);
            ES3.Save("Agility", _agility, path);
            ES3.Save("Dexterity", _dexterity, path);
            ES3.Save("Toughness", _toughness, path);
            ES3.Save("Exobiotic", _exobiotic, path);
            ES3.Save("Willpower", _willpower, path);


            _dirty = false;
        }
        public void Load()
        {
            if (overrideAttributesOnLoad)
            {
                _strength = overrideStrength;
                _agility = overrideAgility;
                _dexterity = overrideDexterity;
                _toughness = overrideToughness;
                _exobiotic = overrideExobiotic;
                _willpower = overrideWillpower;


                MarkDirty();

                ConditionalSave();

                return;
            }

            var path = GetSaveFilePath();

            if (ES3.KeyExists("Strength", path))
                _strength = ES3.Load<int>("Strength", path);

            if (ES3.KeyExists("Agility", path))
                _agility = ES3.Load<int>("Agility", path);

            if (ES3.KeyExists("Dexterity", path))
                _dexterity = ES3.Load<int>("Dexterity", path);

            if (ES3.KeyExists("Toughness", path))
                _toughness = ES3.Load<int>("Toughness", path);


            if (ES3.KeyExists("Exobiotic", path))
                _exobiotic = ES3.Load<int>("Exobiotic", path);

            if (ES3.KeyExists("Willpower", path))
                _willpower = ES3.Load<int>("Willpower", path);
        }
        public void Reset()
        {
            _strength = 1;
            _agility = 1;
            _dexterity = 1;
            _toughness = 1;
            _exobiotic = 1;
            _willpower = 1;


            MarkDirty();

            ConditionalSave();
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.AttributesSave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void OnMMEvent(GatedLevelingEvent eventType)
        {
            if (eventType.EventType == GatedInteractionEventType.CompleteInteraction)
            {
                var newAttributeValues = eventType.AttributeValues;

                // If any have increased, call AttributeLevelUpEvent
                if (newAttributeValues.strength > _strength)
                    AttributeLevelUpEvent.Trigger(AttributeType.Strength, newAttributeValues.strength);

                if (newAttributeValues.agility > _agility)
                    AttributeLevelUpEvent.Trigger(AttributeType.Agility, newAttributeValues.agility);

                if (newAttributeValues.dexterity > _dexterity)
                    AttributeLevelUpEvent.Trigger(AttributeType.Dexterity, newAttributeValues.dexterity);

                if (newAttributeValues.toughness > _toughness)
                    AttributeLevelUpEvent.Trigger(AttributeType.Toughness, newAttributeValues.toughness);

                if (newAttributeValues.exobiotic > _exobiotic)
                    AttributeLevelUpEvent.Trigger(AttributeType.Exobiotic, newAttributeValues.exobiotic);

                if (newAttributeValues.willpower > _willpower)
                    AttributeLevelUpEvent.Trigger(AttributeType.Willpower, newAttributeValues.willpower);


                Strength = newAttributeValues.strength;
                Agility = newAttributeValues.agility;
                Dexterity = newAttributeValues.dexterity;
                Toughness = newAttributeValues.toughness;
                Exobiotic = newAttributeValues.exobiotic;
                Willpower = newAttributeValues.willpower;

                MarkDirty();
            }
        }
        public void OnMMEvent(NotifyAttributesNewlySetEvent eventType)
        {
            Strength = eventType.Strength;
            Agility = eventType.Agility;
            Dexterity = eventType.Dexterity;
            Toughness = eventType.Toughness;
            Exobiotic = eventType.BioticLevel;
            Willpower = eventType.Willpower;

            MarkDirty();
        }


        public int GetXpRequiredForLevel(int level)
        {
            if (level <= 1)
                return Mathf.RoundToInt(baseCost);

            return Mathf.RoundToInt(baseCost * Mathf.Pow(growth, level - 2));
        }


        public float GetEffectiveTimeCostMultiplier(GatedInteractionType interactionType)
        {
            switch (interactionType)
            {
                case GatedInteractionType.BreakObstacle:
                    return 1.0f - Strength * 0.05f;
                case GatedInteractionType.HarvesteableBiological:
                    return 1.0f - Dexterity * 0.05f;
                case GatedInteractionType.InteractMachine:
                    return 1.0f - Dexterity * 0.05f;
                case GatedInteractionType.NotGated:
                    return 1.0f - Dexterity * 0.05f;
                case GatedInteractionType.Rest:
                    return 1.0f;
                default:
                    return 1.0f;
            }
        }
        public float GetStatusEffectSeverityMultiplier(string effectID)
        {
            // higher mental toughness reduces severity of status effects
            return 1.0f;
        }
    }
}
