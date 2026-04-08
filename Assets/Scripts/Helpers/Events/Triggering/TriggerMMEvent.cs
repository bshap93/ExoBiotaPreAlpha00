// TriggerMmEvent.cs
// Put in any folder in your project. Requires Odin Inspector.

using Domains.Gameplay.Managers.Messages;
using Events;
using OWPData.Structs;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;

namespace Helpers.Events.Triggering
{
    // CommsMessageEvent, CommsMessageEventType, MessagePanelEvent, MessagePanelEventType

    public enum MmEventKind
    {
        Camera,
        Docking,
        ModeLoad,
        Alert,
        Inventory,
        CommsMessage,
        UI,
        MessagePanel,
        OverviewLocation
    }

    [DisallowMultipleComponent]
    public class TriggerMmEvent : MonoBehaviour
    {
        [Title("General")] [EnumToggleButtons] [LabelWidth(120)]
        public MmEventKind Kind = MmEventKind.Camera;

        [ToggleLeft] public bool FireOnEnable; // optional convenience

        // --------------------------
        // CameraEvent
        // --------------------------
        [FoldoutGroup("Payload/Camera")] [ShowIf(nameof(IsCamera))] [EnumToggleButtons] [LabelText("Type")]
        public CameraEventType cameraType = CameraEventType.CameraShake;

        [FoldoutGroup("Payload/Camera")] [ShowIf(nameof(IsCamera))] [Min(0f)]
        public float cameraDuration = 0.25f;

        [FoldoutGroup("Payload/Camera")] [ShowIf(nameof(IsCamera))] [Min(0f)]
        public float cameraMagnitude = 0.75f;

        // --------------------------
        // DockingEvent
        // // --------------------------
        // [FoldoutGroup("Payload/Docking")] [ShowIf(nameof(IsDocking))] [EnumToggleButtons] [LabelText("Type")]
        // public DockingEventType dockingType = DockingEventType.DockAtLocation;
        //
        // [FoldoutGroup("Payload/Docking")] [ShowIf(nameof(IsDocking))] [LabelText("Dock (optional)")]
        // public DockDefinition dockingDock;

        // --------------------------
        // ModeLoadEvent
        // --------------------------
        [FoldoutGroup("Payload/Mode")] [ShowIf(nameof(IsMode))] [EnumToggleButtons] [LabelText("Event")]
        public ModeLoadEventType modeEventType = ModeLoadEventType.Enabled;

        [FoldoutGroup("Payload/Mode")] [ShowIf(nameof(IsMode))] [LabelText("Mode")]
        public GameMode modeName = GameMode.Overview;

        // --------------------------
        // AlertEvent
        // --------------------------
        [FoldoutGroup("Payload/Alert")] [ShowIf(nameof(IsAlert))]
        public AlertReason alertReason;

        [FoldoutGroup("Payload/Alert")] [ShowIf(nameof(IsAlert))] [TextArea]
        public string alertMessage = "Something happened.";

        [FoldoutGroup("Payload/Alert")] [ShowIf(nameof(IsAlert))]
        public string alertTitle = "Alert";

        [FoldoutGroup("Payload/Alert")] [ShowIf(nameof(IsAlert))]
        public Sprite alertIcon;

        [FoldoutGroup("Payload/Alert")] [ShowIf(nameof(IsAlert))]
        public AudioClip alertSound;

        [FoldoutGroup("Payload/Alert")] [ShowIf(nameof(IsAlert))]
        public Color alertColor = Color.white;

        [FoldoutGroup("Payload/Alert")] [ShowIf(nameof(IsAlert))] [EnumToggleButtons]
        public AlertType alertVariant = AlertType.Basic;

        // --------------------------
        // InventoryEvent
        // --------------------------
        [FoldoutGroup("Payload/Inventory")] [ShowIf(nameof(IsInventory))] [EnumToggleButtons]
        public InventoryEventType inventoryEventType = InventoryEventType.ContentChanged;

        [FoldoutGroup("Payload/Inventory")] [ShowIf(nameof(IsInventory))] [LabelText("Inventory Id")]
        public string inventoryId = "PlayerInventory";

        // --------------------------
        // CommsMessageEvent
        // --------------------------
        [FoldoutGroup("Payload/Comms")] [ShowIf(nameof(IsComms))] [LabelText("Message Id")]
        public string commsMessageId = "Msg001";

