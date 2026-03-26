using Helpers.Events;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Utilities.Static
{
    public static class SaveSystemTriggers
    {
#if UNITY_EDITOR
        [MenuItem("Debug/Clear All Save Data")]
#endif
        public static void ClearAllSaveData()
        {
            ResetDataEvent.Trigger();
        }


#if UNITY_EDITOR

        [MenuItem("Debug/Save All Data")]
#endif
        public static void SaveAllData()
        {
            SaveDataEvent.Trigger();
            AlertEvent.Trigger(AlertReason.SavingGame, "All data saved successfully!", "Saved Game");
        }
    }
}
