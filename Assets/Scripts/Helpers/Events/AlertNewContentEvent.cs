using System;
using MoreMountains.Tools;

namespace Helpers.Events
{
    [Serializable]
    public enum NewContentType
    {
        ObjectiveCompleted,
        ObjectiveOffer
    }

    [Serializable]
    public struct AlertNewContentEvent
    {
        private static AlertNewContentEvent _e;
        public NewContentType Type;
        public string LocationId;
        public string DockId;

        public void Trigger(NewContentType type, string locationId, string dockId)
        {
            _e.Type = type;
            _e.LocationId = locationId;
            _e.DockId = dockId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}