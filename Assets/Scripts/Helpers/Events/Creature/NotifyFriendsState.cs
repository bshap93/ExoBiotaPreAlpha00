using FirstPersonPlayer.FPNPCs.AlienNPC;
using MoreMountains.Tools;

namespace Helpers.Events.Combat
{
    public struct AlienNotifyFriendsOfStateEvent
    {
        static AlienNotifyFriendsOfStateEvent _e;
        
        public string UniqueID;
        
        public bool IsHostile;
        public AlienNPCState NewState;

        public static void Trigger(string uniqueID, bool isHostile, AlienNPCState newState)
        {
            _e.UniqueID = uniqueID;
            _e.IsHostile = isHostile;
            _e.NewState = newState;
            
            MMEventManager.TriggerEvent(_e);
        }

    }
}
