using System;
using System.Collections.Generic;
using Helpers.Events.Machinery;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.StateManager
{
    public class ResourceContainerManager : StateManager<ResourceContainerManager>,
        MMEventListener<ResourceContainerInitStateEvent>
    {
        [Serializable]
        public enum ResourceContainerInitializationState
        {
            None,
            IsFullOrInitial,
            IsDepleted,
            ShouldBeDestroyed,
            IsBeingReplenished
        }

        Dictionary<string, ResourceContainerInitializationState>
            _containerStates = new(StringComparer.Ordinal);

        public override void Reset()
        {
            _containerStates.Clear();
            MarkDirty();
            ConditionalSave();
        }
        // Start is called once before the first execution of Update after the MonoBehaviour is created
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
        public void OnMMEvent(ResourceContainerInitStateEvent eventType)
        {
            if (eventType.EventType == ResourceContainerStateEventType.SetNewResourceContainerState)
                AddOrSetResourceContainerState(eventType.UniqueID, eventType.ResourceContainerInitializationState);
        }
        public void AddOrSetResourceContainerState(string uniqueID,
            ResourceContainerInitializationState containerInitState)
        {
            if (string.IsNullOrEmpty(uniqueID)) return;

            _containerStates[uniqueID] = containerInitState;
            Debug.Log("Set container state for " + uniqueID);
            MarkDirty();
            ConditionalSave();
        }
        public override void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("ResourceContainerStates", _containerStates, path);
            Dirty = false;
        }
        public override void Load()
        {
            var path = GetSaveFilePath();
            _containerStates.Clear();

            if (ES3.KeyExists("ResourceContainerStates", path))
                _containerStates =
                    ES3.Load<Dictionary<string, ResourceContainerInitializationState>>("ResourceContainerStates", path);

            Dirty = false;
        }
        protected override string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.ResourceContainersSave);
        }
        public ResourceContainerInitializationState GetContainerState(string uniqueID)
        {
            return _containerStates.GetValueOrDefault(uniqueID, ResourceContainerInitializationState.None);
        }
    }
}
