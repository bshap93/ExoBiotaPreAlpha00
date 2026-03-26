using FirstPersonPlayer.UI.Samples;
using Helpers.Events;
using Inventory;
using Manager.Global;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using NewScript.UI;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;

namespace SharedUI.IGUI
{
    public class IGUICanvasGroupListener : MonoBehaviour, MMEventListener<MyUIEvent>, MMEventListener<MMInventoryEvent>
    {
        public string playerMainInvName = "PlayerMainInventory";
        public string dirigibleInvName = "DirigibleInventory";

        [SerializeField] ObjectivesIGUIController objectivesIGUIController;

        [SerializeField] AttrIGUIList attrIGUIList;
        BioSamplesIGUILIstView _bioSamplesIGUILIstView;
        Canvas _canvas;
        CanvasGroup _canvasGroup;
        InventoryIGUIController _inventoryIGUIController;

        bool _isOpen;

        bool _needsRefresh;
        ObjectivesIGUIController _objectivesIGUIController;
        SlotsIGUIController _slotsIGUIController;


        void Awake()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            _canvas = GetComponentInParent<Canvas>(); // <- add

            if (_canvasGroup == null) Debug.LogError("IGUICanvasGroupListener requires a CanvasGroup component.");
            _slotsIGUIController = GetComponentInChildren<SlotsIGUIController>();
            if (_slotsIGUIController == null) Debug.LogWarning("Slots controlle not found.");

            _inventoryIGUIController = GetComponentInChildren<InventoryIGUIController>();
            if (_inventoryIGUIController == null) Debug.LogWarning("Inventory controller not found.");

            _objectivesIGUIController = objectivesIGUIController;
            if (_objectivesIGUIController == null) Debug.LogWarning("Objectives controller not found.");

            _bioSamplesIGUILIstView = GetComponentInChildren<BioSamplesIGUILIstView>();
            if (_bioSamplesIGUILIstView == null) Debug.LogWarning("BioSamplesIGUILIstView not found.");

            OnClose();
        }

