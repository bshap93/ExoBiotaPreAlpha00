using System;
using MoreMountains.Tools;

namespace Helpers.Events
{
    [Serializable]
    public enum ScannerEventType
    {
        ScannedObject,
        ScannerCalibrated,
        ScanEnded,
        ScanStarted,
        ExaminationStart,
        ExaminationEnd
    }

    public struct ScannerEvent
    {
        private static ScannerEvent _e;

        public ScannerEventType ScannerEventType;
        public float Duration;


        public static void Trigger(ScannerEventType toolType, float duration = 0f)
        {
            _e.ScannerEventType = toolType;
            _e.Duration = duration;
            MMEventManager.TriggerEvent(_e);
        }
    }
}