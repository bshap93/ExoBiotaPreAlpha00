using System;
using MoreMountains.Tools;
using PhysicsHandlers.Triggers;

namespace Helpers.Events.Triggering
{
    [Serializable]
    public enum TriggerColliderEventType
    {
        SetTriggerable
    }


    public struct TriggerColliderEvent
    {
        static TriggerColliderEvent _e;

        public string ColliderID;
        public TriggerColliderEventType EventType;
        public bool IsTriggerable;
        public TriggerColliderType ColliderType;


        public static void Trigger(string colliderID, TriggerColliderEventType eventType, bool isTriggerable,
            TriggerColliderType colliderType)
        {
            _e.ColliderID = colliderID;
            _e.EventType = eventType;
            _e.IsTriggerable = isTriggerable;
            _e.ColliderType = colliderType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
