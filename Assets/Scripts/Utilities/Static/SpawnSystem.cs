using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Events;
using LevelConstruct.Spawn;
using Manager;
using Manager.Global;
using Structs;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Utilities.Static
{
    public struct SpawnRequest
    {
        public string Scene;
        public GameMode Mode;
        public string PointId;

        public SpawnRequest(string scene, GameMode mode, string pointId)
        {
            (Scene, Mode, PointId) = (scene, mode, pointId);
        }
    }


    public static class SpawnSystem
    {
        // SpawnSystem.cs  (top of the class)
        public static readonly HashSet<string> PersistentScenes = new()
        {
            "Boot", // stays loaded forever
            "Core",
            "Actors",
            "DialogueScene",
            "Tutorial"
        };


        public static SpawnInfo CurrentSpawn { get; set; }

        public static async Task LoadAndSpawnAsync(SpawnInfo info)
        {
            await LoadOverScene(info.OverSceneName);
            await LoadWorldScene(info.SceneName, info.OverSceneName);

            // Give the scene a moment to fully initialize
            await Task.Delay(100);

            // Move PlayerRoot BEFORE switching modes
            var root = GameObject.FindWithTag("PlayerRoot");
            var point = SpawnRegistry.Get(info.SpawnPointId);
            root.transform.SetPositionAndRotation(point.Xform.position, point.Xform.rotation);
            SpawnCheckpoint.NotifySpawned(); // <- add this

            if (point != null && info.Mode != point.Mode && info.Mode != GameMode.FreeLook)
                info.Mode = point.Mode; // Now switch to the mode (instantiate dirigible)

            ModeLoadEvent.Trigger(ModeLoadEventType.Enabled, info.Mode);


            CurrentSpawn = info;
        }

        public static async Task LoadAndSpawnAsync(string spawnPointId)
        {
            // 1. Ask the save-manager for that specific point
            var save = PlayerSpawnManager.Instance;
            var info = save.LoadSlot(spawnPointId);

            // 2. Fallback: if the ID wasn’t in the save, use the usual “last-spawn” record
            if (info == null)
                throw new Exception($"Spawn point ID '{spawnPointId}' not found in save data.");

            // 3. Re-use your existing loader
            await LoadAndSpawnAsync(info);
        }

        static async Task LoadWorldScene(string sceneName, string overSceneName = null)
        {
            // Collect scenes to unload FIRST, then unload after iteration
            var toUnload = new List<Scene>();
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scn = SceneManager.GetSceneAt(i);
                if (!scn.isLoaded) continue;

                var keep = PersistentScenes.Contains(scn.name)
                           || scn.name == sceneName
                           || scn.name == overSceneName; // <-- keep the over scene

                if (!keep) toUnload.Add(scn);
            }

            foreach (var scn in toUnload)
            {
                await SceneManager.UnloadSceneAsync(scn);
                await Task.Yield();
            }

            // Load if not already loaded
            if (!SceneManager.GetSceneByName(sceneName).isLoaded)
            {
                await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
                await Task.Yield();
            }

            // 👉 Make the world scene active
            var worldScene = SceneManager.GetSceneByName(sceneName);
            if (worldScene.IsValid() && worldScene.isLoaded) SceneManager.SetActiveScene(worldScene);
        }

        static async Task LoadOverScene(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return;
            if (SceneManager.GetSceneByName(sceneName).isLoaded) return;

            await SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
            await Task.Yield();
        }


        public static async Task LoadAndSpawnAsync(SpawnRequest req)
        {
            // ---------- unload current world & load target scene ----------
            await LoadWorldScene(req.Scene); // <— helper extracted from your existing code


            if (!SpawnRegistry.TryGet(req.PointId, out var point))
                throw new Exception($"Spawn point '{req.PointId}' not found in scene '{req.Scene}'");

            // build a SpawnInfo on the fly and delegate
            var info = new SpawnInfo
            {
                SceneName = req.Scene,
                Mode = req.Mode,
                SpawnPointId = req.PointId
            };

            await LoadAndSpawnAsync(info); // reuse the old method
        }

        // SpawnSystem.cs
        public static void TeleportAndSwitch(SpawnPoint point, GameMode mode)
        {
            var root = GameObject.FindWithTag("PlayerRoot");
            root.transform.SetPositionAndRotation(
                point.transform.position,
                point.transform.rotation);

            SpawnCheckpoint.NotifySpawned(); // <- add this

            GameStateManager.Instance.SwitchTo(mode);
        }
    }
}
