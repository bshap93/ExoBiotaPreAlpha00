using System;
using System.Collections.Generic;
using Helpers.Events.Journal;
using Helpers.Events.PlayerData;
using Helpers.Interfaces;
using JournalData;
using JournalData.JournalEntries;
using JournalData.JournalTopics;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Manager.PlayerDataManagers
{
    public class JournalEntryManager : MonoBehaviour, ICoreGameService, MMEventListener<JournalEntryEvent>
    {
        [SerializeField] JournalDatabase journalDatabase;
        [SerializeField] bool autoSave;

        [SerializeField] bool hasInitialTopicsAndEntries;
        [ShowIf("hasInitialTopicsAndEntries")] public JournalTopic[] initialTopics;
        [ShowIf("hasInitialTopicsAndEntries")] public JournalEntry[] initialEntries;
        bool _dirty;

        string _savePath;

        public static JournalEntryManager Instance { get; private set; }

        public
            void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
        }
        void Start()
        {
            _savePath = GetSaveFilePath();
            if (!HasSavedData())
            {
                Reset();
                PopulateDummyData();
                return;
            }

            Load();

            PopulateDummyData();
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
            var path = GetSaveFilePath();

            ES3.Save("JournalTopicsState", _topicInstances, path);
            ES3.Save("JournalEntriesState", _entryInstances, path);

            _dirty = false;
        }
        public void Load()
        {
            var path = GetSaveFilePath();
            if (ES3.KeyExists("JournalTopicsState", path))
                _topicInstances = ES3.Load<Dictionary<string, JournalTopicInstance>>("JournalTopicsState", path);
            else
                _topicInstances = new Dictionary<string, JournalTopicInstance>();

            if (ES3.KeyExists("JournalEntriesState", path))
                _entryInstances = ES3.Load<Dictionary<string, JournalEntryInstance>>("JournalEntriesState", path);
            else
                _entryInstances = new Dictionary<string, JournalEntryInstance>();

            _dirty = false;
        }
        public void Reset()
        {
            _topicInstances.Clear();
            _entryInstances.Clear();
            _dirty = true;
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.JournalEntrySave);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }
        public void OnMMEvent(JournalEntryEvent eventType)
        {
            if (eventType.EventType == JournalEntryEventType.Added)
            {
                if (_entryInstances.ContainsKey(eventType.JournalEntryUniqueId))
                {
                    Debug.LogWarning(
                        $"Journal entry with unique ID {eventType.JournalEntryUniqueId} already exists in journal. Ignoring add event.");

                    return;
                }

                // Add entry 
                var newEntryInstance = new JournalEntryInstance
                {
                    entryID = eventType.JournalEntryUniqueId,
                    AquiredAt = DateTime.Now,
                    state = JournalEntryState.Unread
                };

                _entryInstances[eventType.JournalEntryUniqueId] = newEntryInstance;

                var entryData = journalDatabase.GetEntryAsset(eventType.JournalEntryUniqueId);
                if (entryData == null)
                {
                    Debug.LogError($"Could not find journal entry data for unique ID {eventType.JournalEntryUniqueId}");
                    return;
                }


                // If topic isn't added, automatically add it
                var topicId = entryData.parentalTopic.uniqueID;
                if (!_topicInstances.ContainsKey(topicId))
                {
                    _topicInstances[topicId] = new JournalTopicInstance
                    {
                        aquiredJournalEntryUniqueIds = new List<string> { eventType.JournalEntryUniqueId },
                        aquiredAt = DateTime.Now,
                        journalTopicUniqueId = topicId
                    };

                    JournalNotificationEvent.Trigger(
                        JournalEntityType.Topic, entryData.entryName,
                        "New " + entryData.parentalTopic.topicType + "Topic");

                    JournalTopicEvent.Trigger(JournalTopicEventType.Added, topicId);
                }
                else
                {
                    if (!_topicInstances[topicId].aquiredJournalEntryUniqueIds.Contains(eventType.JournalEntryUniqueId))
                        _topicInstances[topicId].aquiredJournalEntryUniqueIds.Add(eventType.JournalEntryUniqueId);

                    JournalNotificationEvent.Trigger(
                        JournalEntityType.Entry, entryData.entryName,
                        "New " + entryData.parentalTopic.topicType + "Entry");
                }
            }
        }

        void PopulateDummyData()
        {
            if (hasInitialTopicsAndEntries)
            {
                foreach (var topic in initialTopics)
                    if (!_topicInstances.ContainsKey(topic.UniqueID))
                    {
                        _topicInstances[topic.UniqueID] = new JournalTopicInstance
                        {
                            aquiredJournalEntryUniqueIds = new List<string>(),
                            aquiredAt = DateTime.Now,
                            journalTopicUniqueId = topic.UniqueID
                        };

                        MarkDirty();
                        Debug.Log($"Added initial topic {topic.journalTopicName} to journal.");
                    }

                foreach (var entry in initialEntries)
                    if (!_entryInstances.ContainsKey(entry.UniqueID))
                    {
                        _entryInstances[entry.UniqueID] = new JournalEntryInstance
                        {
                            entryID = entry.UniqueID,
                            AquiredAt = DateTime.Now,
                            state = JournalEntryState.Unread
                        };

                        MarkDirty();
                    }

                JournalTopicEvent.Trigger(JournalTopicEventType.Initialized);
            }
        }

        public List<JournalTopic> GetTopicsAquired()
        {
            var topics = new List<JournalTopic>();
            foreach (var topicID in _topicInstances.Keys)
            {
                var topicData = journalDatabase.GetTopicAsset(topicID);
                if (topicData != null)
                    topics.Add(topicData);
            }

            return topics;
        }

        public List<JournalEntry> GetEntriesAquired(string topicID)
        {
            var entries = new List<JournalEntry>();
            if (topicID == null) return entries;
            var topicInstance = _topicInstances.TryGetValue(topicID, out var instance) ? instance : null;
            if (topicInstance != null)
                foreach (var topicEntryID in topicInstance.aquiredJournalEntryUniqueIds)
                {
                    var entryData = journalDatabase.GetEntryAsset(topicEntryID);
                    if (entryData != null)
                        entries.Add(entryData);
                }


            return entries;
        }

        #region State

        Dictionary<string, JournalEntryInstance> _entryInstances = new();
        Dictionary<string, JournalTopicInstance> _topicInstances = new();

        #endregion
    }
}
