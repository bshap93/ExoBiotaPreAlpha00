using System;

namespace Helpers.Events
{
    [Serializable]
    public enum InteractableType
    {
        Button
    }

    [Serializable]
    public enum InteractableEventType
    {
        Interacted,
        Focused,
        Unfocused
    }

    public struct InteractableEvent
    {
        static InteractableEvent _e;
    }
}
