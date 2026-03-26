using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using Domains.Player.Scripts;
using Helpers.Events;
using Helpers.Events.ManagerEvents;
using Helpers.Interfaces;
using Manager;
using Manager.SceneManagers;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using Objectives.ScriptableObjects;
using UnityEngine;
using UnityEngine.SceneManagement;
using Debug = UnityEngine.Debug;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Objectives
{
    public class ObjectivesManager : MonoBehaviour, ICollectionManager, MMEventListener<ObjectiveEvent>,
        ICoreGameService
    {
#if UNITY_EDITOR
        [SerializeField] DefaultAsset objectivesFolder; // Drag your folder here
#endif
        public List<ObjectiveObject> Objectives = new();

        public MMFeedbacks objectiveCompletedFeedback;

        [SerializeField] bool autoSave; // checkpoint-only by default

        [SerializeField] bool overrideObjectiveProgression;

        [SerializeField] List<ObjectiveObject> objectivesToAddOnStart = new();
        [SerializeField] List<ObjectiveObject> objectivesToActivateOnStart = new();
        [SerializeField] List<ObjectiveObject> objectivesToMarkCompleteOnStart = new();

        [SerializeField] MMFeedbacks addedAndActivatedPunctuatedObjective;

        [SerializeField] MMFeedbacks completedPunctuatedObjectiveFeedback;


        readonly HashSet<string> _processingEvents = new();

        HashSet<string> _activeObjectives = new();

        // HashSet<string> _allObjectives = new();

        HashSet<string> _completedObjectives = new();
        bool _dirty;
        HashSet<string> _inactiveObjectives = new();

        Dictionary<string, ObjectiveProgress> _objectiveProgress = new();


        string _savePath;
        public static ObjectivesManager Instance { get; private set; }

        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            if (Objectives.Count == 0)
                Debug.LogWarning(
                    "[ObjectivesManager] No objective catalogs assigned. Please assign at least one catalog.");
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("[ObjectivesManager] No save file found, forcing initial save...");
                ResetObjectives(); // Ensure default values are set

                return;
            }

            // _allObjectives = new HashSet<string>(GetAllObjectiveIdsInOrder()); // keep if you want O(1) membership


            LoadObjectives();


            StartCoroutine(InitializeAfterFrame());
        }


        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            if (objectivesFolder == null) return;

            var folderPath = AssetDatabase.GetAssetPath(objectivesFolder);
            if (string.IsNullOrEmpty(folderPath) || !AssetDatabase.IsValidFolder(folderPath))
                return;

            // Clear and repopulate
            Objectives.Clear();

            // 1. Collect all loose ObjectiveObjects in folder
            var objectiveGuids = AssetDatabase.FindAssets(
                "t:Objectives.ScriptableObjects.ObjectiveObject", new[] { folderPath });

            foreach (var guid in objectiveGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var obj = AssetDatabase.LoadAssetAtPath<ObjectiveObject>(path);
                if (obj != null) Objectives.Add(obj);
            }

            // 2. Collect all catalogs in folder and flatten their objectives
            var catalogGuids = AssetDatabase.FindAssets(
                "t:Objectives.ScriptableObjects.ObjectivesCatalog", new[] { folderPath });

            foreach (var guid in catalogGuids)
            {
                var path = AssetDatabase.GUIDToAssetPath(guid);
                var catalog = AssetDatabase.LoadAssetAtPath<ObjectivesCatalog>(path);
                if (catalog != null && catalog.objectives != null)
                    foreach (var obj in catalog.objectives)
                        if (obj != null && !Objectives.Contains(obj))
                            Objectives.Add(obj);
            }

            EditorUtility.SetDirty(this);
        }
#endif

        public void Save()
        {
            SaveAllObjectives();
        }

        public void Load()
        {
            LoadObjectives();
        }

        public void Reset()
        {
            ResetObjectives();

            MarkDirty();
            ConditionalSave();
        }

        string ICoreGameService.GetSaveFilePath()
        {
            return GetSaveFilePath();
        }

