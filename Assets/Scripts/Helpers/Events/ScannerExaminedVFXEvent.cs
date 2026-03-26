using MoreMountains.Tools;
using UnityEngine;

namespace Helpers.Events
{
    // Broadcast when a scan-based examination completes on a specific target
    public struct ScannerExaminedVFXEvent
    {
        private static ScannerExaminedVFXEvent _e;
        public string TargetId; // HighlightEffectController.targetID (preferred for matching)
        public Transform Target; // Fallback, if you prefer transform-based matching
        public float Duration; // How long to show any special VFX (optional)

        public static void Trigger(string targetId, Transform target, float duration = 0f)
        {
            _e.TargetId = targetId;
            _e.Target = target;
            _e.Duration = duration;
            MMEventManager.TriggerEvent(_e);
        }
    }
}