using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Gated;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.MediStat
{
    public class InfectedMediStat : MediStatHub, IInteractable
    {
        public override void Interact()
        {
            if (CanInteract())
            {
                MyUIEvent.Trigger(UIType.LevelingUIInfected, UIActionType.Open);
                GatedLevelingEvent.Trigger(GatedInteractionEventType.TriggerGateUI, null);
                BillboardEvent.Trigger(null, BillboardEventType.Hide);
            }
            else
            {
                AlertWhyCant();
            }
        }


        void QuitGame()
        {
            Application.Quit();
        }
    }
}
