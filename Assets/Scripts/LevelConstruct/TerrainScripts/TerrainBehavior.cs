using System;
using System.Collections.Generic;
using FirstPersonPlayer.Tools;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace TerrainScripts
{
    [Serializable]
    public class TerrainLayerChoices
    {
        public int terrainLayerIndex;
        public int terrainToUseInstead; // or whatever type you need
    }

    [Serializable]
    public class TerrainDigParticlePrefab
    {
        public int terrainLayerIndex;
        [FormerlySerializedAs("prefab")] public GameObject primaryPrefab;
        [CanBeNull] public GameObject secondaryPrefab;
    }

    [Serializable]
    public class DefaultLayerAboveDepth
    {
        public float playerDepth;
        public int defaultLayerIndex;
        public int[] alternateAcceptableLayerIndices;
    }

    [CreateAssetMenu(fileName = "TerrainBehavior", menuName = "Scriptable Objects/TerrainBehavior")]
    public class TerrainBehavior : ScriptableObject
    {
        public List<ToolTerrainLayerMapping> toolLayerMappings;

        [Header("Terrain Layer Choices")] [FormerlySerializedAs("statEntries")]
        public List<TerrainLayerChoices> terrainChoices;

        public List<DefaultLayerAboveDepth> defaultLayerAboveDepths;

        [Header("Dig Particle Prefabs")] public List<TerrainDigParticlePrefab> terrainDigParticlePrefabs;
        public GameObject defaultDigParticlePrefab;

        private Dictionary<ToolType, int[]> _allowedTerrainTextureIndices;

        private void OnEnable()
        {
            _allowedTerrainTextureIndices = new Dictionary<ToolType, int[]>();

            foreach (var mapping in toolLayerMappings)
                if (!_allowedTerrainTextureIndices.ContainsKey(mapping.toolType))
                    _allowedTerrainTextureIndices.Add(mapping.toolType, mapping.allowedTerrainIndices);
        }

        public int[] GetAllowedTerrainTextureIndices(ToolType toolType)
        {
            return _allowedTerrainTextureIndices.TryGetValue(toolType, out var indices)
                ? indices
                : Array.Empty<int>();
        }

        [Serializable]
        public class ToolTerrainLayerMapping
        {
            public ToolType toolType;
            public int[] allowedTerrainIndices;
        }
    }
}