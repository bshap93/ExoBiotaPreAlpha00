using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Overview.NPC
{
    [CreateAssetMenu(fileName = "NpcDatabase", menuName = "Scriptable Objects/Character/NpcDatabase", order = 1)]
    public class NpcDatabase : ScriptableObject
    {
        public NpcDefinition[] npcDefinitions;
        Dictionary<string, NpcDefinition> _map; // npcId → definition

        void OnEnable()
        {
            _map = npcDefinitions.ToDictionary(n => n.npcId, n => n);
        }

        void ReInitializeMap()
        {
            if (npcDefinitions != null)
                _map = npcDefinitions.ToDictionary(n => n.npcId, n => n);
            else
                _map = new Dictionary<string, NpcDefinition>();
        }

        public bool TryGet(string id, out NpcDefinition def)
        {
            if (_map == null)
                ReInitializeMap();

            if (_map != null)
                return _map.TryGetValue(id, out def);

            def = null;
            return false;
        }

        public IEnumerable<NpcDefinition> GetAll()
        {
            if (_map == null)
            {
                Debug.LogError("NpcDatabase not initialized. Call OnEnable first.");
                return Enumerable.Empty<NpcDefinition>();
            }

            return _map.Values;
        }

        // New methods for inspector dropdowns
        public string[] GetAllNpcIds()
        {
            return new[]
            {
                "ScientistHypolita", "CheckpointSoldier", "NavigationServer", "MetaTerminalServer",
                "WombKeeper", "WombSquire", "OutcastKinMotile", "HospitableFlora", "FloraNimensis",
                "ElevatorSystemServer01", "MedistatPodServer", "NarratorInnerVoice", "SlaverHylic01",
                "GuardCharacterHybrid01",
                "MinerCharacterHybrid01"
            };
        }

        public string[] GetStartNodesForNpc(string npcId)
        {
            // Ensure map is initialized
            if (_map == null)
            {
                if (npcDefinitions != null && npcDefinitions.Length > 0)
                    _map = npcDefinitions.ToDictionary(n => n.npcId, n => n);
                else
                    return new string[] { };
            }

            if (!_map.TryGetValue(npcId, out var def))
            {
                Debug.LogWarning($"NPC '{npcId}' not found in database");
                return new string[] { };
            }

            if (def.availableStartNodes == null || def.availableStartNodes.Length == 0)
            {
                Debug.LogWarning($"NPC '{npcId}' has no availableStartNodes defined");
                return new string[] { };
            }

            return def.availableStartNodes;
        }
        public NpcDefinition GetDefinitionByID(string eventTypeNPCId)
        {
            if (_map == null)
            {
                if (npcDefinitions != null && npcDefinitions.Length > 0)
                {
                    _map = npcDefinitions.ToDictionary(n => n.npcId, n => n);
                }
                else
                {
                    Debug.LogError("NpcDatabase not initialized and npcDefinitions is empty. Call OnEnable first.");
                    return null;
                }
            }

            if (_map.TryGetValue(eventTypeNPCId, out var def))
                return def;

            Debug.LogWarning($"NPC '{eventTypeNPCId}' not found in database");
            return null;
        }
    }
}
