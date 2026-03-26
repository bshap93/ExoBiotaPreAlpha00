using Manager;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.HoloInteractable
{
    public class BioContainerDialogueInteractable : DialogueInteractable
    {
        public string bioContainerName;
        public override string GetName()
        {
            return bioContainerName;
        }
        public override Sprite GetIcon()
        {
            return PlayerUIManager.Instance.defaultIconRepository.bioContainerIcon;
        }
    }
}
