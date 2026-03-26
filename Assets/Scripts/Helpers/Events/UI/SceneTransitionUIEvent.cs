using System;
using MoreMountains.Tools;

namespace Helpers.Events.UI
{
    [Serializable]
    public enum SceneTransitionUIEventType
    {
        Show,
        Hide
    }

    public struct SceneTransitionUIEvent
    {
        static SceneTransitionUIEvent _e;
        public SceneTransitionUIEventType EventType;


        public static void Trigger(SceneTransitionUIEventType eventType)
        {
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
