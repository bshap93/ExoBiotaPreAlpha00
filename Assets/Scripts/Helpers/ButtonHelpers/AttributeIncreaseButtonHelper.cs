using System;
using FirstPersonPlayer.Interactable.HoloInteractable;
using Helpers.Events.Progression;
using Manager.ProgressionMangers;
using Michsky.MUIP;
using MoreMountains.Tools;
using SharedUI.InputsD;
using UnityEngine;

namespace Helpers.ButtonHelpers
{
    public class AttributeIncreaseButtonHelper : DialogueInteractable,
        MMEventListener<ProgressionUpdateListenerNotifier>
    {
        [SerializeField] ButtonManager buttonManager;
        [SerializeField] LevelingManager levelingManager;
        [SerializeField] CanvasGroup buttonCanvasGroup;


        void OnEnable()
        {
            this.MMEventStartListening();

            buttonManager.isInteractable = levelingManager.UnspentAttributePoints > 0;

            buttonCanvasGroup.alpha = buttonManager.isInteractable ? 1 : 0.25f;
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ProgressionUpdateListenerNotifier eventType)
        {
            buttonManager.isInteractable = eventType.CurrentAttributePointsUnused > 0;

            buttonCanvasGroup.alpha = buttonManager.isInteractable ? 1 : 0.25f;
        }

        public void TriggerIGUIClose()
        {
            DefaultInput.ToggleIGUI();
        }
        public override string GetName()
        {
            return "Attribute Increase Button";
        }
        public override Sprite GetIcon()
        {
            throw new NotImplementedException();
        }
    }
}
