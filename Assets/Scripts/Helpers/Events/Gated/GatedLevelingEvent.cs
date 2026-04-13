using System;
using MoreMountains.Tools;
using UnityEngine.Serialization;

namespace Helpers.Events.Gated
{
    [Serializable]
    public class NewAttributeValues
    {
        [FormerlySerializedAs("Strength")] public int strength;
        [FormerlySerializedAs("Agility")] public int agility;
        [FormerlySerializedAs("Dexterity")] public int dexterity;
        [FormerlySerializedAs("mentalToughness")] [FormerlySerializedAs("MentalToughness")]
        public int toughness;
        [FormerlySerializedAs("Exobiotic")] public int exobiotic;
        public int willpower;
    }

    public struct GatedLevelingEvent
    {
        static GatedLevelingEvent _e;
        public GatedInteractionEventType EventType;

        public NewAttributeValues AttributeValues;

        public static void Trigger(GatedInteractionEventType eventType, NewAttributeValues newAttributeValues)
        {
            _e.EventType = eventType;
            _e.AttributeValues = newAttributeValues;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
