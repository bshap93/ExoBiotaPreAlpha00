using System;
using System.Collections.Generic;
using Rewired;
using Sirenix.OdinInspector;
using UnityEngine;

namespace SharedUI
{
    [Serializable]
    public struct ControlsPromptDatum
    {
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int ActionId;

        public string PromptText;
        public Sprite PromptIcon;
        public string AdditionalContext;

#if UNITY_EDITOR
        // This will be called from the parent ScriptableObject
        IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            var parent = ControlsPromptSchemeSet._currentContextSO;
            if (parent == null || parent.inputManagerPrefab == null) yield break;

            var data = parent.inputManagerPrefab.userData;
            if (data == null) yield break;

            foreach (var action in data.GetActions_Copy())
                yield return new ValueDropdownItem<int>(action.name, action.id);
        }
#endif
    }

    [CreateAssetMenu(fileName = "ControlsPromptSchemeSets", menuName = "Scriptable Objects/ControlsPromptSchemeSets")]
    public class ControlsPromptSchemeSet : ScriptableObject
    {
        [SerializeField] internal InputManager inputManagerPrefab;
        public List<ControlsPromptDatum> ControlsPromptSet;

        public ControlsPromptDatum GetDatumByActionId(int eventTypeActionId)
        {
            var datum = ControlsPromptSet.Find(d => d.ActionId == eventTypeActionId);

            return datum;
        }

#if UNITY_EDITOR
        // static context pointer so child dropdowns know which SO they belong to
        internal static ControlsPromptSchemeSet _currentContextSO;

        void OnValidate()
        {
            _currentContextSO = this;
        }
#endif
    }
}
