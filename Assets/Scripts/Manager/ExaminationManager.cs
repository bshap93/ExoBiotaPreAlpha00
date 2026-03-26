using System;
using System.Collections.Generic;
using Helpers.Events;
using Helpers.Interfaces;
using Helpers.ScriptableObjects.IconRepositories;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Serialization;

namespace Manager
{
    public class ExaminationManager : MonoBehaviour, ICoreGameService, MMEventListener<ExaminationEvent>
    {
        [FormerlySerializedAs("DefaultUnknownIcon")]
        public Sprite defaultUnknownIcon;

        public IconRepository iconRepository;
        [SerializeField] bool autoSave; // checkpoint-only by default

        readonly HashSet<string> _biologicalsExamined = new();

        readonly HashSet<string> _itemPickerTypesExamined = new();
        readonly HashSet<string> _oresExamined = new();


        bool _dirty;
        string _savePath;


        public static ExaminationManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
                    DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("[ExaminationManager] No save file found, forcing initial save...");
                Reset(); // Ensure default values are set
            }

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

        public void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("ItemPickerTypesExamined", _itemPickerTypesExamined, path);
            ES3.Save("OresExamined", _oresExamined, path);
            ES3.Save("BiologicalsExamined", _biologicalsExamined, path);

            _dirty = false;
        }

        public void Load()
        {
            var path = GetSaveFilePath();

            _itemPickerTypesExamined.Clear();
            _oresExamined.Clear();
            _biologicalsExamined.Clear();

            if (ES3.KeyExists("ItemPickerTypesExamined", path))
            {
                var set = ES3.Load<HashSet<string>>("ItemPickerTypesExamined", path);
                foreach (var id in set)
                    _itemPickerTypesExamined.Add(id);
            }

            if (ES3.KeyExists("OresExamined", path))
            {
                var set = ES3.Load<HashSet<string>>("OresExamined", path);
                foreach (var id in set)
                    _oresExamined.Add(id);
            }

            if (ES3.KeyExists("BiologicalsExamined", path))
            {
                var set = ES3.Load<HashSet<string>>("BiologicalsExamined", path);
                foreach (var id in set)
                    _biologicalsExamined.Add(id);
            }

            _dirty = false;
        }

        public void Reset()
        {
            _itemPickerTypesExamined.Clear();
            _oresExamined.Clear();
            _biologicalsExamined.Clear();

            _dirty = true;
            ConditionalSave();
        }

        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.ExaminationSave);
        }

        public void CommitCheckpointSave()
        {
            throw new NotImplementedException();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty)
                Save();
        }


        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void OnMMEvent(ExaminationEvent eventType)
        {
            switch (eventType.SceneObjectType)
            {
                case ExaminableItemType.Biological:
                    AddBiologicalsExamined(eventType.Data.Id);
                    break;
                case ExaminableItemType.Pickable:
                    AddItemPickerTypeExamined(eventType.Data.Id);
                    break;
                case ExaminableItemType.Ore:
                    AddOreExamined(eventType.Data.Id);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public bool HasOreBeenExamined(string oreTypeId)
        {
            if (string.IsNullOrEmpty(oreTypeId)) return false;
            return _oresExamined.Contains(oreTypeId);
        }

        public bool HasItemPickerBeenExamined(string itemPickerType)
        {
            if (string.IsNullOrEmpty(itemPickerType)) return false;
            return _itemPickerTypesExamined.Contains(itemPickerType);
        }

        public bool HasBiologicalBeenExamined(string biologicalId)
        {
            if (string.IsNullOrEmpty(biologicalId)) return false;
            return _biologicalsExamined.Contains(biologicalId);
        }

        public void AddBiologicalsExamined(string biologicalId)
        {
            if (string.IsNullOrEmpty(biologicalId)) return;

            if (_biologicalsExamined.Add(biologicalId))
            {
                MarkDirty();
                ConditionalSave();
            }
        }

        public void AddItemPickerTypeExamined(string itemPickerType)
        {
            if (string.IsNullOrEmpty(itemPickerType)) return;

            if (_itemPickerTypesExamined.Add(itemPickerType))
            {
                MarkDirty();
                ConditionalSave();
            }
        }


        public void AddOreExamined(string oreId)
        {
            if (string.IsNullOrEmpty(oreId)) return;

            if (_oresExamined.Add(oreId))
            {
                MarkDirty();
                ConditionalSave();
            }
        }

        public bool HasTypeBeenExamined(string typeId)
        {
            if (string.IsNullOrEmpty(typeId)) return false;
            // however you store examined IDs; unify ores + items if you haven’t already

            return _biologicalsExamined.Contains(typeId) || _itemPickerTypesExamined.Contains(typeId) ||
                   _oresExamined.Contains(typeId);
        }
    }
}
