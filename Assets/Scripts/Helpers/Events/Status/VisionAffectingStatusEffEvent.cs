using MoreMountains.Tools;

namespace Helpers.Events.Status
{
    public enum VisionAffectingStatusEffType
    {
        Distortion,
        Floaters,
        All
    }

    public struct VisionAffectingStatusEffEvent
    {
        static VisionAffectingStatusEffEvent _e;

        public VisionAffectingStatusEffType StatusEffType;
        public bool Enable;

        public static void Trigger(VisionAffectingStatusEffType statusEffType, bool enable)
        {
            _e.StatusEffType = statusEffType;
            _e.Enable = enable;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
