using System;
using System.Collections;
using System.Collections.Generic;
using CompassNavigatorPro;
using Events;
using Helpers.Events;
using Helpers.Events.ManagerEvents;
using Helpers.Interfaces;
using Helpers.Wrappers;
using MoreMountains.Tools;
using Objectives;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager.SceneManagers
{
    public class CoreGamePOIManager : MonoBehaviour, ICoreGameService, MMEventListener<GamePOIEvent>,
        MMEventListener<ScannerEvent>
    {
        #region POI State Management

        void AddPOIWellKnown(string uniqueId, bool wellKnown, string sceneName = null)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;

            if (!wellKnown)
            {
                // Optional: support "un-know" if you ever need it
                if (_poisWellKnown.Remove(uniqueId) && !string.IsNullOrEmpty(sceneName)
                                                    && _poisWellKnownByScene.TryGetValue(sceneName, out var set))
                    set.Remove(uniqueId);

                MarkDirty();
                ConditionalSave();
                return;
            }

            // Add globally
            if (_poisWellKnown.Add(uniqueId))
            {
                MarkDirty();
                ConditionalSave();
            }

            if (!string.IsNullOrEmpty(sceneName))
            {
                if (!_poisWellKnownByScene.TryGetValue(sceneName, out var set))
                {
                    set = new HashSet<string>();
                    _poisWellKnownByScene[sceneName] = set;
                }

                set.Add(uniqueId);
            }
        }

        void AddPOILittleKnown(string uniqueId, bool littleKnown, string sceneName = null)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;

            if (!littleKnown)
            {
                // Optional: support "un-know" if you ever need it
                if (_poisLittleKnown.Remove(uniqueId) && !string.IsNullOrEmpty(sceneName)
                                                      && _poisLittleKnownByScene.TryGetValue(sceneName, out var set))
                    set.Remove(uniqueId);

                MarkDirty();
                ConditionalSave();
                return;
            }

            // Add globally
            if (_poisLittleKnown.Add(uniqueId))
            {
                MarkDirty();
                ConditionalSave();
            }

            if (!string.IsNullOrEmpty(sceneName))
            {
                if (!_poisLittleKnownByScene.TryGetValue(sceneName, out var set))
                {
                    set = new HashSet<string>();
                    _poisLittleKnownByScene[sceneName] = set;
                }

                set.Add(uniqueId);
            }
        }


        void AddPOIAlwaysVisible(string uniqueId, bool alwaysVisible, string sceneName = null)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;

            if (!alwaysVisible)
            {
                // Optional: support "un-know" if you ever need it
                if (_poisAlwaysVisible.Remove(uniqueId) && !string.IsNullOrEmpty(sceneName)
                                                        && _poisAlwaysVisibleByScene.TryGetValue(
                                                            sceneName,
                                                            out var set))
                    set.Remove(uniqueId);

                MarkDirty();
                ConditionalSave();
                return;
            }

            // Add globally
            if (_poisAlwaysVisible.Add(uniqueId))
            {
                MarkDirty();
                ConditionalSave();
            }

            if (!string.IsNullOrEmpty(sceneName))
            {
                if (!_poisAlwaysVisibleByScene.TryGetValue(sceneName, out var set))
                {
                    set = new HashSet<string>();
                    _poisAlwaysVisibleByScene[sceneName] = set;
                }

                set.Add(uniqueId);
                _poisAlwaysVisible.Add(uniqueId);
                POIWrapperEvent.Trigger(uniqueId, sceneName, POIWrapperEventType.VisibilityChanged);
            }
        }
        void MarkPOIAsHavingNewContent(string uniqueId, bool hasNewContent, string sceneName = null)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;

            if (!hasNewContent)
            {
                // Optional: support "un-know" if you ever need it
                if (_poisWithNewContent.Remove(uniqueId) && !string.IsNullOrEmpty(sceneName)
                                                         && _poisWithNewContentByScene.TryGetValue(
                                                             sceneName, out var set))
                    set.Remove(uniqueId);

                MarkDirty();
                ConditionalSave();
                return;
            }

            // Add globally
            if (_poisWithNewContent.Add(uniqueId))
            {
                MarkDirty();
                ConditionalSave();
            }

            if (!string.IsNullOrEmpty(sceneName))
            {
                if (!_poisWithNewContentByScene.TryGetValue(sceneName, out var set))
                {
                    set = new HashSet<string>();
                    _poisWithNewContentByScene[sceneName] = set;
                }

                set.Add(uniqueId);
                _poisWithNewContent.Add(uniqueId);
            }

            POIWrapperEvent.Trigger(uniqueId, sceneName, POIWrapperEventType.StateChanged);
        }

        // void MarkPOIAsTrackedByObjective(string uniqueId, bool isTracked, string sceneName = null)
        // {
        //     if (string.IsNullOrEmpty(uniqueId)) return;
        //
        //     // This is a placeholder for potential future functionality
        //     // Currently, we do not maintain a separate tracked state
        //     // Set POI to always visible, CanBeVisited=true, hasBeenVisited=false
        //     var poiWrapper = GetPOIWrapper(uniqueId);
        //     AddPOIAlwaysVisible(uniqueId, isTracked, sceneName);
        //
        //     poiWrapper.SetPOIVisitable(isTracked, sceneName);
        //
        //
        //     MarkDirty();
        //     ConditionalSave();
        // }

        void MarkPOIAsTrackedByObjective(string uniqueId, bool isTracked, string sceneName = null)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;
            var poiWrapper = GetPOIWrapper(uniqueId);
            if (poiWrapper == null) return;

            if (isTracked)
            {
                _poisTrackedByObjective.Add(uniqueId);
                if (!string.IsNullOrEmpty(sceneName))
                {
                    if (!_poisTrackedByObjectiveByScene.TryGetValue(sceneName, out var set))
                        _poisTrackedByObjectiveByScene[sceneName] = set = new HashSet<string>();

                    set.Add(uniqueId);
                }

                // Keep always visible, mark visitable
                AddPOIAlwaysVisible(uniqueId, true, sceneName);
                poiWrapper.SetPOIVisitable(true, sceneName);
                POIWrapperEvent.Trigger(uniqueId, sceneName, POIWrapperEventType.TrackedByObjective);
            }
            else
            {
                _poisTrackedByObjective.Remove(uniqueId);
                if (!string.IsNullOrEmpty(sceneName) &&
                    _poisTrackedByObjectiveByScene.TryGetValue(sceneName, out var set))
                    set.Remove(uniqueId);

                // If it wasn’t explicitly “always visible” otherwise, restore usual visibility
                if (!IsPOIAlwaysVisible(uniqueId)) poiWrapper.SetPOIVisitable(false, sceneName);
                POIWrapperEvent.Trigger(uniqueId, sceneName, POIWrapperEventType.Untracked);
            }

            MarkDirty();
            ConditionalSave();
        }

        #endregion


        #region Fields & Inspector

        [Tooltip(
            "If true, the manager will save automatically when it becomes dirty. Leave false for explicit, checkpoint-only saving.")]
        [SerializeField]
        bool autoSave;

        [SerializeField] CompassPro compassProGlobal;

        [Header("Visibility")]
        // These POIs are always visible, regardless of knowledge state. They are linked to
        // an objective or are vital to have HUD visibility of.
        readonly HashSet<string> _poisAlwaysVisible = new();

        readonly Dictionary<string, HashSet<string>> _poisAlwaysVisibleByScene = new(StringComparer.Ordinal);

        [Header("POIs Little Known")]
        // These POIs we know exist but do not know well and but have not yet examined their associated Examinable.
        readonly HashSet<string> _poisLittleKnown = new();

        readonly Dictionary<string, HashSet<string>> _poisLittleKnownByScene = new(StringComparer.Ordinal);

        [Header("POIs Well Known")]
        // These POIs are either already known or have been scanned but not yet examined.
        readonly HashSet<string> _poisWellKnown = new();

        // These POIs we have full information about.
        readonly Dictionary<string, HashSet<string>> _poisWellKnownByScene = new(StringComparer.Ordinal);

        readonly HashSet<string> _poisWithNewContent = new(StringComparer.Ordinal);

        readonly Dictionary<string, HashSet<string>> _poisWithNewContentByScene = new(StringComparer.Ordinal);

        // Fast lookup of currently loaded/active wrappers
        readonly Dictionary<string, GamePOIWrapper> _wrappersById = new(StringComparer.Ordinal);

        readonly HashSet<string> _poisTrackedByObjective = new();
        readonly Dictionary<string, HashSet<string>> _poisTrackedByObjectiveByScene = new(StringComparer.Ordinal);


        bool _dirty;

        int _numberOfPOIsJustScanned;

        string _savePath;


        public static CoreGamePOIManager Instance { get; private set; }

        #endregion

        #region Events

        public void OnMMEvent(GamePOIEvent eventType)
        {
            if (eventType.GamePOIEventTypeValue == GamePOIEventType.MakeAlwaysVisible)
            {
                var uniqueId = eventType.UniqueId;
                if (string.IsNullOrEmpty(uniqueId))
                {
                    Debug.LogError(
                        "[GamePOIManager] POIWasAreaScanned event received with null or empty uniqueId.");

                    return;
                }

                var poiWrapper = GetPOIWrapper(uniqueId);
                if (poiWrapper == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated wrapper.");

                    return;
                }

                var poi = poiWrapper.compassProPOI;
                if (poi == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated CompassProPOI.");

                    return;
                }


                if (poiWrapper == null || poi == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated wrapper or POI.");

                    return;
                }

                var sceneName = poiWrapper.gameObject.scene.name;

                if (string.IsNullOrEmpty(sceneName))
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated scene name.");

                    return;
                }

                RecordAndChangeVisibility(uniqueId, POIVisibility.AlwaysVisible, sceneName, poi);
            }

            if (eventType.GamePOIEventTypeValue == GamePOIEventType.MarkPOIAsTrackedByObjective)
                MarkPOIAsTrackedByObjective(eventType.UniqueId, true, eventType.SceneName);

            if (eventType.GamePOIEventTypeValue == GamePOIEventType.UnmarkPOIAsTrackedByObjective)
                MarkPOIAsTrackedByObjective(eventType.UniqueId, false, eventType.SceneName);

            if (eventType.GamePOIEventTypeValue == GamePOIEventType.POIWasAreaScanned)
            {
                _numberOfPOIsJustScanned++;

                var uniqueId = eventType.UniqueId;


                var poiWrapper = GetPOIWrapper(uniqueId);
                if (string.IsNullOrEmpty(uniqueId))
                {
                    Debug.LogError(
                        "[GamePOIManager] POIWasAreaScanned event received with null or empty uniqueId.");

                    return;
                }

                if (poiWrapper == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIWasAreaScanned event received but could not find associated wrapper.");

                    return;
                }

                if (poiWrapper == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIWasAreaScanned event received but could not find associated CompassProPOI.");

                    return;
                }

                var sceneName = poiWrapper.gameObject.scene.name;
                if (poiWrapper.examinationPolicy == GamePOIWrapper.ExaminationPolicy.Required)
                    AddPOILittleKnown(uniqueId, true, sceneName);
                else if (poiWrapper.examinationPolicy == GamePOIWrapper.ExaminationPolicy.NonRequired)
                    AddPOIWellKnown(uniqueId, true, sceneName);

                var poi = poiWrapper.compassProPOI;

                if (poi == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIWasAreaScanned event received but could not find associated CompassProPOI.");

                    return;
                }


                if (poiWrapper.revealPolicy == GamePOIWrapper.RevealPolicy.Brief)
                    StartCoroutine(BrieflyVisualizeScannedPOI(poi));
                else if (poiWrapper.revealPolicy == GamePOIWrapper.RevealPolicy.StayVisible)
                    // AddPOIAlwaysVisible(uniqueId, true, sceneName);
                    //
                    // poi.visibility = POIVisibility.AlwaysVisible;
                    // poi.showOnScreenIndicator = true;
                    RecordAndChangeVisibility(uniqueId, POIVisibility.AlwaysVisible, sceneName, poi);
            }
            else if (eventType.GamePOIEventTypeValue == GamePOIEventType.POIMarkedAsHavingNewContent)
            {
                var uniqueId = eventType.UniqueId;
                if (string.IsNullOrEmpty(uniqueId))
                {
                    Debug.LogError(
                        "[GamePOIManager] POIWasAreaScanned event received with null or empty uniqueId.");

                    return;
                }

                var poiWrapper = GetPOIWrapper(uniqueId);
                if (poiWrapper == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated wrapper.");

                    return;
                }

                var poi = poiWrapper.compassProPOI;
                if (poi == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated CompassProPOI.");

                    return;
                }


                if (poiWrapper == null || poi == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated wrapper or POI.");

                    return;
                }

                var sceneName = poiWrapper.gameObject.scene.name;

                if (string.IsNullOrEmpty(sceneName))
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated scene name.");

                    return;
                }

                MarkPOIAsHavingNewContent(uniqueId, true, sceneName);

                poi.canBeVisited = true;
                // poi.iconScale = 1.2f; -> same as compassCurrentIconScale
                // poi.compassCurrentIconScale = 1.2f;
                poi.onScreenIndicatorScale = 1.2f;
                poi.showOnScreenIndicator = true;
                poi.visibility = POIVisibility.AlwaysVisible;

                Debug.Log("[GamePOIManager] POI marked as having new content: " + uniqueId);
            }
            else if (eventType.GamePOIEventTypeValue == GamePOIEventType.POIWithNewContentMarkedAsVisited)
            {
                var uniqueId = eventType.UniqueId;
                if (string.IsNullOrEmpty(uniqueId))
                {
                    Debug.LogError(
                        "[GamePOIManager] POIWasAreaScanned event received with null or empty uniqueId.");

                    return;
                }

                var poiWrapper = GetPOIWrapper(uniqueId);
                if (poiWrapper == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated wrapper.");

                    return;
                }

                var poi = poiWrapper.compassProPOI;
                if (poi == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated CompassProPOI.");

                    return;
                }


                if (poiWrapper == null || poi == null)
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated wrapper or POI.");

                    return;
                }

                var sceneName = poiWrapper.gameObject.scene.name;

                if (string.IsNullOrEmpty(sceneName))
                {
                    Debug.LogError(
                        "[GamePOIManager] POIMarkedAsHavingNewContent event received but could not find associated scene name.");

                    return;
                }

                MarkPOIAsHavingNewContent(uniqueId, false, sceneName);

                poi.canBeVisited = false;
                poi.onScreenIndicatorScale = 0.4f;

                if (!IsPOIAlwaysVisible(uniqueId))
                {
                    poi.visibility = POIVisibility.AlwaysHidden;
                    poi.showOnScreenIndicator = false;
                }

                Debug.Log("[GamePOIManager] POI marked as having no new content: " + uniqueId);

                var objectives = ObjectivesManager.Instance.GetActiveObjectives();

                foreach (var objective in objectives)
                {
                    var obj = ObjectivesManager.Instance.GetObjectiveById(objective);
                    if (obj != null && obj.GetPOIUniqueId() == uniqueId)
                    {
                        // Fulfill the objective
                        ObjectiveEvent.Trigger(obj.objectiveId, ObjectiveEventType.ObjectiveCompleted);
                        break;
                    }
                }
            }
        }
        void RecordAndChangeVisibility(string uniqueId, POIVisibility visibility, string sceneName, CompassProPOI poi)
        {
            if (visibility == POIVisibility.AlwaysVisible)
            {
                AddPOIAlwaysVisible(uniqueId, true, sceneName);

                poi.visibility = POIVisibility.AlwaysVisible;
                poi.showOnScreenIndicator = true;
            }
        }


        IEnumerator BrieflyVisualizeScannedPOI(CompassProPOI poi, float duration = 7f)
        {
            if (compassProGlobal == null || poi == null) yield break;

            var previousVisibility = poi.visibility;
            var previousShowOnScreenIndicator = poi.showOnScreenIndicator;

            poi.visibility = POIVisibility.AlwaysVisible;
            poi.showOnScreenIndicator = true;


            // Wait for the duration
            yield return new WaitForSeconds(duration);

            // Restore previous visibility
            poi.visibility = previousVisibility;
            poi.showOnScreenIndicator = previousShowOnScreenIndicator;


            Debug.Log("[GamePOIManager] Ended briefly visualizing scanned POI: " + poi.name);
        }

        public void OnMMEvent(ScannerEvent eventType)
        {
            if (eventType.ScannerEventType == ScannerEventType.ScanEnded)
                if (_numberOfPOIsJustScanned > 0)
                {
                    Debug.Log("[GamePOIManager] Scan ended, " + _numberOfPOIsJustScanned + " POIs were just scanned.");
                    _numberOfPOIsJustScanned = 0;
                }
        }

        #endregion

        #region Wrappers Management

        public void RegisterWrapper(GamePOIWrapper wrapper)
        {
            if (wrapper == null || string.IsNullOrEmpty(wrapper.UniqueID)) return;

            // Warn on duplicate IDs to catch prefab copy/paste mistakes
            if (_wrappersById.TryGetValue(wrapper.UniqueID, out var existing) && existing != null &&
                existing != wrapper)
                // Debug.Log(
                //     $"[GamePOIManager] Duplicate POI uniqueId '{wrapper.UniqueID}'. " +
                //     $"Existing={existing.name} New={wrapper.name}");

                _wrappersById[wrapper.UniqueID] = wrapper;
        }

        public void UnregisterWrapper(GamePOIWrapper wrapper)
        {
            if (wrapper == null || string.IsNullOrEmpty(wrapper.UniqueID)) return;
            if (_wrappersById.TryGetValue(wrapper.UniqueID, out var existing) && existing == wrapper)
                _wrappersById.Remove(wrapper.UniqueID);
        }

        public GamePOIWrapper GetPOIWrapper(string uniqueId)
        {
            if (string.IsNullOrEmpty(uniqueId)) return null;

            // Fast path (most calls)
            if (_wrappersById.TryGetValue(uniqueId, out var wrapper) && wrapper != null)
                return wrapper;

            // Slow fallback: scan currently loaded scenes, including inactive objects
            // This covers edge cases (e.g., instantiated inactive, scene just loaded)
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                var roots = scene.GetRootGameObjects();
                foreach (var root in roots)
                {
                    var found = root.GetComponentsInChildren<GamePOIWrapper>(true);
                    foreach (var f in found)
                    {
                        // Lazily rehydrate the cache
                        RegisterWrapper(f);
                        if (f.UniqueID == uniqueId) return f;
                    }
                }
            }

            return null;
        }

        #endregion


        #region LifeCycle

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

            // if (!HasSavedData())
            // {
            //     Load();
            // }
            // else
            // {
            //     Debug.Log("[GamePOIManager] No save file found, forcing initial reset...");
            //     Reset();
            // }
            //
            // Load();
            if (HasSavedData())
            {
                Load();
            }
            else
            {
                Debug.Log("[GamePOIManager] No save file found, forcing initial reset...");
                Reset();
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<ScannerEvent>();
            this.MMEventStartListening<GamePOIEvent>();


            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        void OnDisable()
        {
            this.MMEventStopListening<ScannerEvent>();
            this.MMEventStopListening<GamePOIEvent>();

            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            if (!scene.isLoaded) return;
            foreach (var root in scene.GetRootGameObjects())
            foreach (var w in root.GetComponentsInChildren<GamePOIWrapper>(true))
                RegisterWrapper(w);
        }

        void OnSceneUnloaded(Scene scene)
        {
            // Conservative cleanup: remove any wrappers that belonged to this scene
            // (Wrappers also unregister in OnDisable/OnDestroy, so this is just extra safety)
        }

        #endregion

        #region IGameService

        public void Save()
        {
            var path = GetSaveFilePath();

            ES3.Save("POIsAlwaysVisible", _poisAlwaysVisible, path);
            ES3.Save("POIsAlwaysVisibleByScene", _poisAlwaysVisibleByScene, path);
            ES3.Save("POIsLittleKnown", _poisLittleKnown, path);
            ES3.Save("POIsLittleKnownByScene", _poisLittleKnownByScene, path);
            ES3.Save("POIsWellKnown", _poisWellKnown, path);
            ES3.Save("POIsWellKnownByScene", _poisWellKnownByScene, path);
            ES3.Save("POIsWithNewContent", _poisWithNewContent, path);
            ES3.Save("POIsWithNewContentByScene", _poisWithNewContentByScene, path);
            ES3.Save("POIsTrackedByObjective", _poisTrackedByObjective, path);
            ES3.Save("POIsTrackedByObjectiveByScene", _poisTrackedByObjectiveByScene, path);

            _dirty = false;
        }

        public void Load()
        {
            var path = GetSaveFilePath();

            _poisAlwaysVisible.Clear();
            _poisAlwaysVisibleByScene.Clear();
            _poisLittleKnown.Clear();
            _poisLittleKnownByScene.Clear();
            _poisWellKnown.Clear();
            _poisWellKnownByScene.Clear();
            _poisWithNewContent.Clear();
            _poisWithNewContentByScene.Clear();
            _poisTrackedByObjective.Clear();
            _poisTrackedByObjectiveByScene.Clear();

            if (ES3.KeyExists("POIsAlwaysVisible", path))
            {
                var set = ES3.Load<HashSet<string>>("POIsAlwaysVisible", path);
                foreach (var id in set) _poisAlwaysVisible.Add(id);
            }

            if (ES3.KeyExists("POIsAlwaysVisibleByScene", path))
            {
                var dict = ES3.Load<Dictionary<string, HashSet<string>>>("POIsAlwaysVisibleByScene", path);
                foreach (var kv in dict)
                    _poisAlwaysVisibleByScene[kv.Key] = new HashSet<string>(kv.Value);
            }

            if (ES3.KeyExists("POIsLittleKnown", path))
            {
                var set = ES3.Load<HashSet<string>>("POIsLittleKnown", path);
                foreach (var id in set) _poisLittleKnown.Add(id);
            }

            if (ES3.KeyExists("POIsLittleKnownByScene", path))
            {
                var dict = ES3.Load<Dictionary<string, HashSet<string>>>("POIsLittleKnownByScene", path);
                foreach (var kv in dict)
                    _poisLittleKnownByScene[kv.Key] = new HashSet<string>(kv.Value);
            }

            if (ES3.KeyExists("POIsWellKnown", path))
            {
                var set = ES3.Load<HashSet<string>>("POIsWellKnown", path);
                foreach (var id in set) _poisWellKnown.Add(id);
            }

            if (ES3.KeyExists("POIsWellKnownByScene", path))
            {
                var dict = ES3.Load<Dictionary<string, HashSet<string>>>("POIsWellKnownByScene", path);
                foreach (var kv in dict)
                    _poisWellKnownByScene[kv.Key] = new HashSet<string>(kv.Value);
            }

            if (ES3.KeyExists("POIsWithNewContent", path))
            {
                var set = ES3.Load<HashSet<string>>("POIsWithNewContent", path);
                foreach (var id in set) _poisWithNewContent.Add(id);
            }

            if (ES3.KeyExists("POIsWithNewContentByScene", path))
            {
                var dict = ES3.Load<Dictionary<string, HashSet<string>>>("POIsWithNewContentByScene", path);
                foreach (var kv in dict)
                    _poisWithNewContentByScene[kv.Key] = new HashSet<string>(kv.Value);
            }

            if (ES3.KeyExists("POIsTrackedByObjective", path))
            {
                var set = ES3.Load<HashSet<string>>("POIsTrackedByObjective", path);
                foreach (var id in set) _poisTrackedByObjective.Add(id);
            }

            if (ES3.KeyExists("POIsTrackedByObjectiveByScene", path))
            {
                var dict = ES3.Load<Dictionary<string, HashSet<string>>>("POIsTrackedByObjectiveByScene", path);
                foreach (var kv in dict)
                    _poisTrackedByObjectiveByScene[kv.Key] = new HashSet<string>(kv.Value);
            }
        }

        public void Reset()
        {
            _poisAlwaysVisible.Clear();
            _poisAlwaysVisibleByScene.Clear();
            _poisLittleKnown.Clear();
            _poisLittleKnownByScene.Clear();
            _poisWellKnown.Clear();
            _poisWellKnownByScene.Clear();
            _poisWithNewContent.Clear();
            _poisWithNewContentByScene.Clear();
            _poisTrackedByObjective.Clear();
            _poisTrackedByObjectiveByScene.Clear();
            _dirty = true;
            ConditionalSave();
        }


        public string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.GamePOISave);
        }

        #endregion

        #region Public API

        public bool IsPOIAlwaysVisible(string uniqueId)
        {
            return _poisAlwaysVisible.Contains(uniqueId);
        }

        public bool IsPOILittleKnown(string uniqueId)
        {
            return _poisLittleKnown.Contains(uniqueId);
        }

        public bool IsPOIWellKnown(string uniqueId)
        {
            return _poisWellKnown.Contains(uniqueId);
        }

        public bool IsPOITrackedByObjective(string uniqueId)
        {
            return _poisTrackedByObjective.Contains(uniqueId);
        }


        public bool IsPOIAlwaysVisibleInScene(string uniqueId, string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) && _poisAlwaysVisibleByScene.TryGetValue(sceneName, out var set) &&
                   set.Contains(uniqueId);
        }

        public bool IsPOILittleKnownInScene(string uniqueId, string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) && _poisLittleKnownByScene.TryGetValue(sceneName, out var set) &&
                   set.Contains(uniqueId);
        }

        public bool IsPOIWellKnownInScene(string uniqueId, string sceneName)
        {
            return !string.IsNullOrEmpty(sceneName) && _poisWellKnownByScene.TryGetValue(sceneName, out var set) &&
                   set.Contains(uniqueId);
        }

        public bool DoesPOIHaveNewContent(string uniqueId)
        {
            return _poisWithNewContent.Contains(uniqueId);
        }

        public IEnumerable GetWrappersOfType(CanBeAreaScannedType type)
        {
            // Iterate the live cache and yield only matching wrappers
            foreach (var kv in _wrappersById)
            {
                var w = kv.Value;
                if (w != null && w.HasScannerCapability(type))
                    yield return w; // non-generic IEnumerable is fine here
            }
        }


        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        #endregion

        #region Internals

        public void ConditionalSave()
        {
            if (autoSave && _dirty) Save();
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        #endregion
    }
}
