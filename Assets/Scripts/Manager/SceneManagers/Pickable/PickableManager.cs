using System;
using System.Collections;
using System.Collections.Generic;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Gameplay.Events;
using Helpers.Events;
using Helpers.Events.Inventory;
using Helpers.Interfaces;
using LevelConstruct.Interactable.ItemInteractables.ItemPicker;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using Utilities.Inputs;

namespace Manager.SceneManagers.Pickable
{
    public class PickableManager : MonoBehaviour, MMEventListener<PickableEvent>, ICoreGameService,
        MMEventListener<LoadedManagerEvent>

    {
        [Tooltip(
            "If true, the manager will save automatically when it becomes dirty. Leave false for explicit, checkpoint-only saving.")]
        [SerializeField]
        bool autoSave;
        [FormerlySerializedAs("WeightAblePerStrength")]
        public float weightAblePerStrength = 5.0f;

        public bool allowPickingUpPhysicalItems;

        readonly Dictionary<string, MovedItemData> _movedPickables = new(StringComparer.Ordinal);

        // Modular index: which items were picked in which scene (no foreach over all)
        readonly Dictionary<string, HashSet<string>> _pickedByScene = new(StringComparer.Ordinal);

        // Global membership check: O(1) "was this ID ever picked?"
        readonly HashSet<string> _pickedItems = new();

        readonly HashSet<string> _pickedItemTypes = new();

        readonly Dictionary<string, PlacedItemData> _placedPickables = new(StringComparer.Ordinal);


        bool _dirty;

        // Input passthrough to preserve your current ItemPicker flow
        RewiredFirstPersonInputs _rewiredFirstPersonInputs;
        string _savePath;
        public static PickableManager Instance { get; private set; }

        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All) OnSceneLoaded();
        }

        public bool IsItemTypePicked(string itemTypeId)
        {
            return _pickedItemTypes.Contains(itemTypeId);
        }


        // Back-compat for existing callers (e.g., ItemPicker)
        public void AddPickedItem(string uniqueId, bool picked, string sceneName = null)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;

            if (!picked)
            {
                // Optional: support "unpick" if you ever need it
                if (_pickedItems.Remove(uniqueId) && !string.IsNullOrEmpty(sceneName)
                                                  && _pickedByScene.TryGetValue(sceneName, out var set))
                    set.Remove(uniqueId);

                MarkDirty();
                ConditionalSave();
                return;
            }

            // Add globally
            if (_pickedItems.Add(uniqueId))
            {
                MarkDirty();
                ConditionalSave();
            }

            // Record scene index if provided
            if (!string.IsNullOrEmpty(sceneName))
            {
                if (!_pickedByScene.TryGetValue(sceneName, out var set))
                {
                    set = new HashSet<string>();
                    _pickedByScene[sceneName] = set;
                }

                set.Add(uniqueId);
            }
        }

        public void AddPickedItemTypeIf(string inventoryItemItemID)
        {
            _pickedItemTypes.Add(inventoryItemItemID);
        }

        // Store transform data instead of Transform references (which don't serialize well)
        [Serializable]
        public class MovedItemData
        {
            public Vector3 position;
            public Quaternion rotation;
            public string sceneName;

            public MovedItemData(Vector3 pos, Quaternion rot, string scene)
            {
                position = pos;
                rotation = rot;
                sceneName = scene;
            }
        }

        [Serializable]
        public class PlacedItemData
        {
            public string itemId;
            public Vector3 position;
            public Quaternion rotation;
            public string sceneName;

            public PlacedItemData(Vector3 pos, Quaternion rot, string scene, string itemId)
            {
                position = pos;
                rotation = rot;
                sceneName = scene;
                this.itemId = itemId;
            }
        }

        #region Events

        public void OnMMEvent(PickableEvent e)
        {
            if (e.EventType == PickableEventType.Picked)
            {
                var id = e.UniqueId;
                if (string.IsNullOrEmpty(id)) return;

                var sceneName = e.ItemTransform ? e.ItemTransform.gameObject.scene.name : string.Empty;

                var newlyAdded = _pickedItems.Add(id);
                if (!_pickedItemTypes.Contains(e.SOItemID))
                {
                    MyUIEvent.Trigger(UIType.ItemInfoPopup, UIActionType.Open);
                    ItemInfoUIEvent.Trigger(ItemInfoUIEventType.ShowNewItemType, e.SOItemID);
                }

                StartCoroutine(WaitThenAddSOItemID(e.SOItemID));

                // Always ensure scene index is populated
                if (!string.IsNullOrEmpty(sceneName))
                {
                    if (!_pickedByScene.TryGetValue(sceneName, out var set))
                    {
                        set = new HashSet<string>();
                        _pickedByScene[sceneName] = set;
                    }

                    set.Add(id);
                }

                // Remove from moved items if it was there (picked up from world)
                if (_movedPickables.ContainsKey(id))
                {
                    _movedPickables.Remove(id);
                    Debug.Log($"Removed {id} from moved items (picked up)");
                }

                if (_placedPickables.ContainsKey(id))
                {
                    _placedPickables.Remove(id);
                    Debug.Log($"Removed {id} from placed items (picked up)");
                }

                if (newlyAdded)
                {
                    MarkDirty();
                    ConditionalSave();
                }
            }
            else if (e.EventType == PickableEventType.MovedItemCameToRest)
            {
                var id = e.UniqueId;
                if (string.IsNullOrEmpty(id) || e.ItemTransform == null) return;

                var sceneName = e.ItemTransform.gameObject.scene.name;
                var data = new MovedItemData(
                    e.ItemTransform.position,
                    e.ItemTransform.rotation,
                    sceneName
                );

                _movedPickables[id] = data;
                if (_placedPickables.ContainsKey(id))
                {
                    _placedPickables.Remove(id);
                    Debug.Log($"Removed {id} from placed items (now moved)");
                }

                Debug.Log($"Item moved in world: {id} at {data.position} in scene {sceneName}");

                MarkDirty();
                ConditionalSave();
            }
            else if (e.EventType == PickableEventType.PlacedItemCameToRest)
            {
                var id = e.UniqueId;
                if (string.IsNullOrEmpty(id) || e.ItemTransform == null) return;

                var invID = e.SOItemID;
                if (string.IsNullOrEmpty(invID)) return;

                var sceneName = e.ItemTransform.gameObject.scene.name;
                var data = new PlacedItemData(
                    e.ItemTransform.position,
                    e.ItemTransform.rotation,
                    sceneName,
                    invID
                );

                _placedPickables[id] = data;
                Debug.Log($"Item placed in world: {id} at {data.position} in scene {sceneName}");

                MarkDirty();
                ConditionalSave();
            }
        }

        IEnumerator WaitThenAddSOItemID(string itemID)
        {
            yield return null; // wait a frame to ensure any related events (like showing the popup) have processed

            _pickedItemTypes.Add(itemID);
        }

        #endregion

        #region Scene Restoration

        void RestorePlacedItemsInScene(string currentScene)
        {
            if (string.IsNullOrEmpty(currentScene)) return;

            // Get all existing ItemPickers in the scene FIRST
            var existingPickers = new HashSet<string>();
            var allPickers = FindObjectsByType<ItemPicker>(FindObjectsSortMode.None);
            foreach (var picker in allPickers)
                if (!string.IsNullOrEmpty(picker.uniqueID))
                    existingPickers.Add(picker.uniqueID);

            var restoredCount = 0;
            var skippedCount = 0;

            foreach (var kvp in _placedPickables)
            {
                var uniqueId = kvp.Key;
                var itemData = kvp.Value;

                // Only restore items in this scene
                if (itemData.sceneName != currentScene) continue;

                // CRITICAL: Skip if item already exists in scene (prevents duplication)
                if (existingPickers.Contains(uniqueId))
                {
                    skippedCount++;
                    continue;
                }

                var itemObject = Resources.Load<MyBaseItem>("Items/" + itemData.itemId);
                var itemPrefab = itemObject != null ? itemObject.Prefab : null;
                if (itemPrefab == null)
                {
                    Debug.LogWarning($"Could not load prefab for placed item ID {itemData.itemId}");
                    continue;
                }

                var pickerGameObject = Instantiate(itemPrefab, itemData.position, itemData.rotation);
                var pickerRB = pickerGameObject.GetComponent<Rigidbody>();
                if (pickerRB != null)
                {
                    pickerRB.isKinematic = true;
                    pickerRB.useGravity = false;
                }

                pickerGameObject.SetActive(true);

                var picker = pickerGameObject.GetComponent<ItemPicker>();
                if (picker != null)
                {
                    picker.uniqueID = uniqueId;
                    restoredCount++;
                }
            }

            if (restoredCount > 0)
                Debug.Log($"Restored {restoredCount} placed items in scene {currentScene}");

            if (skippedCount > 0)
                Debug.Log($"Skipped {skippedCount} placed items (already in scene {currentScene})");
        }


        /// <summary>
        ///     Call this after a scene loads to restore moved items to their saved positions
        /// </summary>
        public void RestoreMovedItemsInScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;

            var restoredCount = 0;
            var notFoundCount = 0;

            foreach (var kvp in _movedPickables)
            {
                var itemId = kvp.Key;
                var itemData = kvp.Value;

                // Only restore items in this scene
                if (itemData.sceneName != sceneName) continue;

                // Find the ItemPicker in the scene
                var allPickers = FindObjectsByType<ItemPicker>(FindObjectsSortMode.None);
                ItemPicker foundPicker = null;

                foreach (var picker in allPickers)
                    if (picker.uniqueID == itemId)
                    {
                        foundPicker = picker;
                        break;
                    }

                if (foundPicker != null)
                {
                    foundPicker.RestoreMovedPosition(itemData.position, itemData.rotation);
                    restoredCount++;
                }
                else
                {
                    notFoundCount++;
                    Debug.LogWarning(
                        $"Could not find ItemPicker with ID {itemId} in scene {sceneName} to restore position");
                }
            }

            if (restoredCount > 0) Debug.Log($"Restored {restoredCount} moved items in scene {sceneName}");

            if (notFoundCount > 0) Debug.LogWarning($"{notFoundCount} moved items not found in scene {sceneName}");
        }

        /// <summary>
        ///     Check if an item has been moved from its original position
        /// </summary>
        public bool IsItemMoved(string uniqueId)
        {
            return _movedPickables.ContainsKey(uniqueId);
        }

        /// <summary>
        ///     Get the saved position of a moved item
        /// </summary>
        public bool TryGetMovedItemData(string uniqueId, out MovedItemData data)
        {
            return _movedPickables.TryGetValue(uniqueId, out data);
        }

        public void RemoveMovedPickable(string uniqueId)
        {
            if (string.IsNullOrEmpty(uniqueId)) return;

            if (_movedPickables.Remove(uniqueId))
            {
                Debug.Log("Removed placed pickable: " + uniqueId);
                MarkDirty();
                ConditionalSave();
            }
        }

        #endregion


        #region Lifecycle

        void Awake()
        {
            if (Instance == null)
                Instance = this;

            else
                Destroy(gameObject);

            // Subscribe to scene loaded events
            // SceneManager.sceneLoaded += OnSceneLoaded;
        }

        void OnDestroy()
        {
            // SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        IEnumerator RestoreAfterSceneSetup()
        {
            // Wait a frame for all ItemPickers to initialize
            yield return null;

            var currentScene = SceneManager.GetActiveScene().name;

            RestoreMovedItemsInScene(currentScene);
            RestorePlacedItemsInScene(currentScene);
        }
        void OnSceneLoaded()
        {
            // if (SpawnSystem.PersistentScenes.Contains(scene.name)) return;
            // var currentScene = SceneManager.GetActiveScene().name;
            // Restore moved items when a new scene loads
            StartCoroutine(RestoreAfterSceneSetup());
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData())
            {
                Debug.Log("[PickableManager] No save file found, forcing initial reset...");
                Reset(); // sets defaults; does NOT save unless you call Save()
            }

            Load();
        }

        void OnEnable()
        {
            this.MMEventStartListening<LoadedManagerEvent>();
            this.MMEventStartListening<PickableEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<LoadedManagerEvent>();
            this.MMEventStopListening<PickableEvent>();
        }

        #endregion

        #region IGameService (parity with ObjectivesManager)

        public void Save()
        {
            var path = GetSaveFilePath();

            // Save global set and per-scene index
            ES3.Save("PickedItems", _pickedItems, path);
            ES3.Save("PickedByScene", _pickedByScene, path);
            ES3.Save("MovedPickables", _movedPickables, path);
            ES3.Save("PlacedPickables", _placedPickables, path);


            // Optional backward-compat: also write individual keys once
            foreach (var id in _pickedItems)
                ES3.Save(id, true, path);


            _dirty = false;
            // Debug.Log(
            // $"[PickableManager] Saved {_pickedItems.Count} picked items and {_movedPickables.Count} moved items");
        }

        public void Load()
        {
            var path = GetSaveFilePath();

            _pickedItems.Clear();
            _pickedByScene.Clear();
            _movedPickables.Clear();
            _placedPickables.Clear();

            if (ES3.KeyExists("PickedItems", path))
            {
                var set = ES3.Load<HashSet<string>>("PickedItems", path);
                foreach (var id in set) _pickedItems.Add(id);
            }
            else if (ES3.FileExists(path))
            {
                // Back-compat: old per-key format
                foreach (var key in ES3.GetKeys(path))
                    if (ES3.KeyExists(key, path) && ES3.Load<bool>(key, path))
                        _pickedItems.Add(key);
            }

            if (ES3.KeyExists("PickedByScene", path))
            {
                var dict = ES3.Load<Dictionary<string, HashSet<string>>>("PickedByScene", path);
                foreach (var kv in dict)
                    _pickedByScene[kv.Key] = new HashSet<string>(kv.Value);
            }

            if (ES3.KeyExists("MovedPickables", path))
            {
                var dict = ES3.Load<Dictionary<string, MovedItemData>>("MovedPickables", path);
                foreach (var kv in dict)
                    _movedPickables[kv.Key] = kv.Value;
            }

            if (ES3.KeyExists("PlacedPickables", path))
            {
                var dict = ES3.Load<Dictionary<string, PlacedItemData>>("PlacedPickables", path);
                foreach (var kv in dict)
                    _placedPickables[kv.Key] = kv.Value;
            }


            _dirty = false;
        }

        public void Reset()
        {
            _pickedItems.Clear();
            _pickedByScene.Clear();
            _movedPickables.Clear();
            _placedPickables.Clear();

            _dirty = true;
            ConditionalSave();
        }

        public string GetSaveFilePath()
        {
            // Follow the global save path pattern used by ObjectivesManager
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.PickablesSave);
        }

        #endregion

        #region Public API (keeps current ItemPicker flow unchanged)

        public bool IsItemPicked(string uniqueId)
        {
            return _pickedItems.Contains(uniqueId);
        }

        public bool IsItemPickedInScene(string uniqueId, string sceneName)
        {
            return _pickedByScene.TryGetValue(sceneName, out var set) && set.Contains(uniqueId);
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath ?? GetSaveFilePath());
        }

        // Keeps ItemPicker’s existing input check intact
        public bool IsInteractPressed()
        {
            if (_rewiredFirstPersonInputs != null) return _rewiredFirstPersonInputs.interact;

            _rewiredFirstPersonInputs = FindFirstObjectByType<RewiredFirstPersonInputs>();
            if (_rewiredFirstPersonInputs == null)
                throw new Exception("RewiredFirstPersonInputs not found! Please ensure it is set up correctly.");

            return _rewiredFirstPersonInputs.interact;
        }

        // Optional: call this at checkpoints if you prefer the same wording you used for Objectives
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
