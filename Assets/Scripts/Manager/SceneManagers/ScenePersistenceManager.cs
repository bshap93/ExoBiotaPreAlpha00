using System.Collections.Generic;
using Helpers.Interfaces;
using Interfaces;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Manager.SceneManagers
{
    public class ScenePersistenceManager : MonoBehaviour, ICoreGameService
    {
        [ValueDropdown("GetScenNameOptions")] public List<string> KnownWorldScenes;

        [SerializeField] bool autosave;

        readonly Dictionary<string, string> _sceneSavePaths = new();

        bool _dirty;

        string _savePath;


        public string CurrentWorldSceneName { get; private set; }

        public static ScenePersistenceManager Instance { get; private set; }

// ScenePersistenceManager.cs
        void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
                DontDestroyOnLoad(gameObject);
        }

        void Start()
        {
            _savePath = GetSaveFilePath();

            if (!HasSavedData()) Reset(); // Ensure default values are set

            Load();
        }

        void OnEnable()
        {
            // defensive: remove before add
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }


        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }


        public void Save()
        {
        }

        public void Load()
        {
            // Prefer an already-loaded world scene (e.g., after your bootstrapper loaded additively)
            if (TryGetLoadedWorldScene(out var world))
            {
                var path = GetSavePath(world.name);
                foreach (var root in world.GetRootGameObjects())
                foreach (var savable in root.GetComponentsInChildren<ISceneSavable>(true))
                    savable.LoadSceneState(path);

                CurrentWorldSceneName = world.name;
            }

            // If no world scene is loaded yet, do nothing here; OnSceneLoaded will handle it.
        }

        public void Reset()
        {
            // Optionally wipe all scene files
            WipeAllSceneFiles();
            CurrentWorldSceneName = null;
        }

        public void ConditionalSave()
        {
            if (autosave && _dirty)
            {
                SaveCurrentScene();
                _dirty = false;
            }
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        public string GetSaveFilePath()
        {
            // Re-use your existing SaveSlotManager to stay consistent
            return SaveManager.Instance.GetGlobalSaveFilePath(GlobalManagerType.ScenePersistenceSave);
        }

        public void CommitCheckpointSave()
        {
            if (_dirty) Save();
        }

        public bool HasSavedData()
        {
            return ES3.FileExists(_savePath);
        }

        bool TryGetLoadedWorldScene(out Scene world)
        {
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var s = SceneManager.GetSceneAt(i);
                if (s.isLoaded && KnownWorldScenes.Contains(s.name))
                {
                    world = s;
                    return true;
                }
            }

            world = default;
            return false;
        }


        void WipeAllSceneFiles()
        {
            foreach (var sceneName in KnownWorldScenes) // already in this class
            foreach (var path in SaveManager.Instance.GetAllSceneSavePaths(sceneName))
                if (ES3.FileExists(path))
                {
                    Debug.Log($"[ScenePersistenceManager] Deleting {path}");
                    ES3.DeleteFile(path);
                }
        }

        // ---------- save ----------
        void OnSceneUnloaded(Scene scene)
        {
            SaveSceneManagers(scene);
        }

        public void SaveCurrentScene()
        {
            var scene = SceneManager.GetSceneByName(CurrentWorldSceneName);

            if (!scene.IsValid())
            {
                Debug.LogWarning(
                    $"[ScenePersistenceManager] Current scene '{CurrentWorldSceneName}' is invalid or not loaded.");

                return;
            }

            if (scene.isLoaded)
                SaveSceneManagers(scene);
            else
                Debug.LogWarning($"[ScenePersistenceManager] Attempted to save unloaded scene: {scene.name}");
        }

        public void SaveSceneManagers(Scene scene)
        {
            var path = GetSavePath(scene.name);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var saver in root.GetComponentsInChildren<ISceneSavable>())
                saver.SaveSceneState(path);
        }

        // ---------- load ----------
        void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            var path = GetSavePath(scene.name);
            foreach (var root in scene.GetRootGameObjects())
            foreach (var saver in root.GetComponentsInChildren<ISceneSavable>())
                saver.LoadSceneState(path);

            if (KnownWorldScenes.Contains(scene.name)) CurrentWorldSceneName = scene.name;
        }


        string GetSavePath(string sceneName)
        {
            if (_sceneSavePaths.TryGetValue(sceneName, out var cached)) return cached;
            // Re-use your existing SaveSlotManager to stay consistent
            var path = SaveManager.Instance.GetSceneFilePath(sceneName);
            _sceneSavePaths[sceneName] = path;
            return path;
        }

        string[] GetScenNameOptions()
        {
            // Return the names of all known world scenes
            return PlayerSpawnManager.GetSceneOptions();
        }
    }
}
