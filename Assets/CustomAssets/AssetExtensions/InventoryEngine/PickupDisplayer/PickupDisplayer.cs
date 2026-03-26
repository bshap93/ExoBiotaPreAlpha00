using System.Collections;
using System.Collections.Generic;
using Helpers.Events;
using Manager;
using Manager.SceneManagers.Pickable;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace CustomAssets.AssetExtensions.InventoryEngine.PickupDisplayer
{
    /// <summary>
    ///     A class used to display the picked up items on screen.
    ///     The PickupDisplayItems will be parented to it, so it's better if it has a LayoutGroup (Vertical or Horizontal) too.
    /// </summary>
    public class PickupDisplayer : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<BioSampleEvent>,
        MMEventListener<ItemTransactionEvent>
    {
        [Header("References")] [SerializeField]
        PickableManager pickableManager;
        [Tooltip("the prefab to use to display achievements")]
        public PickupDisplayItem PickupDisplayPrefab;

        public string trashInventoryName;

        [Tooltip("the duration the pickup display item will remain on screen")]
        public float PickupDisplayDuration = 5;

        [Tooltip("the fade in/out duration")] public float PickupFadeDuration = .2f;

        readonly Dictionary<string, PickupDisplayItem> _displays = new();

        bool _itemTransactionInProgress;
        WaitForSeconds _pickupDisplayWfs;

        void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
            this.MMEventStartListening<BioSampleEvent>();
            this.MMEventStartListening<ItemTransactionEvent>();
            OnValidate();
        }

        void OnDisable()
        {
            this.MMEventStopListening<MMInventoryEvent>();
            this.MMEventStopListening<BioSampleEvent>();
            this.MMEventStopListening<ItemTransactionEvent>();
        }

        void OnValidate()
        {
            _pickupDisplayWfs = new WaitForSeconds(PickupDisplayDuration);
        }

        public void OnMMEvent(BioSampleEvent eventType)
        {
            if (_itemTransactionInProgress) return;

            var iconOverride = ExaminationManager.Instance.iconRepository.addLiquidSampleIcon;
            if (eventType.EventType != BioSampleEventType.CompleteCollection) return;
            var item = eventType.BioOrganismType;
            var quantity = 1;
            if (_displays.TryGetValue(item.organismID, out var display))
            {
                display.AddQuantity(quantity);
            }
            else
            {
                _displays[item.organismID] = Instantiate(PickupDisplayPrefab, transform);
                var fakeInventoryItem = new InventoryItem();
                fakeInventoryItem.ItemID = item.organismID;
                fakeInventoryItem.ItemName = item.organismName;
                fakeInventoryItem.Icon = item.organismIcon;
                _displays[item.organismID].Display(fakeInventoryItem, quantity, iconOverride);
                var canvasGroup = _displays[item.organismID].GetComponent<CanvasGroup>();
                if (canvasGroup)
                {
                    canvasGroup.alpha = 0;
                    StartCoroutine(MMFade.FadeCanvasGroup(canvasGroup, PickupFadeDuration, 1));
                }

                StartCoroutine(FadeOutAndDestroy());

                IEnumerator FadeOutAndDestroy()
                {
                    yield return _pickupDisplayWfs;
                    if (canvasGroup) yield return MMFade.FadeCanvasGroup(canvasGroup, PickupFadeDuration, 0);
                    Destroy(_displays[item.organismID].gameObject);
                    _displays.Remove(item.organismID);
                }
            }
        }
        public void OnMMEvent(ItemTransactionEvent eventType)
        {
            if (eventType.EventType == ItemTransactionEventType.StartMove)
                _itemTransactionInProgress = true;
            else if (eventType.EventType == ItemTransactionEventType.FinishMove) _itemTransactionInProgress = false;
        }

        public void OnMMEvent(MMInventoryEvent inventoryEvent)
        {
            if (_itemTransactionInProgress) return;
            Sprite iconOverride = null;
            if (inventoryEvent.InventoryEventType != MMInventoryEventType.Pick &&
                inventoryEvent.InventoryEventType != MMInventoryEventType.Destroy) return;

            if (inventoryEvent.InventoryEventType == MMInventoryEventType.Pick)
                iconOverride = ExaminationManager.Instance.iconRepository.addItemIcon;
            else if (inventoryEvent.InventoryEventType == MMInventoryEventType.Destroy)
                iconOverride = ExaminationManager.Instance.iconRepository.removeItemIcon;

            var item = inventoryEvent.EventItem;
            // Do not display if the item is going to trash
            if (item.TargetInventoryName == trashInventoryName) return;

            if (pickableManager != null &&
                !pickableManager.IsItemTypePicked(item.ItemID)) return;

            var quantity = inventoryEvent.Quantity;
            if (_displays.TryGetValue(item.ItemID, out var display))
            {
                display.AddQuantity(quantity);
            }
            else
            {
                _displays[item.ItemID] = Instantiate(PickupDisplayPrefab, transform);
                _displays[item.ItemID].Display(item, quantity, iconOverride);
                var canvasGroup = _displays[item.ItemID].GetComponent<CanvasGroup>();
                if (canvasGroup)
                {
                    canvasGroup.alpha = 0;
                    StartCoroutine(MMFade.FadeCanvasGroup(canvasGroup, PickupFadeDuration, 1));
                }

                StartCoroutine(FadeOutAndDestroy());

                IEnumerator FadeOutAndDestroy()
                {
                    yield return _pickupDisplayWfs;
                    if (canvasGroup) yield return MMFade.FadeCanvasGroup(canvasGroup, PickupFadeDuration, 0);
                    Destroy(_displays[item.ItemID].gameObject);
                    _displays.Remove(item.ItemID);
                }
            }
        }
    }
}
