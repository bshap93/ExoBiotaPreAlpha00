using Helpers.Events;
using MoreMountains.Tools;
using Rewired;
using UnityEngine;
using Yarn.Unity;

namespace Utilities.Inputs
{
    public class RewiredLineAdvancerOutputs : MonoBehaviour, MMEventListener<PlayerDeathEvent>
    {
        public enum LineAdvancerInputActions
        {
            HurryUpLine,
            NextLine,
            CancelDialogue
        }

        [SerializeField] LineAdvancer lineAdvancer;

        public bool hurryUpLine;
        public bool nextLine;
        public bool cancelDialogue;
        bool _isPlayerDead;

        Player _rewiredPlayer;
        void Start()
        {
            _rewiredPlayer = ReInput.players.GetPlayer(0);
        }

        // Update is called once per frame
        void Update()
        {
            if (_rewiredPlayer == null || _isPlayerDead) return;

            hurryUpLine = _rewiredPlayer.GetButton("UseEquipped");
            nextLine = _rewiredPlayer.GetButton("Interact");
            cancelDialogue = _rewiredPlayer.GetButton("Pause");

            if (hurryUpLine)
                lineAdvancer.RequestLineHurryUp();

            if (nextLine)
                lineAdvancer.RequestNextLine();

            if (cancelDialogue)
                lineAdvancer.RequestDialogueCancellation();
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(PlayerDeathEvent eventType)
        {
            _isPlayerDead = true;
        }

        public bool GetButtonInput(LineAdvancerInputActions input)
        {
            switch (input)
            {
                case LineAdvancerInputActions.CancelDialogue:
                    return cancelDialogue;
                case LineAdvancerInputActions.HurryUpLine:
                    return hurryUpLine;
                case LineAdvancerInputActions.NextLine:
                    return nextLine;
                default:
                    return false;
            }
        }
    }
}
