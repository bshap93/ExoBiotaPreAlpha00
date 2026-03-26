using JetBrains.Annotations;
using MoreMountains.Tools;

namespace Helpers.Events.ManagerEvents
{
    public enum GamePOIEventType
    {
        MakeAlwaysVisible,
        UnmakeAlwaysVisible,
        MakeWellKnown,
        MakeLittleKnown,
        ParentDestroyed,
        POIWasAreaScanned,
        POIMarkedAsHavingNewContent,
        POIWithNewContentMarkedAsVisited,
        MarkPOIAsTrackedByObjective,
        UnmarkPOIAsTrackedByObjective
    }

    public enum CanBeAreaScannedType
    {
        BasicScanner,
        BasicBioScanner,
        NotDetectableByScan
    }

    public struct GamePOIEvent
    {
        static GamePOIEvent _e;

        public string UniqueId;
        public GamePOIEventType GamePOIEventTypeValue;
        public string SceneName;
        [CanBeNull] public string OtherParam;

        public static void Trigger(string uniqueId, GamePOIEventType eventType, string sceneName,
            string otherParam = null)
        {
            _e.UniqueId = uniqueId;
            _e.GamePOIEventTypeValue = eventType;
            _e.SceneName = sceneName;
            _e.OtherParam = otherParam;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
