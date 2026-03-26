using JetBrains.Annotations;
using Manager.Global;
using TerrainScripts;
using UnityEngine;

namespace SceneScripts.TerrainScripts
{
    public class TerrainController : MonoBehaviour
    {
        [SerializeField] public TerrainBehavior terrainBehavior;
        private GameObject _currentDebrisPrefab;

        private GameObject _previousDebrisPrefab;

        public void OnEnable()
        {
            if (TerrainManager.Instance != null)
                TerrainManager.Instance.currentTerrainController = this;
        }


        public GameObject GetTerrainPrefab(int textureIndex)
        {
            if (terrainBehavior == null)
            {
                Debug.LogError("TerrainBehavior is not assigned.");
                return null;
            }

            foreach (var terrain in terrainBehavior.terrainDigParticlePrefabs)
                if (terrain.terrainLayerIndex == textureIndex)
                    return terrain.primaryPrefab;


            return terrainBehavior.defaultDigParticlePrefab;
        }

        [CanBeNull]
        public GameObject GetSecondaryTerrainPrefab(int textureName)
        {
            if (terrainBehavior == null)
            {
                Debug.LogError("TerrainBehavior is not assigned.");
                return null;
            }

            foreach (var terrain in terrainBehavior.terrainDigParticlePrefabs)
                if (terrain.terrainLayerIndex == textureName)
                    return terrain.secondaryPrefab;


            return null;
        }
    }
}