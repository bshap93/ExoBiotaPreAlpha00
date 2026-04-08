using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using MoreMountains.Tools;

namespace Helpers.Events.Creature
{
    public struct CreatureStateChangeEvent
    {
        static CreatureStateChangeEvent _e;
        public CreatureController.CreatureState NewState;
        public string CreatureUniqueId;
        public string CreatureName;

        public static void Trigger(string creatureUniqueId, string creatureName,
            CreatureController.CreatureState newState)
        {
            _e.CreatureUniqueId = creatureUniqueId;
            _e.CreatureName = creatureName;
            _e.NewState = newState;
            MMEventManager.TriggerEvent(_e);
        }
    }
}
