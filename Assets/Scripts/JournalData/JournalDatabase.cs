using System.Collections.Generic;
using System.Linq;
using JournalData.JournalEntries;
using JournalData.JournalTopics;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace JournalData
{
    [CreateAssetMenu(fileName = "JournalDatabase", menuName = "Scriptable Objects/Journal/Journal Database")]
    public class JournalDatabase : SerializedScriptableObject
    {
        Dictionary<string, JournalTopic> _topicLookup;
        Dictionary<string, JournalEntry> _entryLookup;

        [FormerlySerializedAs("items")] [LabelText("All Journal Topics from Resources/Journal/Topics)")] [ReadOnly]
        // so you don't edit directly — list is managed automatically
        public List<JournalTopic> topics = new();

        [LabelText("All Journal Entries from Resources/Journal/Entries)")] [ReadOnly]
        // so you don't edit directly — list is managed automatically
        public List<JournalEntry> entries = new();

        void OnEnable()
        {
            AutoPopulateFromResources();
            BuildLookup();
        }

        [Button("Auto-Populate From Resources", ButtonSizes.Large)]
        void AutoPopulateFromResources()
        {
            // Load all InventoryItem assets in Resources/Items (including subfolders)
            topics = Resources.LoadAll<JournalTopic>("Journal/Topics")
                .Where(topic => topic != null)
                .OrderBy(topic => topic.UniqueID)
                .ToList();

            entries = Resources.LoadAll<JournalEntry>("Journal/Entries")
                .Where(entry => entry != null)
                .OrderBy(entry => entry.UniqueID)
                .ToList();
        }

        [Button("Rebuild Lookup", ButtonSizes.Small)]
        void BuildLookup()
        {
            _topicLookup = new Dictionary<string, JournalTopic>();
            foreach (var topic in topics)
            {
                if (_topicLookup.ContainsKey(topic.UniqueID)) continue;

                _topicLookup[topic.UniqueID] = topic;
            }

            _entryLookup = new Dictionary<string, JournalEntry>();
            foreach (var entry in entries)
            {
                if (_entryLookup.ContainsKey(entry.UniqueID)) continue;

                _entryLookup[entry.UniqueID] = entry;
            }
        }

        public JournalTopic GetTopicAsset(string uniqueID)
        {
            if (_topicLookup == null || _topicLookup.Count != topics.Count)
                BuildLookup();

            _topicLookup.TryGetValue(uniqueID, out var topic);
            return topic;
        }

        public JournalEntry GetEntryAsset(string uniqueID)
        {
            if (_entryLookup == null || _entryLookup.Count != entries.Count) BuildLookup();

            if (_entryLookup != null)
            {
                _entryLookup.TryGetValue(uniqueID, out var entry);
                return entry;
            }

            Debug.LogWarning("No entry found for " + uniqueID);
            return null;
        }
    }
}
