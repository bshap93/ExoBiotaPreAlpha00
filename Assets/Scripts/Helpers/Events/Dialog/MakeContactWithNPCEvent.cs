using MoreMountains.Tools;

namespace Helpers.Events.Dialog
{
    public struct MakeContactWithNPCEvent
    {
        static MakeContactWithNPCEvent _e;

        public string NPCId;

        public static void Trigger(string npcId)
        {
            _e.NPCId = npcId;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
