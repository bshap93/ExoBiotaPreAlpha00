using MoreMountains.Tools;
using UnityEngine;

namespace Events
{
    public struct EquipmentReadyEvent
    {
        public static EquipmentReadyEvent _e;

        public Transform Anchor;

        public static void Trigger(Transform anchor)
        {
            _e.Anchor = anchor;
            MMEventManager.TriggerEvent(_e);
        }
    }
}