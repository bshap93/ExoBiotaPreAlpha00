using Helpers.Events.UI;
using Manager;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.HUD
{
    public class LiquidToolUI : MonoBehaviour, MMEventListener<LiquidToolStateEvent>
    {
        // [SerializeField] TMP_Text gunModeText;
        [SerializeField] CanvasGroup canvasGroup;
        [SerializeField] Image[] fillNotches;
        [SerializeField] Color emptyNotchColor;
        [SerializeField] Color fullNotchColor;
        [SerializeField] ToolsStateManager toolsStateManager;

        void Awake()
        {
            if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
            Hide();
        }

        void Start()
        {
            var ichorCharges = toolsStateManager.CurrentIchorCharges;
            var maxIchorCharges = toolsStateManager.MaxIchorCharges;
            UpdateIchorChargeDisplay(ichorCharges, maxIchorCharges);
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(LiquidToolStateEvent eventType)
        {
            if (eventType.Type == LiquidToolStateEventType.UpdatedIchorCharges)
                UpdateIchorChargeDisplay(eventType.CurrentIchorCharges, eventType.MaxIchorCharges);
            else if (eventType.Type == LiquidToolStateEventType.EquippedLiquidTool)
                Show();
            else if (eventType.Type == LiquidToolStateEventType.UnequippedLiquidTool) Hide();
        }

        void UpdateIchorChargeDisplay(int ichorCharges, int maxIchorCharges = 4)
        {
            foreach (var fill in fillNotches) fill.color = emptyNotchColor;

            for (var i = 0; i < ichorCharges && i < fillNotches.Length; i++) fillNotches[i].color = fullNotchColor;
        }

        public void Hide()
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("Canvas group is null");
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        public void Show()
        {
            if (canvasGroup == null)
            {
                Debug.LogWarning("Canvas group is null");
                return;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}
