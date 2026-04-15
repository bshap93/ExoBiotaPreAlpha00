using System;
using System.Collections.Generic;
using FirstPersonPlayer.ScriptableObjects.Scenario;
using Helpers.Events.Progression;
using Helpers.Events.Progression.Scenario;
using Helpers.Exceptions;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.StateManager
{
    [Serializable]
    public class ScenarioInstance
    {
        public string scenarioDefinitionId;

        public Dictionary<string, bool> BooleanFlagValuesDict = new();
        public Dictionary<string, int> IntCountValuesDict = new();

        public bool GetBooleanFlag(string flag)
        {
            if (BooleanFlagValuesDict.ContainsKey(flag)) return BooleanFlagValuesDict[flag];
            return false;
        }

        public int GetIntFlag(string countKey)
        {
            if (IntCountValuesDict.ContainsKey(countKey)) return IntCountValuesDict[countKey];
            return 0;
        }

        public void SetBooleanFlag(string flag, bool value)
        {
            if (BooleanFlagValuesDict.ContainsKey(flag))
                BooleanFlagValuesDict[flag] = value;
        }

        public void SetIntCount(string countKey, int value)
        {
            if (IntCountValuesDict.ContainsKey(countKey))
                IntCountValuesDict[countKey] = value;
        }
    }

    public class ScenarioManager : StateManager<ScenarioManager>, MMEventListener<ScenarioLifeCycleEvent>,
        MMEventListener<ScenarioBoolValueEvent>, MMEventListener<ScenarioIntValueEvent>
    {
        [SerializeField] List<ScenarioDefinition> scenarioDefinitions = new();


        Dictionary<string, ScenarioDefinition> _scenarioDefinitionsLookup;
        Dictionary<string, ScenarioInstance> _scenarioInstances = new();

        public override void Reset()
        {
            _scenarioInstances.Clear();
            MarkDirty();
            ConditionalSave();
        }
        void Start()
        {
            InitializeScenarioDefinitions();
            if (!ES3.FileExists(GetSaveFilePath())) return;
            Load();
        }
        void OnEnable()
        {
            this.MMEventStartListening<ScenarioBoolValueEvent>();
            this.MMEventStartListening<ScenarioIntValueEvent>();
            this.MMEventStartListening<ScenarioLifeCycleEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<ScenarioBoolValueEvent>();
            this.MMEventStopListening<ScenarioIntValueEvent>();
            this.MMEventStopListening<ScenarioLifeCycleEvent>();
        }
        public void OnMMEvent(ScenarioBoolValueEvent eventType)
        {
            SetBooleanFlag(eventType.ScenarioUniqueID, eventType.KeyId, eventType.Value);
        }
        public void OnMMEvent(ScenarioIntValueEvent eventType)
        {
            switch (eventType.EventType)
            {
                case ScenarioIntValueEvent.ScenarioDataEventType.SetValue:
                    SetIntCounter(eventType.ScenarioUniqueID, eventType.KeyId, eventType.ValueId);
                    break;
                case ScenarioIntValueEvent.ScenarioDataEventType.IncrementValue:
                    var currentVal = GetIntCounter(eventType.ScenarioUniqueID, eventType.KeyId);
                    SetIntCounter(eventType.ScenarioUniqueID, eventType.KeyId, currentVal + eventType.ValueId);
                    break;
                case ScenarioIntValueEvent.ScenarioDataEventType.DecrementValue:
                    var currentVal2 = GetIntCounter(eventType.ScenarioUniqueID, eventType.KeyId);
                    SetIntCounter(eventType.ScenarioUniqueID, eventType.KeyId, currentVal2 - eventType.ValueId);
                    break;
            }
        }
        public void OnMMEvent(ScenarioLifeCycleEvent lifeCycleEventType)
        {
            switch (lifeCycleEventType.LifeCycleEventType)
            {
                case ScenarioLifeCycleEventType.ScenarioStarted:
                    StartScenario(lifeCycleEventType.ScenarioUniqueID);
                    break;
                case ScenarioLifeCycleEventType.ScenarioFinished:
                    FinishScenario(lifeCycleEventType.ScenarioUniqueID);
                    break;
            }
        }

        public int GetIntCounter(string scenarioUniqueID, string counterKey)
        {
            if (!_scenarioInstances.TryGetValue(scenarioUniqueID, out var instance)) return 0;
            if (!instance.IntCountValuesDict.ContainsKey(counterKey)) return 0;

            return instance.IntCountValuesDict[counterKey];
        }

        void StartScenario(string scenarioUniqueID)
        {
            if (_scenarioInstances.ContainsKey(scenarioUniqueID)) return;
            if (!_scenarioDefinitionsLookup.TryGetValue(scenarioUniqueID, out var def))
                Debug.LogWarning($"No ScenarioDefinition found for id: {scenarioUniqueID}");

            var instance = new ScenarioInstance
            {
                scenarioDefinitionId = scenarioUniqueID
            };

            if (def != null)
            {
                foreach (var flag in def.booleanFlags) instance.BooleanFlagValuesDict[flag] = false;
                foreach (var count in def.intCounters) instance.IntCountValuesDict[count] = 0;
            }

            _scenarioInstances.Add(scenarioUniqueID, instance);
            Debug.Log($"Started scenario '{scenarioUniqueID}'");
            MarkDirty();
            ConditionalSave();
        }

        void SetBooleanFlag(string scenarioUniqueID, string flagKey, bool value)
        {
            if (!_scenarioInstances.TryGetValue(scenarioUniqueID, out var instance))
            {
                StartScenario(scenarioUniqueID);
                instance = _scenarioInstances[scenarioUniqueID];
                if (instance == null)
                {
                    Debug.LogError($"Failed to create scenario instance for '{scenarioUniqueID}'");
                    return;
                }
            }


            instance.BooleanFlagValuesDict[flagKey] = value;
            MarkDirty();
            ConditionalSave();
        }

        void SetIntCounter(string scenarioUniqueID, string counterKey, int value)
        {
            if (!_scenarioInstances.TryGetValue(scenarioUniqueID, out var instance))
            {
                StartScenario(scenarioUniqueID);
                instance = _scenarioInstances[scenarioUniqueID];
                if (instance == null)
                {
                    Debug.LogError($"Failed to create scenario instance for '{scenarioUniqueID}'");
                    return;
                }
            }


            instance.IntCountValuesDict[counterKey] = value;
            MarkDirty();
            ConditionalSave();
        }

        void FinishScenario(string scenarioUniqueID)
        {
            // Not yet needed
        }

        void InitializeScenarioDefinitions()
        {
            _scenarioDefinitionsLookup = new Dictionary<string, ScenarioDefinition>();

            foreach (var scenarioDefinition in scenarioDefinitions)
                _scenarioDefinitionsLookup[scenarioDefinition.scenarioUniqueID] = scenarioDefinition;
        }
        public override void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("ScenarioInstances", _scenarioInstances, path);
            Dirty = false;
        }
        public override void Load()
        {
            var path = GetSaveFilePath();
            _scenarioInstances.Clear();

            if (ES3.KeyExists("ScenarioInstances", path))
                _scenarioInstances = ES3.Load<Dictionary<string, ScenarioInstance>>("ScenarioInstances", path);

            Dirty = false;
        }
        protected override string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.ScenarioSave);
        }
        public bool GetBooleanFlagValue(string scenarioUniqueID, string flagKeyValue)
        {
            if (!_scenarioInstances.TryGetValue(scenarioUniqueID, out var instance)) throw new ValueNotFoundException();
            if (!instance.BooleanFlagValuesDict.ContainsKey(flagKeyValue)) throw new ValueNotFoundException();

            return instance.BooleanFlagValuesDict[flagKeyValue];
        }
        public bool IsScenarioActive(string scenarioID)
        {
            foreach (var scenario in _scenarioInstances)
                if (scenario.Key == scenarioID)
                    return true;

            return false;
        }
    }
}
