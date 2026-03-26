using MoreMountains.Tools;
using SharedUI.Interface;

namespace Helpers.Events
{
    public enum BillboardEventType
    {
        Show,
        Hide,
        Update
    }

    public struct BillboardEvent
    {
        private static BillboardEvent _e;

        public SceneObjectData SceneObjectData;
        public BillboardEventType EventType;


        public static void Trigger(SceneObjectData sceneObjectData, BillboardEventType eventType)
        {
            _e.EventType = eventType;
            _e.SceneObjectData = sceneObjectData;

            MMEventManager.TriggerEvent(_e);
        }
    }
}