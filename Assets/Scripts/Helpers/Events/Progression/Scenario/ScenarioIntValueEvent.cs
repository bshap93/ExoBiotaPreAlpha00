using MoreMountains.Tools;

namespace Helpers.Events.Progression
{
    public struct ScenarioIntValueEvent
    {
        public enum ScenarioDataEventType
        {
            SetValue,
            IncrementValue,
            DecrementValue
        }

        static ScenarioIntValueEvent _e;

        public string ScenarioUniqueID;
        public string KeyId;
        public ScenarioDataEventType EventType;
        public int ValueId;

        public static void Trigger(ScenarioDataEventType eventType, string scenarioUniqueID, string keyId, int valueId)
        {
            _e.EventType = eventType;
            _e.ScenarioUniqueID = scenarioUniqueID;
            _e.KeyId = keyId;
            _e.ValueId = valueId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