        [FoldoutGroup("Payload/Comms")] [ShowIf(nameof(IsComms))] [EnumToggleButtons]
        public CommsMessageEventType commsType = CommsMessageEventType.SendMessage;

        // --------------------------
        // UIEvent
        // --------------------------
        [FoldoutGroup("Payload/UI")] [ShowIf(nameof(IsUI))]
        public UIType uiType = UIType.Any;

        [FoldoutGroup("Payload/UI")] [ShowIf(nameof(IsUI))]
        public UIActionType uiActionType = UIActionType.Open;

        [FoldoutGroup("Payload/UI")] [ShowIf(nameof(IsUI))] [Min(0)]
        public int uiIndex;

        /// <summary>
        ///     OverviewLocationEvent
        /// </summary>
        [FoldoutGroup("Payload/Overview")] [ShowIf(nameof(IsOverview))]
        public LocationType overviewLocationType;

        [FoldoutGroup("Payload/Overview")] [ShowIf(nameof(IsOverview))]
        public LocationActionType locationActionType;

        [FoldoutGroup("Payload/Overview")] [ShowIf(nameof(IsOverview))]
        public string overviewLocationId;

        [FoldoutGroup("Payload/Overview")] [ShowIf(nameof(IsOverview))]
        public Transform cameraTransform;

        // --------------------------
        // MessagePanelEvent
        // --------------------------
        [FoldoutGroup("Payload/MessagePanel")] [ShowIf(nameof(IsMessagePanel))] [LabelText("Panel Id")]
        public string messagePanelId = "Panel01";

        [FoldoutGroup("Payload/MessagePanel")] [ShowIf(nameof(IsMessagePanel))] [EnumToggleButtons]
        public MessagePanelEventType messagePanelType = MessagePanelEventType.MessagePanelOpened;

        bool IsOverview => Kind == MmEventKind.OverviewLocation;


        void OnEnable()
        {
            if (FireOnEnable) TriggerNow();
        }

        [Button(ButtonSizes.Medium)]
        [GUIColor(0.6f, 1f, 0.6f)]
        public void TriggerNow()
        {
            switch (Kind)
            {
                case MmEventKind.Camera:
                    CameraEvent.Trigger(cameraType, cameraDuration, cameraMagnitude);
                    break;


                case MmEventKind.ModeLoad:
                    ModeLoadEvent.Trigger(modeEventType, modeName);
                    break;

                case MmEventKind.Alert:
                    AlertEvent.Trigger(
                        alertReason, alertMessage, alertTitle, alertVariant, 3f, alertIcon, alertSound
                    );

                    break;

                case MmEventKind.Inventory:
                    InventoryEvent.Trigger(inventoryEventType, inventoryId);
                    break;

                case MmEventKind.CommsMessage:
                    CommsMessageEvent.Trigger(commsMessageId, commsType);
                    break;

                case MmEventKind.UI:
                    MyUIEvent.Trigger(uiType, uiActionType);
                    break;

                case MmEventKind.MessagePanel:
                    MessagePanelEvent.Trigger(messagePanelId, messagePanelType);
                    break;
                case MmEventKind.OverviewLocation:
                    OverviewLocationEvent.Trigger(
                        overviewLocationType, locationActionType, overviewLocationId,
                        cameraTransform);

                    break;
            }
        }

        // Hook this to a Unity UI Button
        public void TriggerFromUIButton()
        {
            TriggerNow();
        }

        // ShowIf helpers
        bool IsCamera()
        {
            return Kind == MmEventKind.Camera;
        }

        bool IsDocking()
        {
            return Kind == MmEventKind.Docking;
        }

        bool IsMode()
        {
            return Kind == MmEventKind.ModeLoad;
        }

        bool IsAlert()
        {
            return Kind == MmEventKind.Alert;
        }

        bool IsInventory()
        {
            return Kind == MmEventKind.Inventory;
        }

        bool IsComms()
        {
            return Kind == MmEventKind.CommsMessage;
        }

        bool IsUI()
        {
            return Kind == MmEventKind.UI;
        }

        bool IsMessagePanel()
        {
            return Kind == MmEventKind.MessagePanel;
        }
    }
}
