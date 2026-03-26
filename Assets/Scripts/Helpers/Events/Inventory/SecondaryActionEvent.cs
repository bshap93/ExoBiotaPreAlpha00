using FirstPersonPlayer.Tools.Interface;
using MoreMountains.Tools;

namespace Helpers.Events.Inventory
{
    public struct SecondaryActionEvent
    {
        static SecondaryActionEvent _e;

        public SecondaryActionType SecondaryActionType;

        public static void Trigger(SecondaryActionType injectAvailableIchor)
        {
            _e.SecondaryActionType = injectAvailableIchor;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
