using Helpers.Events.Combat;
using Helpers.Events.Progression;
using Inventory;
using Manager.ProgressionMangers;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;

namespace SharedUI.Progression
{
    public class XPDebugChip : MonoBehaviour, MMEventListener<MMInventoryEvent>,
        MMEventListener<ProgressionUpdateListenerNotifier>, MMEventListener<StaminaRestoreRateEvent>
    {
        [Header("Main Canvas Group")] [SerializeField]
        CanvasGroup debugCanvasGroup;
        [SerializeField] bool debugMode = true;
        [Header("Debug")] [SerializeField] CanvasGroup debugChipsCanvasGroup;
        [SerializeField] TMP_Text totalXPText;
        [SerializeField] TMP_Text currentLevelText;
        [SerializeField] TMP_Text staminaRestoreRateText;
        [SerializeField] TMP_Text unusedAttributePointsText;
        [SerializeField] TMP_Text coresNumberText;
        [Header("References")] [SerializeField]
        LevelingManager levelingManager;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            debugChipsCanvasGroup.alpha = debugMode ? 1 : 0;

            if (levelingManager != null)
            {
                totalXPText.text = levelingManager.CurrentTotalXP.ToString();
                currentLevelText.text = levelingManager.CurrentLevel.ToString();
                // unusedUpgradesText.text = levelingManager.UnspentStatUpgrades.ToString();
                unusedAttributePointsText.text = levelingManager.UnspentAttributePoints.ToString();
                coresNumberText.text = levelingManager.CurentNumberOfCores().ToString();
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
            this.MMEventStartListening<ProgressionUpdateListenerNotifier>();
            this.MMEventStartListening<StaminaRestoreRateEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MMInventoryEvent>();
            this.MMEventStopListening<ProgressionUpdateListenerNotifier>();
            this.MMEventStopListening<StaminaRestoreRateEvent>();
        }


        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.TargetInventoryName != GlobalInventoryManager.OuterCoresInventoryName) return;
            if (eventType.InventoryEventType == MMInventoryEventType.ContentChanged)
                coresNumberText.text = levelingManager.CurentNumberOfCores().ToString();
        }
        public void OnMMEvent(ProgressionUpdateListenerNotifier eventType)
        {
            // for debug
            totalXPText.text = eventType.CurrentTotalXP.ToString();
            currentLevelText.text = eventType.CurrentLevel.ToString();
            // unusedUpgradesText.text = eventType.CurrentUpgradesUnused.ToString();
            unusedAttributePointsText.text = eventType.CurrentAttributePointsUnused.ToString();
        }
        public void OnMMEvent(StaminaRestoreRateEvent eventType)
        {
            // For debug: show current stamina restore rate in the debug panel
            if (debugMode) staminaRestoreRateText.text = $"{eventType.CurrentStaminaRestoreRate}";
        }
    }
}
