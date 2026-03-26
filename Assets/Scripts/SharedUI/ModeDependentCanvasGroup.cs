using System;
using Events;
using MoreMountains.Tools;
using Structs;
using UnityEngine;

namespace SharedUI
{
    public class ModeDependentCanvasGroup : MonoBehaviour, MMEventListener<ModeLoadEvent>
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private GameMode[] modesToShow;

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ModeLoadEvent eventType)
        {
            if (eventType.EventType == ModeLoadEventType.Enabled)
            {
                if (Array.Exists(modesToShow, mode => mode == eventType.ModeName))
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.interactable = true;
                    canvasGroup.blocksRaycasts = true;
                }
                else
                {
                    canvasGroup.alpha = 0f;
                    canvasGroup.interactable = false;
                    canvasGroup.blocksRaycasts = false;
                }
            }
        }
    }
}