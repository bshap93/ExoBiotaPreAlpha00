using System;
using MoreMountains.Tools;

namespace Helpers.Events.Dialog
{
    public enum LanguageType
    {
        ModernGalactic,
        Sheolite
    }

    [Serializable]
    public enum DialoguePresentationEventType
    {
        ChangeFontsOfNPCSide
    }

    public struct DialoguePresentationEvent
    {
        static DialoguePresentationEvent _e;

        public DialoguePresentationEventType EventType;

        public LanguageType Language;


        public static void Trigger(DialoguePresentationEventType eventType, LanguageType language)
        {
            _e.Language = language;
            _e.EventType = eventType;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
