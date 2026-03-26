using System;
using System.Collections.Generic;
using Helpers.Events;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.SceneManagers.Pickable
{
    public class PickableLocationManager : MonoBehaviour, ICoreGameService, MMEventListener<PickableLocationEvent>
    {
        readonly Dictionary<string, Dictionary<string, Vector3>> _initialRotationBySceneDictionary =
            new(StringComparer.Ordinal);

        readonly Dictionary<string, Vector3> _initialRotationDictionary = new(StringComparer.Ordinal);
        readonly Dictionary<string, Dictionary<string, Vector3>> _itemLocationBySceneDictionary =
            new(StringComparer.Ordinal);

        readonly Dictionary<string, Vector3> _itemLocationDictionary = new(StringComparer.Ordinal);
        bool _dirty;
        PickableManager _pickableManager;
        string _savePath;

        bool autoSave;

        public static PickableLocationManager Instance { get; private set; }

        #region Event Handling

        public void OnMMEvent(PickableLocationEvent eventType)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region Lifecycle

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        void Start()
        {
            _pickableManager = PickableManager.Instance;

            _savePath = GetSaveFilePath();

            if (!HasSavedData())
                //Debug.Log("");
                Reset();

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

        #endregion

        #region IGameService

        public void Save()
        {
            var path = GetSaveFilePath();

            ES3.Save("ItemsLocationsDictionary", _itemLocationDictionary, path);
            ES3.Save("ItemsLocationsBySceneDictionary", _itemLocationBySceneDictionary, path);

            ES3.Save("InitialRotationDictionary", _initialRotationDictionary, path);
            ES3.Save("InitialRotationBySceneDictionary", _initialRotationBySceneDictionary, path);

            _dirty = false;
        }

        public void Load()
        {
            var path = GetSaveFilePath();

            _itemLocationDictionary.Clear();
            _itemLocationBySceneDictionary.Clear();

            _initialRotationDictionary.Clear();
            _initialRotationBySceneDictionary.Clear();

            if (ES3.KeyExists("ItemsLocationsDictionary", path))
            {
                var dict = ES3.Load<Dictionary<string, Vector3>>("ItemsLocationsDictionary", path);
                foreach (var kvp in dict) _itemLocationDictionary[kvp.Key] = kvp.Value;
            }

            if (ES3.KeyExists("ItemsLocationsBySceneDictionary", path))
            {
                var dict = ES3.Load<Dictionary<string, Dictionary<string, Vector3>>>(
                    "ItemsLocationsBySceneDictionary", path);

                foreach (var kvp in dict) _itemLocationBySceneDictionary[kvp.Key] = kvp.Value;
            }

            if (ES3.KeyExists("InitialRotationDictionary", path))
            {
                var dict = ES3.Load<Dictionary<string, Vector3>>("InitialRotationDictionary", path);
                foreach (var kvp in dict) _initialRotationDictionary[kvp.Key] = kvp.Value;
            }

            if (ES3.KeyExists("InitialRotationBySceneDictionary", path))
            {
                var dict = ES3.Load<Dictionary<string, Dictionary<string, Vector3>>>(
                    "InitialRotationBySceneDictionary",
                    path);

                foreach (var kvp in dict) _initialRotationBySceneDictionary[kvp.Key] = kvp.Value;
            }

            _dirty = false;
        }

        public void Reset()
        {
            _itemLocationBySceneDictionary.Clear();
            _itemLocationDictionary.Clear();
            _initialRotationBySceneDictionary.Clear();
            _initialRotationDictionary.Clear();

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
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.PickableLocationSave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        #endregion
    }
}
