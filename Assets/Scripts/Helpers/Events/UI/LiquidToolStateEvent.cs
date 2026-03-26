using System;
using MoreMountains.Tools;
using UnityEngine;

namespace Helpers.Events.UI
{
    [Serializable]
    public enum LiquidToolStateEventType
    {
        UpdatedIchorCharges,
        EquippedLiquidTool,
        UnequippedLiquidTool
    }

    public struct LiquidToolStateEvent
    {
        static LiquidToolStateEvent _e;

        public LiquidToolStateEventType Type;
        public int CurrentIchorCharges;
        public int MaxIchorCharges;

        public static void Trigger(LiquidToolStateEventType liquidToolStateEventType, int currentIchorCharges,
            int maxIchorCharges)
        {
            _e.Type = liquidToolStateEventType;
            _e.CurrentIchorCharges = currentIchorCharges;
            _e.MaxIchorCharges = maxIchorCharges;

            MMEventManager.TriggerEvent(_e);
        }

        public static void Trigger(LiquidToolStateEventType liquidToolStateEventType)
        {
            if (liquidToolStateEventType == LiquidToolStateEventType.UpdatedIchorCharges)
            {
                Debug.LogWarning(
                    "Current and max ichor charges should be provided when triggering an UpdatedIchorCharges event.");

                return;
            }

            _e.Type = liquidToolStateEventType;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
