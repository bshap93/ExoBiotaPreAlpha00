using System;
using FirstPersonPlayer.ScriptableObjects;
using MoreMountains.Tools;

namespace Helpers.Events
{
    public enum BioSampleEventType
    {
        StartCollection,
        CompleteCollection,
        CompletedSequencing,
        StartSequencing,
        RefreshUI,
        Abort,
        GiveToNPC
    }

    [Serializable]
    public struct BioSampleEvent
    {
        static BioSampleEvent _e;

        public string UniqueID;
        public BioOrganismType BioOrganismType;


        public BioSampleEventType EventType;
        public float Duration; // only meaningful for Start

        public static void Trigger(string uniqueID, BioSampleEventType bioSampleEventType,
            BioOrganismType bioOrganismType, float duration)
        {
            _e.UniqueID = uniqueID;
            _e.EventType = bioSampleEventType;
            _e.BioOrganismType = bioOrganismType;
            _e.Duration = duration;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
