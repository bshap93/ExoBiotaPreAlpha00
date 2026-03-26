using System.Collections.Generic;
using Helpers.Events.Dialog;
using Helpers.Events.Progression;
using Helpers.Interfaces;
using Manager;
using MoreMountains.Tools;
using Overview.NPC;
using UnityEngine;

namespace FirstPersonPlayer.FPNPCs
{
    public class FriendlyNPCManager : MonoBehaviour, ICoreGameService, MMEventListener<MakeContactWithNPCEvent>,
        MMEventListener<QuestEvent>
    {
        const string NPCsContactedKey = "NPCsContacted";
        const string NPCQuestsStartedKey = "NPCQuestsStarted";
        const string NPCQuestsCompletedKey = "NPCQuestsCompleted";
        public bool autoSave;
        [SerializeField] NpcDatabase npcDatabase;
        bool _dirty;
        string _savePath;

        public static FriendlyNPCManager Instance { get; private set; }

        HashSet<string> NPCQuestsStarted { get; set; } = new();
        HashSet<string> NPCQuestsCompleted { get; set; } = new();
        HashSet<string> NPCsContactedAtLeastOnce { get; set; } = new();

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
            if (!ES3.FileExists(_savePath))
            {
                Debug.Log("[PlayerSaveManager] No save file found, forcing initial save...");
                Reset();
            }

            Load();
        }
        void OnEnable()
        {
            this.MMEventStartListening<QuestEvent>();
            this.MMEventStartListening<MakeContactWithNPCEvent>();
        }
        void OnDisable()
        {
            this.MMEventStopListening<QuestEvent>();
            this.MMEventStopListening<MakeContactWithNPCEvent>();
        }


        public void Save()
        {
            var savePath = GetSaveFilePath();

            ES3.Save(NPCsContactedKey, NPCsContactedAtLeastOnce, savePath);

            ES3.Save(NPCQuestsStartedKey, NPCQuestsStarted, savePath);

            ES3.Save(NPCQuestsCompletedKey, NPCQuestsCompleted, savePath);
        }
        public void Load()
        {
            var savePath = GetSaveFilePath();

            if (!ES3.FileExists(savePath)) return;

            if (ES3.KeyExists(NPCsContactedKey, savePath))
                NPCsContactedAtLeastOnce = ES3.Load<HashSet<string>>(NPCsContactedKey, savePath);
            else
                NPCsContactedAtLeastOnce = new HashSet<string>();

            if (ES3.KeyExists(NPCQuestsStartedKey, savePath))
                NPCQuestsStarted = ES3.Load<HashSet<string>>(NPCQuestsStartedKey, savePath);
            else
                NPCQuestsStarted = new HashSet<string>();

            if (ES3.KeyExists(NPCQuestsCompletedKey, savePath))
                NPCQuestsCompleted = ES3.Load<HashSet<string>>(NPCQuestsCompletedKey, savePath);
            else
                NPCQuestsCompleted = new HashSet<string>();
        }
        public void Reset()
        {
            NPCsContactedAtLeastOnce.Clear();
            NPCQuestsStarted.Clear();
            NPCQuestsCompleted.Clear();

            MarkDirty();
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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.FriendlyNPCSave);
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
        public void OnMMEvent(MakeContactWithNPCEvent eventType)
        {
            NPCsContactedAtLeastOnce.Add(eventType.NPCId);


            MarkDirty();
        }
        public void OnMMEvent(QuestEvent eventType)
        {
            if (eventType.Type == QuestEvent.QuestEventType.Started)
                MarkQuestStarted(eventType.QuestID);
            else if (eventType.Type == QuestEvent.QuestEventType.Completed)
                MarkQuestCompleted(eventType.QuestID);
        }

        public void MarkQuestStarted(string questID)
        {
            NPCQuestsStarted.Add(questID);
            MarkDirty();
        }


        public void MarkQuestCompleted(string questID)
        {
            NPCQuestsCompleted.Add(questID);
            MarkDirty();
        }

        public bool HasQuestBeenStarted(string questID)
        {
            return NPCQuestsStarted.Contains(questID);
        }

        public bool HasQuestBeenCompleted(string questID)
        {
            return NPCQuestsCompleted.Contains(questID);
        }

        public bool HasNPCBeenContactedAtLeastOnce(string npcID)
        {
            return NPCsContactedAtLeastOnce.Contains(npcID);
        }
    }
}