// Call this from your SaveManager at checkpoints:
        public void CommitCheckpointSave()
        {
            if (_dirty)
            {
                SaveAllObjectives();
                _dirty = false;
            }
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public void ConditionalSave()
        {
            if (autoSave && _dirty)
            {
                SaveAllObjectives();
                _dirty = false;
            }
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath);
        }

        public void OnMMEvent(ObjectiveEvent eventType)
        {
            if (_processingEvents.Contains($"{eventType.objectiveId}_{eventType.type}"))
            {
                Debug.LogWarning(
                    $"Preventing recursive event processing for {eventType.objectiveId} - {eventType.type}");

                return;
            }

            _processingEvents.Add($"{eventType.objectiveId}_{eventType.type}");

            try
            {
                if (eventType.type == ObjectiveEventType.ObjectiveActivated)
                    SetObjectiveActive(eventType.objectiveId, false);

                if (eventType.type == ObjectiveEventType.IncrementObjectiveProgress)
                    AdvanceObjectiveProgress(
                        eventType.objectiveId, eventType.progressMadeByN,
                        eventType.progressMadeByF, eventType.miscProgressInfo);

                if (eventType.type == ObjectiveEventType.ObjectiveDeactivated)
                    SetObjectiveInactive(eventType.objectiveId);

                if (eventType.type == ObjectiveEventType.ObjectiveAdded) AddObjective(eventType.objectiveId, false);

                if (eventType.type == ObjectiveEventType.ObjectiveDeactivated)
                    SetObjectiveInactive(eventType.objectiveId);

                if (eventType.type == ObjectiveEventType.ObjectiveCompleted)
                    // UnityEngine.Debug.Log($"Objective {eventType.objectiveId} has been completed.");
                    CompleteObjective(eventType.objectiveId, eventType.notifyType);

                if (eventType.type == ObjectiveEventType.CompleteAllActiveObjectives)
                    // UnityEngine.Debug.Log($"Completing all active objectives.");
                    CompleteAllActiveObjectives();

                if (eventType.type == ObjectiveEventType.CompleteAllObjectivesPreviousTo)
                    CompleteAllObjectivesPreviousTo(eventType.objectiveId);
            }
            finally
            {
                _processingEvents.Remove($"{eventType.objectiveId}_{eventType.type}");
            }
        }
        void AdvanceObjectiveProgress(string eventTypeObjectiveId, int eventTypeProgressMadeByN,
            float eventTypeProgressMadeByF, string eventTypeMiscProgressInfo)
        {
            var objective = GetObjectiveById(eventTypeObjectiveId);
            if (objective == null)
            {
                Debug.LogError($"Objective with ID {eventTypeObjectiveId} not found.");
                return;
            }

            if (!_objectiveProgress.ContainsKey(eventTypeObjectiveId))
                _objectiveProgress[eventTypeObjectiveId] = new ObjectiveProgress
                {
                    currentProgress = 0

                    // Add any other default values neededj
                };


            switch (objective.objectiveProgressType)
            {
                case ObjectiveProgressType.None:
                    // No in-between progress, just complete it
                    CompleteObjective(eventTypeObjectiveId);
                    return;
                case ObjectiveProgressType.DoThingNTimes:
                    var currentProgress = _objectiveProgress[eventTypeObjectiveId].currentProgress;
                    currentProgress += eventTypeProgressMadeByN;
                    _objectiveProgress[eventTypeObjectiveId].currentProgress = currentProgress;
                    if (currentProgress >= objective.targetProgress)
                        CompleteObjective(eventTypeObjectiveId);
                    else
                        Refresh(eventTypeObjectiveId);


                    break;
                default:
                    Debug.LogWarning(
                        $"Unknown ObjectiveProgressType {objective.objectiveProgressType} for objective {eventTypeObjectiveId}");

                    break;
            }

            MarkDirty();
            ConditionalSave();
        }

        public HashSet<string> GetCompletedObjectives()
        {
            return _completedObjectives;
        }

        public HashSet<string> GetInactiveObjectives()
        {
            return _inactiveObjectives;
        }

        public HashSet<string> GetAllObjectivesSet()
        {
            var all = new HashSet<string>(_inactiveObjectives);
            all.UnionWith(_activeObjectives);
            all.UnionWith(_completedObjectives);
            return all;
        }

        public HashSet<string> GetActiveObjectives()
        {
            return _activeObjectives;
        }

        // Flattens the two dimensional list of objectives into a single enumerable collection
        IEnumerable<ObjectiveObject> GetAllObjectives()
        {
            foreach (var obj in Objectives)
                if (obj != null)
                    yield return obj;
        }

        IEnumerator InitializeAfterFrame()
        {
            yield return null;

            // Re-emit Activated for UI/listeners
            foreach (var activeId in _activeObjectives)
                ObjectiveEvent.Trigger(activeId, ObjectiveEventType.ObjectiveActivated);


            // Override completions if needed (for testing/debug)
            if (overrideObjectiveProgression)
            {
                if (objectivesToAddOnStart != null && objectivesToAddOnStart.Count > 0)
                    foreach (var objective in objectivesToAddOnStart)
                        if (objective != null)
                            AddObjective(objective.objectiveId, true);

                if (objectivesToActivateOnStart != null && objectivesToActivateOnStart.Count > 0)
                    foreach (var objective in objectivesToActivateOnStart)
                        if (objective != null)
                            SetObjectiveActive(objective.objectiveId, true);

                if (objectivesToMarkCompleteOnStart != null && objectivesToMarkCompleteOnStart.Count > 0)
                    foreach (var objective in objectivesToMarkCompleteOnStart)
                        if (objective != null)
                            CompleteObjective(objective.objectiveId, NotifyType.Silent);
            }


            MarkDirty();
            ConditionalSave();
        }

        void SetObjectiveInactive(string objectiveId)
        {
            if (_completedObjectives.Contains(objectiveId))
                return; // don't touch completed

            _activeObjectives.Remove(objectiveId);
            _inactiveObjectives.Add(objectiveId);

            MarkDirty();
            ConditionalSave();
        }

        public bool IsObjectiveActive(string objectiveId)
        {
            var objectiveActive = _activeObjectives.Contains(objectiveId);
            return objectiveActive;
        }

        public bool IsObjectiveAdded(string objectiveId)
        {
            return _inactiveObjectives.Contains(objectiveId) || _activeObjectives.Contains(objectiveId) ||
                   _completedObjectives.Contains(objectiveId);
        }

        public bool IsObjectiveCompleted(string objectiveId)
        {
            return _completedObjectives.Contains(objectiveId);
        }

        public void LoadObjectives()
        {
            if (_savePath == null) _savePath = GetSaveFilePath();

            if (ES3.KeyExists("CompletedObjectives", _savePath))
            {
                var completedObjectives = ES3.Load<HashSet<string>>("CompletedObjectives", _savePath);
                _completedObjectives.Clear();

                foreach (var objective in completedObjectives) _completedObjectives.Add(objective);
            }

            if (ES3.KeyExists("ActiveObjectives", _savePath))
            {
                var activeObjectives = ES3.Load<HashSet<string>>("ActiveObjectives", _savePath);
                _activeObjectives.Clear();

                foreach (var objectiveId in activeObjectives) _activeObjectives.Add(objectiveId);
            }

            if (ES3.KeyExists("InactiveObjectives", _savePath))
            {
                var inactiveObjectives = ES3.Load<HashSet<string>>("InactiveObjectives", _savePath);
                _inactiveObjectives.Clear();

                foreach (var objectiveId in inactiveObjectives) _inactiveObjectives.Add(objectiveId);
            }

            if (ES3.KeyExists("ObjectivesProgress", _savePath))
            {
                var loadedProgress = ES3.Load<Dictionary<string, ObjectiveProgress>>("ObjectivesProgress", _savePath);
                _objectiveProgress = loadedProgress ?? new Dictionary<string, ObjectiveProgress>();
            }
            else
            {
                _objectiveProgress = new Dictionary<string, ObjectiveProgress>();
            }
        }

        public void SetObjectiveActive(string objectiveId, bool force)
        {
            if (_completedObjectives.Contains(objectiveId))
                return; // never reactivate completed

            // check if prerequisites are met
            if (!WerePrerequisitesMet(GetObjectiveById(objectiveId)) && !force) return;


            // Ensure it’s not duplicated
            _inactiveObjectives.Remove(objectiveId);
            if (!_activeObjectives.Contains(objectiveId))
            {
                Debug.Log($"[Objectives] SetObjectiveActive({objectiveId})\n{new StackTrace(1, true)}");
                _activeObjectives.Add(objectiveId);
            }

            var obj = GetObjectiveById(objectiveId);
            if (obj == null)
            {
                Debug.LogError("Objective with ID " + objectiveId + " not found.");
                return;
            }

            if (obj.punctuatedAddAndActivate) addedAndActivatedPunctuatedObjective?.PlayFeedbacks();

            if (!_objectiveProgress.ContainsKey(objectiveId))
                _objectiveProgress[objectiveId] = new ObjectiveProgress
                {
                    currentProgress = 0
                    // Add any other default values needed
                };


            var poiId = obj.GetPOIUniqueId();
            // if (poiId != null) GamePOIEvent.Trigger(poiId, GamePOIEventType.MarkPOIAsTrackedByObjective, objectiveId);
            if (poiId != null)
            {
                var wrapper = CoreGamePOIManager.Instance?.GetPOIWrapper(poiId);
                var sceneName = wrapper != null ? wrapper.gameObject.scene.name : SceneManager.GetActiveScene().name;

                GamePOIEvent.Trigger(poiId, GamePOIEventType.MarkPOIAsTrackedByObjective, sceneName, obj.objectiveId);
            }

            if (obj.triggersSpontaneousEvent &&
                obj.triggersOnEvent == ObjectiveObject.TriggersOnObjectiveLifecycleEvent.OnActivate)
                SpontaneousTriggerEvent.Trigger(
                    obj.spontaneousEventUniqueId,
                    obj.spontaneousEventType, obj.spontaneousEventIntParameter, obj.spontaneousEventStringParameter, obj.secondarySpontaneousStringParameter);

            if (obj.objectiveProgressType == ObjectiveProgressType.DoThingNTimes)
            {
                var progress = _objectiveProgress[objectiveId];

                if (progress.currentProgress >= obj.targetProgress) CompleteObjective(objectiveId);
            }


            MarkDirty();
            ConditionalSave();
            Refresh(objectiveId);
        }

        void Refresh(string id)
        {
            ObjectiveEvent.Trigger(id, ObjectiveEventType.Refresh);
        }

        List<string> GetAllObjectiveIdsInOrder()
        {
            var list = new List<string>();

            foreach (var obj in Objectives)
                if (obj != null)
                    list.Add(obj.objectiveId);

            return list;
        }

        public void AddObjective(string objectiveId, bool force)
        {
            var objective = GetObjectiveById(objectiveId);
            if (objective == null)
            {
                Debug.LogWarning("Objective with ID " + objectiveId + " not found.");
                return;
            }

            // check if prerequisites are met
            if (!WerePrerequisitesMet(objective) && !force) return;

            var shouldBeActive = objective.shouldBeMadeActiveOnAdd;
            if (_activeObjectives.Contains(objectiveId) || _completedObjectives.Contains(objectiveId))
                // Debug.LogWarning($"Objective {objectiveId} is already active or completed.");
                return;

            // ADDED: Initialize progress entry if it doesn't exist
            if (!_objectiveProgress.ContainsKey(objectiveId))
                _objectiveProgress[objectiveId] = new ObjectiveProgress
                {
                    currentProgress = 0
                    // Add any other default values needed
                };

            if (shouldBeActive && !_inactiveObjectives.Contains(objectiveId))
                _activeObjectives.Add(objectiveId);
            else
                _inactiveObjectives.Add(objectiveId);

            MarkDirty();
            ConditionalSave();
        }
        bool WerePrerequisitesMet(ObjectiveObject objective)
        {
            var preRequisites = objective.prerequisiteObjectives;
            if (preRequisites != null)
            {
                foreach (var preRequisite in preRequisites)
                    if (!_completedObjectives.Contains(preRequisite.objectiveId))
                        return false;
            }
            else
            {
                return false;
            }


            return true;
        }

        public void CompleteObjective(string objectiveId, NotifyType notifyType = NotifyType.Regular)
        {
            if (_completedObjectives.Contains(objectiveId))
            {
                Debug.LogWarning($"Objective {objectiveId} has already been completed.");
                return;
            }

            if (_activeObjectives.Contains(objectiveId))
                _activeObjectives.Remove(objectiveId);
            else
                return;

            if (_inactiveObjectives.Contains(objectiveId)) _inactiveObjectives.Remove(objectiveId);

            _completedObjectives.Add(objectiveId);
            ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.Refresh);
            var obj = GetObjectiveById(objectiveId);
            if (obj == null)
            {
                Debug.LogError("Objective with ID " + objectiveId + " not found.");
                return;
            }

            if (obj.punctuatedCompletion) completedPunctuatedObjectiveFeedback?.PlayFeedbacks();

            var poiId = obj.GetPOIUniqueId();
            // get Active Scene
            var sceneName = SceneManager.GetActiveScene().name;
            if (poiId != null) GamePOIEvent.Trigger(poiId, GamePOIEventType.UnmarkPOIAsTrackedByObjective, sceneName);


            if (notifyType == NotifyType.Silent)
                return;

            objectiveCompletedFeedback?.PlayFeedbacks();

            if (obj.activateUponCompletion != null)
            {
                ObjectiveEvent.Trigger(obj.activateUponCompletion.objectiveId, ObjectiveEventType.ObjectiveAdded);
                ObjectiveEvent.Trigger(obj.activateUponCompletion.objectiveId, ObjectiveEventType.ObjectiveActivated);
            }

            if (obj.triggersSpontaneousEvent &&
                obj.triggersOnEvent == ObjectiveObject.TriggersOnObjectiveLifecycleEvent.OnComplete)
                SpontaneousTriggerEvent.Trigger(
                    obj.spontaneousEventUniqueId,
                    obj.spontaneousEventType, obj.spontaneousEventIntParameter, obj.spontaneousEventStringParameter, obj.secondarySpontaneousStringParameter);

            MarkDirty();
            ConditionalSave();
        }

        public void SaveAllObjectives()
        {
            var saveFilePath = GetSaveFilePath();

            ES3.Save("CompletedObjectives", _completedObjectives, saveFilePath);
            ES3.Save("ActiveObjectives", _activeObjectives, saveFilePath);
            ES3.Save("InactiveObjectives", _inactiveObjectives, saveFilePath);
            ES3.Save("ObjectivesProgress", _objectiveProgress, saveFilePath);
        }

        public void ResetObjectives()
        {
            _completedObjectives = new HashSet<string>();
            _activeObjectives = new HashSet<string>();
            _inactiveObjectives = new HashSet<string>();
            _objectiveProgress = new Dictionary<string, ObjectiveProgress>();

            foreach (var objective in objectivesToAddOnStart)
                if (objective != null)
                    AddObjective(objective.objectiveId, true);

            foreach (var objective in objectivesToActivateOnStart)
                if (objective != null && IsObjectiveAdded(objective.objectiveId))
                    SetObjectiveActive(objective.objectiveId, true);

            foreach (var objective in objectivesToMarkCompleteOnStart)
                if ((objective != null && IsObjectiveAdded(objective.objectiveId)) ||
                    IsObjectiveActive(objective.objectiveId))
                    CompleteObjective(objective.objectiveId, NotifyType.Silent);
        }

        static string GetSaveFilePath()
        {
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.ObjectivesSave);
        }

        public ObjectiveObject GetObjectiveById(string objectiveId)
        {
            foreach (var obj in GetAllObjectives())
                if (obj.objectiveId == objectiveId)
                    return obj;


            return null;
        }

        public void CompleteAllActiveObjectives()
        {
            var snapshot = new List<string>(_activeObjectives);
            foreach (var id in snapshot)
                if (!_completedObjectives.Contains(id))
                    ObjectiveEvent.Trigger(id, ObjectiveEventType.ObjectiveCompleted);

            SaveAllObjectives();
        }

        void CompleteAllObjectivesPreviousTo(string objectiveId)
        {
            var ordered = GetAllObjectiveIdsInOrder();
            foreach (var id in ordered)
            {
                if (id == objectiveId) break;
                if (!_completedObjectives.Contains(id))
                    ObjectiveEvent.Trigger(id, ObjectiveEventType.ObjectiveCompleted, NotifyType.Silent);
            }

            SaveAllObjectives();
        }
        public int GetIntProgressForObjective(string eObjectiveId)
        {
            if (_objectiveProgress.TryGetValue(eObjectiveId, out var value))
                return value.currentProgress;

            return 0;
        }
    }
}
