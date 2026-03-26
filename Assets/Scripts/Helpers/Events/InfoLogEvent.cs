using System;
using Helpers.ScriptableObjects;
using MoreMountains.Tools;

namespace Helpers.Events
{
    [Serializable]
    public enum InfoLogEventType
    {
        SetInfoLogContent
    }

    public struct InfoLogEvent
    {
        static InfoLogEvent _e;

        public InfoLogContent InfoLogContent;
        public InfoLogEventType InfoLogEventType;

        public static void Trigger(InfoLogContent infoLogContent, InfoLogEventType infoLogEventType)
        {
            _e.InfoLogContent = infoLogContent;
            _e.InfoLogEventType = infoLogEventType;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
