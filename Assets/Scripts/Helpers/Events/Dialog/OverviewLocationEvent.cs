using MoreMountains.Tools;
using UnityEngine;

namespace Events
{
    public enum LocationType
    {
        Trader,
        Mine,
        Laboratory,
        Dirigible,
        NpcResidence,
        MiscNpc,
        Any,
        CentralGreetingLocation
    }

    public enum LocationActionType
    {
        Approach,
        RetreatFrom,
        BeApproached
    }

    public struct OverviewLocationEvent
    {
        static OverviewLocationEvent _e;

        public LocationType LocationType;
        public LocationActionType LocationActionType;
        public string LocationId;
        public Transform CameraTransform;
        public string StartNodeOverride;


        public static void Trigger(LocationType locationType, LocationActionType locationActionType,
            string locationId, Transform cameraTransform, string startNodeOverride = null
        )
        {
            _e.LocationType = locationType;
            _e.LocationActionType = locationActionType;

            _e.LocationId = locationId;
            _e.CameraTransform = cameraTransform;
            _e.StartNodeOverride = startNodeOverride;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
