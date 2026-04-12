using FirstPersonPlayer.Interactable.ResourceBoxes;
using Manager.StateManager;
using MoreMountains.Tools;

namespace Helpers.Events.Machinery
{
    public enum ResourceContainerStateEventType
    {
        SetNewResourceContainerState
    }

    public struct ResourceContainerInitStateEvent
    {
        static ResourceContainerInitStateEvent _e;

        public string UniqueID;
        public ResourceContainerStateEventType EventType;
        public ResourceCollectionContainerInteractable.ResourceType ResourceType;
        public ResourceContainerManager.ResourceContainerInitializationState ResourceContainerInitializationState;

        public static void Trigger(ResourceContainerStateEventType eventType,
            ResourceCollectionContainerInteractable.ResourceType resourceType,
            ResourceContainerManager.ResourceContainerInitializationState initState, string uniqueID)
        {
            _e.EventType = eventType;
            _e.ResourceType = resourceType;
            _e.ResourceContainerInitializationState = initState;
            _e.UniqueID = uniqueID;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
