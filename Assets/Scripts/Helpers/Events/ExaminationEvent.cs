using MoreMountains.Tools;
using OWPData.DataClasses;

namespace Helpers.Events
{
    public enum ExaminableItemType
    {
        Ore,
        Pickable,
        Biological
    }

    public struct ExaminationEvent
    {
        private static ExaminationEvent _e;

        public ExaminableItemType SceneObjectType;
        public ExaminableObjectData Data;

        public static void Trigger(ExaminableItemType sceneObjectType, ExaminableObjectData data)
        {
            _e.SceneObjectType = sceneObjectType;
            _e.Data = data;
            MMEventManager.TriggerEvent(_e);
        }
    }
}