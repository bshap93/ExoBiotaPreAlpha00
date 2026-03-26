using System;
using MoreMountains.Tools;

namespace Helpers.Events
{
    [Serializable]
    public enum AudioEventType
    {
        PauseAudio,
        UnPauseAudio
    }

    public struct AudioEvent
    {
        static AudioEvent _e;
        public AudioEventType EventType;

        public static void Trigger(AudioEventType eventType)
        {
            _e.EventType = eventType;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
