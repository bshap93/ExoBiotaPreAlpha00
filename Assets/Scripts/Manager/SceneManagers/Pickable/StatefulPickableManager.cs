using System;
using System.Collections.Generic;
using Helpers.Interfaces;
using UnityEngine;

namespace Manager.SceneManagers.Pickable
{
    [Serializable]
    public struct StatefulItemData
    {
        public string stateType; // e.g. "RhizomicCoreState"
        public int stateValue; // Enum index (int)
    }

    public class StatefulPickableManager : MonoBehaviour, ICoreGameService
    {
        [SerializeField] bool autoSave;

        bool _dirty;
        string _savePath;
        Dictionary<string, StatefulItemData> _subtypeStates = new(StringComparer.Ordinal);

        public static StatefulPickableManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            // optional: DontDestroyOnLoad(gameObject);
            else
                Destroy(gameObject);
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData()) Reset();

            Load();
        }
        public void Save()
        {
            _savePath = GetSaveFilePath();
            // Debug.Log($"[StatefulPickableManager] Saving {_subtypeStates.Count} states to {_savePath}");
            // foreach (var kvp in _subtypeStates)
            // Debug.Log($"  - {kvp.Key}: {kvp.Value.stateType} = {kvp.Value.stateValue}");

            ES3.Save("SubtypeStates", _subtypeStates, _savePath);
            _dirty = false;
        }
        public void Load()
        {
            _savePath = GetSaveFilePath();
            _subtypeStates.Clear();
            if (ES3.KeyExists("SubtypeStates", _savePath))
                _subtypeStates = ES3.Load<Dictionary<string, StatefulItemData>>("SubtypeStates", _savePath);

            else
                Debug.Log($"[StatefulPickableManager] No saved data found at {_savePath}");

            _dirty = false;
        }
        public void Reset()
        {
            _subtypeStates.Clear();
            _dirty = true;
            ConditionalSave();
        }
        public void ConditionalSave()
        {
            if (autoSave && _dirty)
                Save();
        }
        public void MarkDirty()
        {
            _dirty = true;
        }
        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.StatefulPickables);
        }
        public void CommitCheckpointSave()
        {
            if (_dirty)
                Save();
        }
        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void SetState<T>(string uniqueId, T state) where T : Enum
        {
            var data = new StatefulItemData
            {
                stateType = typeof(T).Name,
                stateValue = Convert.ToInt32(state)
            };

            _subtypeStates[uniqueId] = data;


            MarkDirty();
        }

        public bool TryGetState<T>(string uniqueId, out T state) where T : Enum
        {
            if (_subtypeStates.TryGetValue(uniqueId, out var data) &&
                data.stateType == typeof(T).Name)
            {
                state = (T)Enum.ToObject(typeof(T), data.stateValue);
                return true;
            }

            state = default;

            return false;
        }
        public static string[] GetAllStateCategories()
        {
            // In a real implementation, this might query a database or configuration file
            return new[] { "RhizomicCore" };
        }
    }
}
