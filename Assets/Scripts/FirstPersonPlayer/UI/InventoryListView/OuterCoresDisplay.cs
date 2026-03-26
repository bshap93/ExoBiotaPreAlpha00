using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.Progression;
using Helpers.Events.UpdateUI;
using Inventory;
using Manager.ProgressionMangers;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI.Inventory;
using TMPro;
using UnityEngine;

namespace FirstPersonPlayer.UI.InventoryListView
{
    public class OuterCoresDisplay : MonoBehaviour, MMEventListener<MMInventoryEvent>,
        MMEventListener<LoadedManagerEvent>, MMEventListener<ProgressionUpdateListenerNotifier>,
        MMEventListener<UpdateInventoryWindowEvent>
    {
        [SerializeField] GradeCoresUILVRow standardCoreRow;
        [SerializeField] GradeCoresUILVRow radiantCoreRow;
        [SerializeField] GradeCoresUILVRow stellarCoreRow;
        [SerializeField] GradeCoresUILVRow unreasonableCoreRow;

        [SerializeField] bool condensedView;

        // [SerializeField] GatedLevelingUIController gatedLevelingUIController;

        [Header("Progression")] [SerializeField]
        LevelingManager levelingManager;
        [SerializeField] TMP_Text xpAmtText;
        [SerializeField] TMP_Text levelIntText;

        void Start()
        {
            Initialize();
        }

        void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
            this.MMEventStartListening<LoadedManagerEvent>();

            this.MMEventStartListening<ProgressionUpdateListenerNotifier>();
            this.MMEventStartListening<UpdateInventoryWindowEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MMInventoryEvent>();
            this.MMEventStopListening<LoadedManagerEvent>();

            this.MMEventStopListening<ProgressionUpdateListenerNotifier>();
            this.MMEventStopListening<UpdateInventoryWindowEvent>();
        }
        public void OnMMEvent(LoadedManagerEvent eventType)
        {
            if (eventType.ManagerType == ManagerType.All) Initialize();
        }

        public void OnMMEvent(MMInventoryEvent eventType)
        {
            if (eventType.TargetInventoryName != GlobalInventoryManager.OuterCoresInventoryName) return;
            if (eventType.InventoryEventType == MMInventoryEventType.ContentChanged)
                RefreshCoreCounts();
        }
        public void OnMMEvent(ProgressionUpdateListenerNotifier eventType)
        {
            xpAmtText.text = eventType.CurrentTotalXP + "/" + levelingManager.TotalXpNeededForNextLevel;
            levelIntText.text = eventType.CurrentLevel.ToString();
        }
        public void OnMMEvent(UpdateInventoryWindowEvent eventType)
        {
            RefreshCoreCounts();

            // Refresh any other 
        }


        void Initialize()
        {
            RefreshCoreCounts();
            xpAmtText.text = levelingManager.CurrentTotalXP + "/" + levelingManager.TotalXpNeededForNextLevel;
            levelIntText.text = levelingManager.CurrentLevel.ToString();
        }

        public void RefreshCoreCounts()
        {
            var globalInventoryManager = GlobalInventoryManager.Instance;
            var numStandard = globalInventoryManager.GetNumberOfOuterCoresInInventory(
                OuterCoreItemObject.CoreObjectValueGrade.StandardGrade);

            var numRadiant = globalInventoryManager.GetNumberOfOuterCoresInInventory(
                OuterCoreItemObject.CoreObjectValueGrade.Radiant);

            var numStellar = globalInventoryManager.GetNumberOfOuterCoresInInventory(
                OuterCoreItemObject.CoreObjectValueGrade.Stellar);

            var numUnreasonable = globalInventoryManager.GetNumberOfOuterCoresInInventory(
                OuterCoreItemObject.CoreObjectValueGrade.Unreasonable);

            // var numExotic = globalInventoryManager.GetNumberOfInnerCoresInInventory(
            //     HarvestableInnerObject.InnerObjectValueGrade.MiscExotic);

            standardCoreRow.Initialize(OuterCoreItemObject.CoreObjectValueGrade.StandardGrade, numStandard);
            radiantCoreRow.Initialize(OuterCoreItemObject.CoreObjectValueGrade.Radiant, numRadiant);
            stellarCoreRow.Initialize(OuterCoreItemObject.CoreObjectValueGrade.Stellar, numStellar);
            unreasonableCoreRow.Initialize(OuterCoreItemObject.CoreObjectValueGrade.Unreasonable, numUnreasonable);

            if (condensedView)
            {
                if (numStandard == 0) standardCoreRow.gameObject.SetActive(false);
                else standardCoreRow.gameObject.SetActive(true);

                if (numRadiant == 0) radiantCoreRow.gameObject.SetActive(false);
                else radiantCoreRow.gameObject.SetActive(true);

                if (numStellar == 0) stellarCoreRow.gameObject.SetActive(false);
                else stellarCoreRow.gameObject.SetActive(true);

                if (numUnreasonable == 0) unreasonableCoreRow.gameObject.SetActive(false);
                else unreasonableCoreRow.gameObject.SetActive(true);
            }
            else
            {
                if (standardCoreRow.convertToXPButton != null)
                {
                    if (numStandard == 0) standardCoreRow.convertToXPButton.gameObject.SetActive(false);
                    else standardCoreRow.convertToXPButton.gameObject.SetActive(true);
                }

                if (radiantCoreRow.convertToXPButton != null)
                {
                    if (numRadiant == 0) radiantCoreRow.convertToXPButton.gameObject.SetActive(false);
                    else radiantCoreRow.convertToXPButton.gameObject.SetActive(true);
                }

                if (stellarCoreRow.convertToXPButton != null)
                {
                    if (numStellar == 0) stellarCoreRow.convertToXPButton.gameObject.SetActive(false);
                    else stellarCoreRow.convertToXPButton.gameObject.SetActive(true);
                }

                if (unreasonableCoreRow.convertToXPButton != null)
                {
                    if (numUnreasonable == 0) unreasonableCoreRow.convertToXPButton.gameObject.SetActive(false);
                    else unreasonableCoreRow.convertToXPButton.gameObject.SetActive(true);
                }
            }
        }
    }
}
