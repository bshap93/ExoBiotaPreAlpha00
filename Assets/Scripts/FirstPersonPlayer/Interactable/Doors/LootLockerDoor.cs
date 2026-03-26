using UnityEngine;

namespace FirstPersonPlayer.Interactable.Doors
{
    public class LootLockerDoor : GenericDoor
    {
        [SerializeField] GameObject innerLight;

        public override void Interact()
        {
            base.Interact();
            innerLight.SetActive(true);
        }
    }
}
