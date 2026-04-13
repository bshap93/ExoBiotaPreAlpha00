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
        public int Willpower { get; set; }

        public static void Trigger(int strength, int agility, int dexterity, int bioticLevel, int toughness,
            int willpower)
        {
            _e.Strength = strength;
            _e.Agility = agility;
            _e.Dexterity = dexterity;
            _e.BioticLevel = bioticLevel;
            _e.Toughness = toughness;
            _e.Willpower = willpower;

            MMEventManager.TriggerEvent(_e);
        }
    }
}
