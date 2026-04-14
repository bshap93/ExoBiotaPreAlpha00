using System.Collections.Generic;
using System.Linq;
using JournalData.JournalTopics;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.ScriptableObjects.Scenario
{
    [CreateAssetMenu(
        fileName = "ScenarioDefinition", menuName = "Scriptable Objects/Scenario/Scenario Definition",
        order = 0)]
    public class ScenarioDefinition : ScriptableObject
    {
        [Header("Reference")] [ReadOnly] [InfoBox("Auto-set from asset name. Rename the asset to change this.")]
        public string scenarioUniqueID;
        public string scenarioName;
        [Required] [AssetsOnly] public JournalTopic scenarioJournalTopic;

        [Header("Data Definitions")]
        [ValidateInput("HasNoDuplicates", "List contains duplicate flag names")]
        [ListDrawerSettings(ShowItemCount = true, DraggableItems = false)]
        public List<string> booleanFlags;
        [ValidateInput("HasNoDuplicates", "List contains duplicate flag names")] [FormerlySerializedAs("intFlags")]
        public List<string> intCounters;
#if UNITY_EDITOR
        void OnValidate()
        {
            if (scenarioUniqueID != name)
            {
                scenarioUniqueID = name;
                EditorUtility.SetDirty(this);
            }
        }
#endif

#if UNITY_EDITOR
        bool HasNoDuplicates(List<string> list)
        {
            return list == null || list.Count == list.Distinct().Count();
        }
#endif
    }
}
