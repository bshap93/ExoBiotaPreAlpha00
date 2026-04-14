using MoreMountains.Tools;

namespace Helpers.Events.Progression.Scenario
{
    public struct ScenarioBoolValueEvent
    {
        static ScenarioBoolValueEvent _e;

        public string ScenarioUniqueID;
        public string KeyId;
        public bool Value;

        public void Trigger(string scenarioUniqueID, string keyId, bool value)
        {
            _e.ScenarioUniqueID = scenarioUniqueID;
            _e.KeyId = keyId;
            _e.Value = value;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
