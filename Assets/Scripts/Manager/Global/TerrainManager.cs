// using Digger.Modules.Runtime.Sources;
using FirstPersonPlayer.Tools;
using SceneScripts.TerrainScripts;
using UnityEngine;

namespace Manager.Global
{
    public class TerrainManager : MonoBehaviour
    {
        public TerrainController currentTerrainController;
        // public DiggerMasterRuntime currentDiggerMasterRuntime;

        public static TerrainManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        public void Initialize()
        {
            var allTerrainControllers =
                FindObjectsByType<TerrainController>(FindObjectsInactive.Include, FindObjectsSortMode.None);

            if (allTerrainControllers.Length == 0)
            {
            }
            else if (allTerrainControllers.Length == 1)
            {
                currentTerrainController = allTerrainControllers[0];
            }
            else
            {
                Debug.LogWarning("Multiple TerrainControllers found. Using the first one.");
            }
        }

        public TerrainController GetCurrentTerrainController()
        {
            if (currentTerrainController != null) return currentTerrainController;

            Debug.LogError("Current Terrain Controller is not assigned.");
            return null;
        }

        public int[] GetAllowedTerrainTextureIndices(ToolType toolType)
        {
            return currentTerrainController.terrainBehavior
                .GetAllowedTerrainTextureIndices(toolType);
        }
    }
}