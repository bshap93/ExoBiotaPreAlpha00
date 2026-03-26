using System;
using System.Collections.Generic;
using Helpers.Events;
using Helpers.Events.Domains.Player.Events;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

// DestructableEvent

// ResetDataEvent if you have one

namespace Manager.SceneManagers
{
    /// <summary>
    ///     Global, modular DestructableManager.
    ///     - Singleton + DontDestroyOnLoad
    ///     - IGameService: Save/Load/Reset/GetSaveFilePath()
    ///     - Modular: global HashSet + per-scene index
    ///     - Explicit saves (only when Save() is called) unless autoSave is enabled.
    /// </summary>
    public class DestructableManager : MonoBehaviour, MMEventListener<DestructableEvent>, ICoreGameService
    {
        [SerializeField] bool autoSave;

        // Global membership: ever destroyed?
        readonly HashSet<string> _destroyed = new();

        // Per-scene shard for modularity
        readonly Dictionary<string, HashSet<string>> _destroyedByScene = new(StringComparer.Ordinal);

        readonly HashSet<string> _emerged = new();

        bool _dirty;
        string _savePath;
        public static DestructableManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
                Instance = this;
            // if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
            //     DontDestroyOnLoad(gameObject);
            else
                Destroy(gameObject);
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("[DestructableManager] No save file found; starting with defaults.");
                Reset();
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

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        #region IGameService

        public void Save()
        {
            var path = GetSaveFilePath();
            ES3.Save("Destructables", _destroyed, path);
            ES3.Save("DestructablesByScene", _destroyedByScene, path);

            // Back-compat keys (optional once)
            foreach (var id in _destroyed)
                ES3.Save(id, true, path);

            _dirty = false;
        }

        public void Load()
        {
            var path = GetSaveFilePath();

            _destroyed.Clear();
            _destroyedByScene.Clear();

            if (ES3.KeyExists("Destructables", path))
            {
                var set = ES3.Load<HashSet<string>>("Destructables", path);
                foreach (var id in set) _destroyed.Add(id);
            }
            else if (ES3.FileExists(path))
            {
                // Old per-key boolean format
                foreach (var key in ES3.GetKeys(path))
                    if (ES3.KeyExists(key, path) && ES3.Load<bool>(key, path))
                        _destroyed.Add(key);
            }

            if (ES3.KeyExists("DestructablesByScene", path))
            {
                var dict = ES3.Load<Dictionary<string, HashSet<string>>>("DestructablesByScene", path);
                foreach (var kv in dict)
                    _destroyedByScene[kv.Key] = new HashSet<string>(kv.Value);
            }

            _dirty = false;
        }

        public void Reset()
        {
            _destroyed.Clear();
            _destroyedByScene.Clear();
            _dirty = true;
            ConditionalSave();
        }

        public string GetSaveFilePath()
        {
            // Mirror your global save path scheme (adjust enum/type as needed)
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.DestructablesSave);
        }

        #endregion

        #region Events

        public void OnMMEvent(DestructableEvent e)
        {
            if (e.EventType != DestructableEventType.Destroyed) return;

            var id = e.UniqueID;
            if (string.IsNullOrEmpty(id)) return;

            // Scene name is provided via expanded event (see section 2)
            var sceneName = e.ItemTransform ? e.ItemTransform.gameObject.scene.name : string.Empty;

            var newlyAdded = _destroyed.Add(id);

            if (!string.IsNullOrEmpty(sceneName))
            {
                if (!_destroyedByScene.TryGetValue(sceneName, out var set))
                {
                    set = new HashSet<string>();
                    _destroyedByScene[sceneName] = set;
                }

                set.Add(id);
                Debug.Log($"[DestructableManager] Added {id} in scene {sceneName}");
            }

            if (newlyAdded)
            {
                MarkDirty();
                ConditionalSave();
            }
        }

        public void OnMMEvent(ResetDataEvent _)
        {
            Reset();
        }

        #endregion

        #region Public API

        public bool IsDestroyed(string uniqueId)
        {
            return _destroyed.Contains(uniqueId);
        }

        public bool IsDestroyedInScene(string uniqueId, string sceneName)
        {
            return _destroyedByScene.TryGetValue(sceneName, out var set) && set.Contains(uniqueId);
        }

        /// <summary>
        ///     Back-compat shim for old callers that directly tell the manager an item was destroyed.
        /// </summary>
        public void AddDestructable(string uniqueId, bool destroyed, string sceneName = null)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;

            if (!destroyed)
            {
                if (_destroyed.Remove(uniqueId) && !string.IsNullOrEmpty(sceneName)
                                                && _destroyedByScene.TryGetValue(sceneName, out var set))
                    set.Remove(uniqueId);

                MarkDirty();
                ConditionalSave();
                return;
            }

            var newlyAdded = _destroyed.Add(uniqueId);

            if (!string.IsNullOrEmpty(sceneName))
            {
                if (!_destroyedByScene.TryGetValue(sceneName, out var set))
                {
                    set = new HashSet<string>();
                    _destroyedByScene[sceneName] = set;
                }

                set.Add(uniqueId);
            }

            if (newlyAdded)
            {
                MarkDirty();
                ConditionalSave();
            }
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        #endregion

        #region Internals

        public void MarkDirty()
        {
            _dirty = true;
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }

        #endregion
    }
}
