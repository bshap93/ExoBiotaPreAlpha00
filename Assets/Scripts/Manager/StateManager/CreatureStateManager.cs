using System;
using System.Collections.Generic;
using Helpers.Events.NPCs;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager.StateManager
{
    public class CreatureStateManager : StateManager<CreatureStateManager>,
        MMEventListener<CreatureInitializationStateEvent>, MMEventListener<CreatureSpecialStateEvent>
    {
        [Serializable]
        public enum CreatureInitializationState
        {
            None,
            ShouldBeInitialized,
            ShouldRespawnAndReinitialize,
            HasBeenInitialized,
            ShouldBeDestroyed
        }

        public enum CreatureSpecialState
        {
            None,
            Placated
        }

        public bool overrideAllCreaturesState;
        [FormerlySerializedAs("overrideCreatureState")] [ShowIf("overrideAllCreaturesState")] [SerializeField]
        CreatureInitializationState overrideCreatureInitializationState;
        readonly Dictionary<string, CreatureSpecialState> _creatureSpecialStates = new(StringComparer.Ordinal);

        Dictionary<string, CreatureInitializationState> _creatureStates = new(StringComparer.Ordinal);
        public override void Reset()
        {
            _creatureStates.Clear();
            _creatureSpecialStates.Clear();
            MarkDirty();
            ConditionalSave();
        }

        // In CreatureStateManager or StateManager<T> base
        void Start()
        {
            if (!ES3.FileExists(GetSaveFilePath())) return;
            Load();
        }

        void OnEnable()
        {
            this.MMEventStartListening<CreatureInitializationStateEvent>();
            this.MMEventStartListening<CreatureSpecialStateEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<CreatureInitializationStateEvent>();
            this.MMEventStopListening<CreatureSpecialStateEvent>();
        }
        public void OnMMEvent(CreatureInitializationStateEvent eventType)
        {
            if (eventType.EventType == CreatureStateEventType.SetNewCreatureState)
                AddOrSetCreatureState(eventType.UniqueID, eventType.CreatureInitializationState);
        }
        public void OnMMEvent(CreatureSpecialStateEvent eventType)
        {
            _creatureSpecialStates[eventType.UniqueID] = eventType.CreatureSpecialState;
            MarkDirty();
            ConditionalSave();
        }

        public CreatureSpecialState GetCreatureSpecialState(string uniqueID)
        {
            return _creatureSpecialStates.GetValueOrDefault(uniqueID, CreatureSpecialState.None);
        }
        public override void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("CreatureStates", _creatureStates, path);
            Dirty = false;
        }
        public override void Load()
        {
            var path = GetSaveFilePath();
            _creatureStates.Clear();


            if (ES3.KeyExists("CreatureStates", path))
                _creatureStates = ES3.Load<Dictionary<string, CreatureInitializationState>>("CreatureStates", path);

            if (overrideAllCreaturesState)
            {
                var keys = new List<string>(_creatureStates.Keys);
                foreach (var key in keys) _creatureStates[key] = overrideCreatureInitializationState;
            }

            Dirty = false;
        }

        public CreatureInitializationState GetCreatureState(string uniqueID)
        {
            return _creatureStates.GetValueOrDefault(uniqueID, CreatureInitializationState.None);
        }

        public void AddOrSetCreatureState(string uniqueId, CreatureInitializationState initializationState)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;

            _creatureStates[uniqueId] = initializationState;
            MarkDirty();
            ConditionalSave();
        }
        protected override string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.CreatureStateSave);
        }
    }
}
