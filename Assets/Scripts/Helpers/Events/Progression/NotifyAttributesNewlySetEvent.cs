using MoreMountains.Tools;

namespace Helpers.Events.Progression
{
    public struct NotifyAttributesNewlySetEvent
    {
        static NotifyAttributesNewlySetEvent _e;

        public int Strength;
        public int Agility;
        public int Dexterity;
        public int BioticLevel;
        public int Toughness { get; set; }

        public static void Trigger(int strength, int agility, int dexterity, int bioticLevel, int toughness)
        {
            _e.Strength = strength;
            _e.Agility = agility;
            _e.Dexterity = dexterity;
            _e.BioticLevel = bioticLevel;
            _e.Toughness = toughness;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
