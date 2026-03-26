using Helpers.Events;
using Inventory;
using Manager.Global;
using Michsky.MUIP;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using Sirenix.OdinInspector;
using Structs;
using UnityEngine;

public class WeightHUDElem : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<LoadedManagerEvent>
{
    [SerializeField] ProgressBar weightProgressBar;
    bool _needsRefresh;

    void LateUpdate()
    {
        if (_needsRefresh)
        {
            if (GameStateManager.Instance == null)
            {
                Debug.LogError("Not workn");
                return;
            }

            if (GameStateManager.Instance.CurrentMode == GameMode.FirstPerson)
                Refresh("PlayerMainInventory");
            else
                Refresh("DirigibleInventory");

            _needsRefresh = false;
        }
    }

    void OnEnable()
    {
        this.MMEventStartListening<MMInventoryEvent>();
        this.MMEventStartListening<LoadedManagerEvent>();
    }

    void OnDisable()
    {
        this.MMEventStopListening<MMInventoryEvent>();
        this.MMEventStopListening<LoadedManagerEvent>();
    }

    public void OnMMEvent(LoadedManagerEvent eventType)
    {
        if (eventType.ManagerType == ManagerType.All) Initialize();
    }

    public void OnMMEvent(MMInventoryEvent eventType)
    {
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

    public void MarkDirty()
    {
        _needsRefresh = true;
    }

    void Initialize()
    {
        MarkDirty();
    }

    [Button("Refresh Weights")]
    void Refresh(string targetInventoryName)
    {
        if (GlobalInventoryManager.Instance == null) return;

        if (GlobalInventoryManager.Instance.playerInventory.name == targetInventoryName)
        {
            var weight =
                GlobalInventoryManager.Instance.GetWeightOfPlayerMainPlusEquipped(
                );

            var maxWeight = GlobalInventoryManager.Instance.GetPlayerMaxWeight();

            weightProgressBar.maxValue = maxWeight;
            weightProgressBar.currentPercent = maxWeight > 0f ? weight : 0f;
            weightProgressBar.UpdateUI();
        }
        else if (GlobalInventoryManager.Instance.dirigibleInventory.name == targetInventoryName)
        {
            var weight =
                GlobalInventoryManager.Instance.GetTotalWeightInDirigible(
                );

            var maxWeight = GlobalInventoryManager.Instance.GetPlayerMaxWeight();

            weightProgressBar.maxValue = maxWeight;
            weightProgressBar.currentPercent = maxWeight > 0f ? weight : 0f;
            weightProgressBar.UpdateUI();
        }
    }
}
