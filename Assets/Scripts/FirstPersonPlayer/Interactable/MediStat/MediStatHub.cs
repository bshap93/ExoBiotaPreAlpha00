using System;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Gated;
using LevelConstruct.Interactable.ItemInteractables;
using Manager;
using MoreMountains.Feedbacks;
using SharedUI.Interface;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable.MediStat
{
    public class MediStatHub : ActionConsole, IRequiresUniqueID, IInteractable, IBillboardable
    {
        [SerializeField] AnimationClip openAnimation;
        [SerializeField] AnimationClip closeAnimation;
        [SerializeField] MMFeedbacks openFeedbacks;
        [SerializeField] MMFeedbacks closeFeedbacks;

        public override Sprite GetIcon()
        {
            return ExaminationManager.Instance.iconRepository.mediStatHubIcon;
        }
        public override string ShortBlurb()
        {
            return "A rest station capable of bio-core augments.";
        }
        public override Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.mediStatHubRestIcon;
        }
        public override string GetActionText()
        {
            return "Utilize";
        }
        public override void Interact()
        {
            if (CanInteract())
            {
                MyUIEvent.Trigger(UIType.LevelingUI, UIActionType.Open);
                GatedLevelingEvent.Trigger(GatedInteractionEventType.TriggerGateUI, null);
                BillboardEvent.Trigger(null, BillboardEventType.Hide);
            }
            else
            {
                AlertWhyCant();
            }
        }
        public override void OnInteractionStart()
        {
        }
        protected void AlertWhyCant()
        {
            if (currentConsoleState == ActionConsoleState.Broken)
                AlertEvent.Trigger(
                    AlertReason.BrokenMachine, "The medi-stat hub console is broken and cannot be used.",
                    "Medi-Stat Console");
            else if (currentConsoleState == ActionConsoleState.LacksPower)
                AlertEvent.Trigger(
                    AlertReason.MachineLacksPower, "The medi-stat hub console lacks power and cannot be used.",
                    "Medi-Stat Console");
        }


        public override void OnInteractionEnd()
        {
        }
        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Utilize";
        }
        public override void SetConsoleToLacksPowerState()
        {
            currentConsoleState = ActionConsoleState.LacksPower;
        }
        public override void SetConsoleToPoweredOnState()
        {
            currentConsoleState = ActionConsoleState.PoweredOn;
        }
        public override void SetConsoleToHailPlayerState()
        {
            throw new NotImplementedException();
        }
    }
}
