using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.ScriptableObjects;
using LevelConstruct.Highlighting;
using UnityEngine;

namespace LevelConstruct.Interactable.ItemInteractables
{
    [DisallowMultipleComponent]
    public class InfoLogTablet : ActionConsole, IInteractable
    {
        public InfoLogContent infoLogContent;
        public override void Interact()
        {
            InfoLogEvent.Trigger(infoLogContent, InfoLogEventType.SetInfoLogContent);
            MyUIEvent.Trigger(UIType.InfoLogTablet, UIActionType.Open);
            ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, actionId);
            BillboardEvent.Trigger(null, BillboardEventType.Hide);
        }
        public override void OnInteractionStart()
        {
        }
        public override void OnInteractionEnd()
        {
        }
        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Read Info Log";
        }
        public override void SetConsoleToLacksPowerState()
        {
        }
        public override void SetConsoleToPoweredOnState()
        {
        }
        public override void SetConsoleToHailPlayerState()
        {
        }
    }
}
