using System;
using System.Collections.Generic;
using Helpers.Events;
using Manager.DialogueScene;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Objectives.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "ObjectiveObject", menuName = "Scriptable Objects/Objectives/ObjectiveObject",
        order = 1)]
    public class ObjectiveObject : ScriptableObject
    {
        [Serializable]
        public enum TriggersOnObjectiveLifecycleEvent
        {
            OnAdd,
            OnActivate,
            OnComplete
        }

        [Header("Objective ID")] public ObjectiveType objectiveType;

        [SerializeField] SubjectLocationObject associatedLocation;

        [SerializeField] [OnValueChanged("OnObjectiveIdChanged")]
        public string objectiveId = "UnnamedObjective";

        public bool punctuatedCompletion;
        public bool punctuatedAddAndActivate;

        [Header("Objective Details")] [SerializeField]
        public string objectiveText;

        public Sprite objectiveImage;

        public bool shouldBeMadeActiveOnAdd = true;

        public List<ObjectiveObject> prerequisiteObjectives;


        [Header("NPC Linking")] [ValueDropdown("GetNpcIdOptions", IsUniqueList = true)]
        public string offerNpcId;

        // [FormerlySerializedAs("activateWhenCompleted")] [SerializeField]
        // public string[] addWhenCompleted;
        public ObjectiveObject activateUponCompletion;

        [SerializeField] public int initialProgress;
        [SerializeField] public int targetProgress = 1;

        public ObjectiveCategoryType catalogContext;
        public ObjectiveProgressType objectiveProgressType = ObjectiveProgressType.None;

        [Header("Spontaneous Event Triggering")] [ToggleLeft] [LabelText("Triggers Spontaneous Event?")]
        public bool triggersSpontaneousEvent;
        [ShowIf(nameof(triggersSpontaneousEvent))]
        public TriggersOnObjectiveLifecycleEvent triggersOnEvent;
        [ShowIf(nameof(triggersSpontaneousEvent))]
        public int spontaneousEventIntParameter;
        [ShowIf(nameof(triggersSpontaneousEvent))]
        public string spontaneousEventStringParameter;
        [ShowIf(nameof(triggersSpontaneousEvent))]
        public string spontaneousEventUniqueId;
        [ShowIf(nameof(triggersSpontaneousEvent))]
        public SpontaneousTriggerEventType spontaneousEventType;
        [ShowIf(nameof(triggersSpontaneousEvent))]
        public string secondarySpontaneousStringParameter;

        public string GetPOIUniqueId()
        {
            return associatedLocation ? associatedLocation.associatedPOIUniqueId : null;
        }


        static string[] GetNpcIdOptions()
        {
            return DialogueManager.GetAllNpcIdOptions();
        }


        // ------------- RENAME-&-SYNC UTILITIES -------------
#if UNITY_EDITOR
        /// <summary>Called automatically whenever the asset or its fields change.</summary>
        string _lastKnownName;

        void OnValidate()
        {
            // Detect if the asset was renamed externally (header bar, AssetDatabase, etc.)
            if (_lastKnownName != name)
            {
                objectiveId = name; // sync ID to match external rename
                _lastKnownName = name;
                EditorUtility.SetDirty(this);
            }
        }

        void OnObjectiveIdChanged()
        {
            if (string.IsNullOrWhiteSpace(objectiveId)) return;

            // Sync asset name to ID
            name = objectiveId;
            _lastKnownName = name;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }

        [Button("Rename Asset ↔ ID", ButtonSizes.Small)]
        void RenameAssetToId()
        {
            if (string.IsNullOrWhiteSpace(objectiveId))
            {
                Debug.LogWarning("[ObjectiveObject] objectiveId is empty; rename cancelled.");
                return;
            }

            name = objectiveId; // ensures sub-asset name matches the field
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
