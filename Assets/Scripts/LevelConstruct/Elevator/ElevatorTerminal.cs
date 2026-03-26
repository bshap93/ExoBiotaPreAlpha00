using LevelConstruct.Interactable;
using UnityEngine.Serialization;

namespace LevelConstruct.Elevator
{
    public class ElevatorTerminal : Terminal
    {
        // Elevators that run vertically in a system will share a system name
        [FormerlySerializedAs("ElevatorSystemName")]
        public string elevatorSystemName;
        public bool initiallyActive = true;

        public bool IsActive { get; private set; }

        void Start()
        {
            IsActive = initiallyActive;
        }
        public override string GetName()
        {
            return elevatorSystemName;
        }
        public override string ShortBlurb()
        {
            return $"Access {elevatorSystemName} Elevator System";
        }

        public override bool CanInteract()
        {
            return IsActive;
        }
        public override bool IsInteractable()
        {
            return true;
        }
    }
}
