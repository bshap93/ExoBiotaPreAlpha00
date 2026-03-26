using JetBrains.Annotations;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace Interactable
{
    public abstract class DoorsController : MonoBehaviour
    {
        public GameObject RDoor;
        public GameObject LDoor;

        [CanBeNull] public MMFeedbacks OpenDoorFeedbacks;
        [CanBeNull] public MMFeedbacks CloseDoorFeedbacks;

        [SerializeField] protected ConditionalDoor conditionalDoor;
        protected bool isOpen;

        private void Start()
        {
            if (conditionalDoor != null)
                if (conditionalDoor.startActive)
                    OpenDoors();
        }


        protected void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                if (conditionalDoor != null && conditionalDoor.GetLockedState()) return;
                OpenDoors();
            }
        }

        protected void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("Player"))
                CloseDoors();
        }

        public abstract void OpenDoors();

        public abstract void CloseDoors();
    }
}