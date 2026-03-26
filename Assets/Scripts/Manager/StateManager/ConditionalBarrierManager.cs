using System;
using System.Collections.Generic;
using Helpers.Events.Machinery;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.StateManager
{
    public class ConditionalBarrierManager : StateManager<ConditionalBarrierManager>,
        MMEventListener<BarrierInitializationStateEvent>
    {
        [Serializable]
        public enum BarrierInitializationState
        {
            None,
            ShouldBeInitializedFresh,
            ShouldBeInitializedAndTriggered,
            ShouldBeDestroyed
        }

        Dictionary<string, BarrierInitializationState> _barrierStates = new(StringComparer.Ordinal);


        public override void Reset()
        {
            _barrierStates.Clear();
            MarkDirty();
            ConditionalSave();
        }
        void Start()
        {
            if (!ES3.FileExists(GetSaveFilePath())) return;
            Load();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(BarrierInitializationStateEvent eventType)
        {
            if (eventType.EventType == BarrierStateEventType.SetNewBarrierState)
                AddOrSetBarrierState(eventType.UniqueID, eventType.BarrierInitializationState);
        }
        public override void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("BarrierStates", _barrierStates, path);
            Dirty = false;
        }

        public BarrierInitializationState GetBarrierInitializationState(string uniqueId)
        {
            return _barrierStates.GetValueOrDefault(uniqueId, BarrierInitializationState.None);
        }

        public void AddOrSetBarrierState(string uniqueId, BarrierInitializationState barrierInitializationState)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;

            _barrierStates[uniqueId] = barrierInitializationState;
            Debug.Log("Set barrier state for " + uniqueId + " to " + barrierInitializationState);
            MarkDirty();
            ConditionalSave();
        }
        public override void Load()
        {
            var path = GetSaveFilePath();
            _barrierStates.Clear();

            if (ES3.KeyExists("BarrierStates", path))
                _barrierStates = ES3.Load<Dictionary<string, BarrierInitializationState>>("BarrierStates", path);

            Dirty = false;
        }
        protected override string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.BarrierStateSave);
        }
    }
}
