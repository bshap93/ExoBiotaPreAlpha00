using Helpers.Events;
using MoreMountains.InventoryEngine;
using Rewired;
using UnityEngine;

namespace Inventory
{
    /// <summary>
    ///     Rewired‑powered version of InventoryInputManager.
    ///     Uses the “InGameUI” map category while the inventory is open,
    ///     and whatever gameplay category you specify when it’s closed.
    /// </summary>
    public class RewiredInventoryInputManager : InventoryInputManager
    {
        [Header("Rewired — general")] public int rewiredPlayerId; // usually 0 for single‑player

        [Tooltip("Gameplay map category to restore when the inventory closes")]
        public string gameplayCategory = "FirstPerson";

        [Header("Rewired — action names")] public string toggleInventoryAction = "ToggleInventory";

        public string openInventoryAction = "OpenInventory";
        public string closeInventoryAction = "CloseInventory";
        public string cancelAction = "Cancel";
        public string prevInvAction = "PrevInventory";
        public string nextInvAction = "NextInventory";
        public string moveAction = "MoveItem";
        public string equipOrUseAction = "EquipUse";
        public string equipAction = "Equip";
        public string useAction = "Use";
        public string dropAction = "Drop";

        // internal ------------------------------------------------------------
        protected Player _player;

        protected override void Start()
        {
            base.Start();
            _player = ReInput.players.GetPlayer(rewiredPlayerId);
        }

        #region inventory‑level input -------------------------------------------------

        protected override void HandleInventoryInput()
        {
            if (_currentInventoryDisplay == null) return;

            // poll Rewired ----------------------------------------------------
            _toggleInventoryKeyPressed = _player.GetButtonDown(toggleInventoryAction);
            _openInventoryKeyPressed = _player.GetButtonDown(openInventoryAction);
            _closeInventoryKeyPressed = _player.GetButtonDown(closeInventoryAction);
            _cancelKeyPressed = _player.GetButtonDown(cancelAction);
            _prevInvKeyPressed = _player.GetButtonDown(prevInvAction);
            _nextInvKeyPressed = _player.GetButtonDown(nextInvAction);
            _moveKeyPressed = _player.GetButtonDown(moveAction);
            _equipOrUseKeyPressed = _player.GetButtonDown(equipOrUseAction);
            _equipKeyPressed = _player.GetButtonDown(equipAction);
            _useKeyPressed = _player.GetButtonDown(useAction);
            _dropKeyPressed = _player.GetButtonDown(dropAction);

            // --- same behaviour as the original InventoryInputManager --------
            if (_toggleInventoryKeyPressed) ToggleInventory();
            if (_openInventoryKeyPressed) OpenInventory();
            if (_closeInventoryKeyPressed) CloseInventory();

            if (_cancelKeyPressed && InventoryIsOpen) CloseInventory();

            if (InputOnlyWhenOpen && !InventoryIsOpen) return;

            if (_prevInvKeyPressed) TryGoToOtherInventory(-1);
            if (_nextInvKeyPressed) TryGoToOtherInventory(+1);

            if (_moveKeyPressed) TryMove();
            if (_equipOrUseKeyPressed) EquipOrUse();
            if (_equipKeyPressed) Equip();
            if (_useKeyPressed) Use();
            if (_dropKeyPressed) Drop();
        }

        #endregion

        #region hotbars ---------------------------------------------------------------

        // Re‑implement so that hotbars use Rewired too
        protected override void HandleHotbarsInput()
        {
            if (InventoryIsOpen) return;

            foreach (var hotbar in _targetInventoryHotbars)
            {
                if (hotbar == null) continue;

                // Put the Rewired action name you want to trigger the hot‑bar
                // into the “HotbarKey” field in the Inspector.
                if (_player.GetButtonDown(hotbar.HotbarKey)) hotbar.Action();
            }
        }

        #endregion

        #region helpers ---------------------------------------------------------------

        protected void TryGoToOtherInventory(int direction)
        {
            var next = _currentInventoryDisplay.GoToInventory(direction);
            if (next != null) _currentInventoryDisplay = next;
        }

        protected void TryMove()
        {
            if (CurrentlySelectedInventorySlot?.CurrentItem == null) return;
            if (CurrentlySelectedInventorySlot.CurrentItem.DisplayProperties.AllowMoveShortcut)
                CurrentlySelectedInventorySlot.Move();
        }

        #endregion

        #region open / close — enable the right Rewired maps --------------------------

        public override void OpenInventory()
        {
            base.OpenInventory();
            MyUIEvent.Trigger(UIType.InGameUI, UIActionType.Open
            ); // tell GSM to unlock + swap maps
        }

        public override void CloseInventory()
        {
            base.CloseInventory();
            MyUIEvent.Trigger(UIType.InGameUI, UIActionType.Close
            ); // tell GSM to lock + swap maps);
        }

        #endregion
    }
}