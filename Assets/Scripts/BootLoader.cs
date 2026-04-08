using System.Threading.Tasks;
using EditorScripts;
using Helpers.ScriptableObjects;
using LevelConstruct;
using Manager;
using OWPData.Structs;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;
using UnityEngine.SceneManagement;
using Utilities.Static;

[DefaultExecutionOrder(-1000)] // ensure this runs before any other scripts
public class BootLoader : MonoBehaviour
{
    public static bool ForceNewGame; // set before scene load

    [SerializeField] bool useOverrideSpawnInfo;
    [SerializeField] bool playWithTutorialOn;
    [SerializeField] bool playWithDevToolsOn;
    [SerializeField] bool restartFromBeginning;
    [SerializeField] bool isBridge;

    [SerializeField] [InlineProperty] [HideLabel]
    SpawnInfoEditor overrideSpawnInfo;

    async void Awake()
    {
        await ConductBootLoad();
    }

    public async Task ConductBootLoad()
    {
        SpawnRegistry.Init();

        DontDestroyOnLoad(gameObject); // survive until we unload Boot

        // Load persistent managers first so SaveManager/PlayerSpawnManager exist
        await SceneManager.LoadSceneAsync("Core", LoadSceneMode.Additive);
        await SceneManager.LoadSceneAsync("Actors", LoadSceneMode.Additive);
        await SceneManager.LoadSceneAsync("DialogueScene", LoadSceneMode.Additive);
        // await SceneManager.LoadSceneAsync("Overseer", LoadSceneMode.Additive);
        if (playWithTutorialOn) await SceneManager.LoadSceneAsync("Tutorial", LoadSceneMode.Additive);
        // if (playWithDevToolsOn) await SceneManager.LoadSceneAsync("DevTools", LoadSceneMode.Additive);

        SpawnRegistry.Init();


        var config = new SaveManagerConfig();
        if (!playWithTutorialOn)
            config.DisabledGlobalManagers.Add(GlobalManagerType.TutorialSave);

        // honor either the title screen PlayerPrefs flag or the editor toggle
        if (ForceNewGame || restartFromBeginning)
        {
            config.ForceReset = true;
            ForceNewGame = false; // reset static for next time
        }

        SaveManager.Instance.ApplyConfig(config);
        if (config.ForceReset)
            // actually perform the reset now so spawn selection sees a "fresh" state
        {
            SaveManager.Instance.ResetGameSave();
            SaveManager.Instance.SaveAll();
        }

        // --- now decide where to spawn ---

        SpawnInfo info;

        // If this is a bridge scene and we have pending bridge data, use that
        if (isBridge && BridgeData.HasPendingSpawn)
        {
            info = BridgeData.ConsumeTarget();
            Debug.Log($"[BootLoader] Bridge mode: Using BridgeData target {info.SceneName}");
        }
        else if (useOverrideSpawnInfo)
        {
            info = overrideSpawnInfo.ToSpawnInfo();
        }
        else
        {
            // After a reset, PlayerSpawnManager will have written a default spawn.
            // So HasSave / LoadSlot will return that default spawn (good).
            if (!config.ForceReset &&
                PlayerSpawnManager.Instance.HasSave())
                info = PlayerSpawnManager.Instance.LoadSlot();
            else
                info = new SpawnInfo
                {
                    SceneName = "AshpoolMine",
                    Mode = GameMode.FirstPerson,
                    SpawnPointId = "StartSpawn",
                    OverSceneName = "MineOverScene"
                };
        }

        // prefer async/await style to mix nicely with the rest of Awake()
        await SpawnSystem.LoadAndSpawnAsync(info);

        SaveManager.Instance.LoadAll();

        // 5) tidy up – Boot scene no longer needed
        if (!isBridge)
            await SceneManager.UnloadSceneAsync("Boot");
    }
}
