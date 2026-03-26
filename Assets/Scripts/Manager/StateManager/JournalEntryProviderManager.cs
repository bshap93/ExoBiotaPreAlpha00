using System;
using System.Collections.Generic;
using Helpers.Events.Journal;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.StateManager
{
    public class JournalEntryProviderManager : StateManager<JournalEntryProviderManager>,
        MMEventListener<JournalEntryProviderStateEvent>
    {
        [Serializable]
        public enum EntryProviderState
        {
            None,
            ShouldBeInitialized,
            HasBeenInitialized,
            ShouldBeDestroyed
        }

        const string EntryProviderSaveStateKey = "JournalEntryProviderStates";

        Dictionary<string, EntryProviderState> _journalEntryProviderstates = new(StringComparer.Ordinal);
        public override void Reset()
        {
            _journalEntryProviderstates.Clear();
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

        public void OnMMEvent(JournalEntryProviderStateEvent eventType)
        {
            if (eventType.EventType == JournalEntryProviderStateEventType.SetNewJournalEntryProviderState)
                AddOrSetJournalProviderState(eventType.UniqueID, eventType.JournalEntryProviderState);
        }

        public override void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save(EntryProviderSaveStateKey, _journalEntryProviderstates, path);
            Dirty = false;
        }
        public override void Load()
        {
            var path = GetSaveFilePath();
            _journalEntryProviderstates.Clear();

            if (ES3.KeyExists(EntryProviderSaveStateKey, path))
                _journalEntryProviderstates =
                    ES3.Load<Dictionary<string, EntryProviderState>>(EntryProviderSaveStateKey, path);

            Dirty = false;
        }
        protected override string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.EntryProviderStateSave);
        }
        public EntryProviderState GetJournalProviderState(string uniqueID)
        {
            return _journalEntryProviderstates.GetValueOrDefault(uniqueID, EntryProviderState.None);
        }

        public void AddOrSetJournalProviderState(string uniqueID,
            EntryProviderState journalEntryProviderInitializationState)
        {
            if (string.IsNullOrEmpty(uniqueID)) return;

            _journalEntryProviderstates[uniqueID] = journalEntryProviderInitializationState;
            Debug.Log(
                "Set journal entry provider state for " + uniqueID + " to " + journalEntryProviderInitializationState);

            MarkDirty();
            ConditionalSave();
        }
    }
}
