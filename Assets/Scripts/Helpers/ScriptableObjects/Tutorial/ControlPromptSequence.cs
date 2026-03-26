using System;
using System.Collections.Generic;
using SharedUI;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.ScriptableObjects.Tutorial
{
    [Serializable]
    public class ControlsPromptInfo
    {
#if UNITY_EDITOR

        [FormerlySerializedAs("ActionId")] [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionBeingPrompted;
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

    [CreateAssetMenu(
        fileName = "ControlPromptSequence", menuName = "Scriptable Objects/UI/ControlPromptSequence",
        order = 1)]
    public class ControlPromptSequence : ScriptableObject
    {
        [FormerlySerializedAs("ControlPromptSequenceID")]
        public string controlPromptSequenceID;
        public List<ControlsPromptInfo> controlPromptsInSequence;
    }
}
