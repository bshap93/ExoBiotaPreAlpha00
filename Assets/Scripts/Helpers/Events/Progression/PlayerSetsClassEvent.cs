using MoreMountains.Tools;

namespace Helpers.Events.Progression
{
    public struct PlayerSetsClassEvent
    {
        static PlayerSetsClassEvent _e;

        public int ClassId;
        public static void Trigger(int classId)
        {
            _e.ClassId = classId;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
