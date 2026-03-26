using Events;
using HighlightPlus;
using ModeControllers;
using MoreMountains.Tools;
using Structs;
using UnityEngine;

namespace Dirigible.SceneEffects
{
    [RequireComponent(typeof(HighlightEffect))]
    public class DirigibleHighlightController : MonoBehaviour, MMEventListener<ModeLoadEvent>
    {
        [SerializeField] private HighlightEffect highlightEffect;

        private DirigibleModeController dirigibleModeController;

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
            if (eventType.EventType == ModeLoadEventType.Load)
                if (eventType.ModeName == GameMode.DirigibleFlight)
                {
                    var pawn = GameObject.FindWithTag("Player");
                    if (pawn != null) dirigibleModeController = pawn.GetComponent<DirigibleModeController>();
                }
        }
    }
}