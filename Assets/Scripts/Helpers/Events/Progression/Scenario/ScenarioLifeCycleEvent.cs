using MoreMountains.Tools;

namespace Helpers.Events.Progression.Scenario
{
    public enum ScenarioLifeCycleEventType
    {
        ScenarioStarted,
        ScenarioFinished
    }


    public struct ScenarioLifeCycleEvent
    {
        static ScenarioLifeCycleEvent _e;

        public ScenarioLifeCycleEventType LifeCycleEventType;

        public string ScenarioUniqueID;

        public static void Trigger(ScenarioLifeCycleEventType lifeCycleEventType, string scenarioUniqueID)
        {
            _e.LifeCycleEventType = lifeCycleEventType;
            _e.ScenarioUniqueID = scenarioUniqueID;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