        void LateUpdate()
        {
            if (_isOpen && _needsRefresh)
            {
                RefreshAll();
                _needsRefresh = false;
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<MMInventoryEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<MMInventoryEvent>();
        }
        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (!_isOpen) return;
            switch (eventType.InventoryEventType)
            {
                case MMInventoryEventType.ContentChanged:
                case MMInventoryEventType.EquipRequest:
                case MMInventoryEventType.ItemUsed:
                case MMInventoryEventType.Drop:
                case MMInventoryEventType.Pick:
                case MMInventoryEventType.UnEquipRequest:
                case MMInventoryEventType.UseRequest:
                case MMInventoryEventType.Move:
                    MarkDirty();
                    break;
            }
        }


        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiType == UIType.InGameUI)
            {
                if (eventType.uiActionType == UIActionType.Open)
                {
                    MoreMountains.InventoryEngine.Inventory centerInventory = null;
                    var slotsSetIndex = 0;
                    if (GameStateManager.Instance.CurrentMode == GameMode.FirstPerson)
                    {
                        slotsSetIndex = SlotsIGUIController.FirstPersonSlotsTypeIndex;
                        centerInventory =
                            MoreMountains.InventoryEngine.Inventory.FindInventory(
                                playerMainInvName,
                                "Player1");
                    }
                    else if (GameStateManager.Instance.CurrentMode == GameMode.DirigibleFlight)
                    {
                        slotsSetIndex = SlotsIGUIController.DirigibleSlotsTypeIndex;
                        centerInventory =
                            MoreMountains.InventoryEngine.Inventory.FindInventory(
                                dirigibleInvName,
                                "Player1");
                    }

                    if (_canvasGroup != null) OnOpen(centerInventory, slotsSetIndex);
                }
                else if (eventType.uiActionType == UIActionType.Close)
                {
                    if (_canvasGroup != null) OnClose();
                }
                else if (eventType.uiActionType == UIActionType.Update)
                {
                    if (_slotsIGUIController != null) _slotsIGUIController.Refresh();
                    if (_inventoryIGUIController != null)
                    {
                        if (GameStateManager.Instance.CurrentMode == GameMode.FirstPerson)
                        {
                            var fpInventory =
                                MoreMountains.InventoryEngine.Inventory.FindInventory(playerMainInvName, "Player1");

                            _inventoryIGUIController.Refresh(
                                fpInventory, GlobalInventoryManager.InventoryWithWeightLimit.PlayerMainInventory);
                        }
                        else if (GameStateManager.Instance.CurrentMode == GameMode.DirigibleFlight)
                        {
                            var dirigibleInventory =
                                MoreMountains.InventoryEngine.Inventory.FindInventory(dirigibleInvName, "Player1");

                            _inventoryIGUIController.Refresh(
                                dirigibleInventory, GlobalInventoryManager.InventoryWithWeightLimit.DirigibleInventory);
                        }
                    }

                    if (_objectivesIGUIController != null) _objectivesIGUIController.Refresh();
                    if (_bioSamplesIGUILIstView != null) _bioSamplesIGUILIstView.Refresh();
                }
            }
        }

        public void MarkDirty()
        {
            _needsRefresh = true;
        }

        void OnOpen(MoreMountains.InventoryEngine.Inventory inventory, int slotsSetIndex)
        {
            if (_canvas != null) _canvas.enabled = true; // <- add

            _canvasGroup.alpha = 1f;
            _canvasGroup.interactable = true;
            _canvasGroup.blocksRaycasts = true;
            _isOpen = true;

            // RefreshAll();
            MarkDirty();
        }
        // Odin button
        [Button("Refresh All Weights")]
        public void RefreshAll()
        {
            MoreMountains.InventoryEngine.Inventory centerInventory = null;
            if (_inventoryIGUIController == null)
            {
                Debug.LogError("InventoryIGUIController not found!");
                return;
            }

            if (GameStateManager.Instance.CurrentMode == GameMode.FirstPerson)
            {
                centerInventory =
                    MoreMountains.InventoryEngine.Inventory.FindInventory(
                        playerMainInvName,
                        "Player1");

                _inventoryIGUIController.SetInventoryTypeDropdown(InventoryIGUIController.FPPlayerInventoryTypeIndex);
                _slotsIGUIController.ShowPlayerSlots();
            }
            // else if (GameStateManager.Instance.CurrentMode == GameMode.Overview)
            // {
            //     centerInventory =
            //         MoreMountains.InventoryEngine.Inventory.FindInventory(
            //             dirigibleInvName,
            //             "Player1");
            //
            //     _inventoryIGUIController.SetInventoryTypeDropdown(InventoryIGUIController.DirigibleInventoryTypeIndex);
            //     _slotsIGUIController.ShowDirigibleSlots();
            // }
            // else if (GameStateManager.Instance.CurrentMode == GameMode.DirigibleFlight)
            // {
            //     centerInventory =
            //         MoreMountains.InventoryEngine.Inventory.FindInventory(
            //             dirigibleInvName,
            //             "Player1");
            //
            //     _inventoryIGUIController.SetInventoryTypeDropdown(InventoryIGUIController.DirigibleInventoryTypeIndex);
            //     _slotsIGUIController.ShowDirigibleSlots();
            // }

            _slotsIGUIController?.Refresh();
            _inventoryIGUIController?.Refresh(
                centerInventory,
                GameStateManager.Instance.CurrentMode == GameMode.FirstPerson
                    ? GlobalInventoryManager.InventoryWithWeightLimit.PlayerMainInventory
                    : GlobalInventoryManager.InventoryWithWeightLimit.DirigibleInventory);

            _objectivesIGUIController?.Refresh();
            _bioSamplesIGUILIstView?.Refresh();

            attrIGUIList.Initialize();
        }

        void OnClose()
        {
            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
            _isOpen = false;


            if (_canvas != null) _canvas.enabled = false; // <- add


            // Cursor: depends on current mode
            var inOverview = GameStateManager.Instance &&
                             GameStateManager.Instance.CurrentMode == GameMode.Overview;


            Time.timeScale = 1f;
        }
    }
}
