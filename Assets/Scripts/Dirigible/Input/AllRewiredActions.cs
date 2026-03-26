using System.Collections.Generic;
using SharedUI;
using Sirenix.OdinInspector;

namespace Dirigible.Input
{
    public static class AllRewiredActions
    {
#if UNITY_EDITOR
        // This will be called from the parent ScriptableObject
        public static IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            var parent = ControlsPromptSchemeSet._currentContextSO;
            if (parent == null || parent.inputManagerPrefab == null) yield break;

            var data = parent.inputManagerPrefab.userData;
            if (data == null) yield break;

            try
            {
                if (data.GetActions_Copy() == null)
                    yield break;
            }
            catch
            {
                yield break;
            }

            foreach (var action in data.GetActions_Copy())
                yield return new ValueDropdownItem<int>(action.name, action.id);
        }
#endif
    }
}
