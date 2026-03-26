using MoreMountains.Tools;

namespace Helpers.Events.Spawn
{
    public struct ContaminationSpikeEvent
    {
        static ContaminationSpikeEvent _e;

        public float NewContaminationAmt;

        public static void Trigger(float newContaminationAmt)
        {
            _e.NewContaminationAmt = newContaminationAmt;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
