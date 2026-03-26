using System.Collections.Generic;
using System.Linq;
using LevelConstruct.Spawn;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utilities.Static
{
    public static class SpawnRegistry
    {
        static readonly Dictionary<string, SpawnPoint >Dict = new();

        public static void Init()
        {
            // Make idempotent
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;

            Dict.Clear();

            // Register points in scenes already loaded
            for (var i = 0; i < SceneManager.sceneCount; i++)
                OnSceneLoaded(SceneManager.GetSceneAt(i), LoadSceneMode.Additive);

            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
        }

        public static bool TryGet(string id, out SpawnPoint p)
        {
            return Dict.TryGetValue(id, out p);
        }

        public static SpawnPoint Get(string id)
        {
            return Dict[id];
        }

        static void OnSceneLoaded(Scene scene, LoadSceneMode _)
        {
            foreach (var root in scene.GetRootGameObjects())
            foreach (var spawnPoint in root.GetComponentsInChildren<SpawnPoint>(true))
                Register(spawnPoint);
        }

        static void OnSceneUnloaded(Scene scene)
        {
            var keysToRemove = new List<string>();

            foreach (var kvp in Dict)
            {
                // 1. skip already-destroyed entries
                if (kvp.Value == null)
                {
                    keysToRemove.Add(kvp.Key);
                    continue;
                }

                // 2. same-scene check as before
                if (kvp.Value.gameObject.scene == scene)
                    keysToRemove.Add(kvp.Key);
            }

            foreach (var k in keysToRemove)
                Dict.Remove(k);
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics()
        {
            Dict.Clear();
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
        }

        static void Register(SpawnPoint sp)
        {
            // purge nulls
            foreach (var k in Dict.Where(kvp => kvp.Value == null).Select(kvp => kvp.Key).ToList())
                Dict.Remove(k);

            if (Dict.TryGetValue(sp.Id, out var existing))
            {
                if (existing == sp) return; // same component seen again
                Debug.LogWarning(
                    $"Duplicate spawn point ID '{sp.Id}'\n" +
                    $"Existing: {existing.transform.GetHierarchyPath()} (scene {existing.gameObject.scene.name})\n" +
                    $"New:      {sp.transform.GetHierarchyPath()} (scene {sp.gameObject.scene.name})");

                return;
            }

            Dict.Add(sp.Id, sp);
        }

// small helper (extension):
        static string GetHierarchyPath(this Transform t)
        {
            var path = t.name;
            while (t.parent)
            {
                t = t.parent;
                path = t.name + "/" + path;
            }

            return path;
        }
    }
}
