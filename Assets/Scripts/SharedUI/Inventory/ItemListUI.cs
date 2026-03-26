// ItemListUI.cs

using System.Collections;
using System.Collections.Generic;
using Helpers.Events;
using Inventory.UI;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI
{
    /// <summary>
    ///     Generic inventory list controller – works with any prefab that
    ///     contains an <see cref="ItemElementBase" />.
    /// </summary>
    public class ItemListUI : MonoBehaviour, MMEventListener<MMInventoryEvent>, MMEventListener<ExaminationEvent>
    {
        [Header("References")] [SerializeField]
        private Transform listRoot;

        [SerializeField] private GameObject rowPrefab; // Hud OR List

        [Header("Data‑source")] [SerializeField]
        private MoreMountains.InventoryEngine.Inventory inventory;

        private readonly List<GameObject> _rows = new();

        private Coroutine _lateInit;


        /* ---------- Unity life‑cycle ---------- */

        private void Start()
        {
            Rebuild();
        }

        private void OnEnable()
        {
            this.MMEventStartListening<MMInventoryEvent>();
            this.MMEventStartListening<ExaminationEvent>();

            // Defer a one-time rebuild until the inventory has real content
            if (_lateInit != null) StopCoroutine(_lateInit);
            _lateInit = StartCoroutine(RebuildWhenInventoryReady());
        }

        private void OnDisable()
        {
            this.MMEventStopListening<MMInventoryEvent>();
            this.MMEventStopListening<ExaminationEvent>();
        }

        public void OnMMEvent(ExaminationEvent eventType)
        {
            if (eventType.SceneObjectType == ExaminableItemType.Pickable)
            {
                Debug.Log("ExaminationEvent received, rebuilding ItemListUI");
                Rebuild();
            }
        }

        /* ---------- Event listener ---------- */

        public void OnMMEvent(MMInventoryEvent e)
        {
            if (e.TargetInventoryName != inventory?.name || e.PlayerID != inventory?.PlayerID) return;

            switch (e.InventoryEventType)
            {
                case MMInventoryEventType.ContentChanged:
                case MMInventoryEventType.ItemEquipped:
                case MMInventoryEventType.ItemUnEquipped:
                    Rebuild();
                    break;
            }
            // if (e.InventoryEventType != MMInventoryEventType.ContentChanged) return;
            // if (e.TargetInventoryName != inventory?.name || e.PlayerID != inventory?.PlayerID) return;
            // Rebuild();
        }

        private IEnumerator RebuildWhenInventoryReady()
        {
            // Short, bounded wait to avoid racing the loader/equip on respawn
            var timeoutAt = Time.realtimeSinceStartup + 2f;
            while (inventory == null || inventory.Content == null || !HasRealItem(inventory))
            {
                if (Time.realtimeSinceStartup > timeoutAt) break;
                yield return null;
            }

            Rebuild();
            _lateInit = null;
        }

        private static bool HasRealItem(MoreMountains.InventoryEngine.Inventory inv)
        {
            if (inv == null || inv.Content == null) return false;
            foreach (var it in inv.Content)
                if (!InventoryItem.IsNull(it) && it.Quantity > 0)
                    return true;
            return false;
        }

        /* ---------- Public API ---------- */

        public void SetInventory(MoreMountains.InventoryEngine.Inventory inv)
        {
            inventory = inv;
            Rebuild();
        }

        /* ---------- Internals ---------- */

        private void Rebuild()
        {
            // 1. Destroy previous rows
            foreach (var go in _rows) Destroy(go);
            _rows.Clear();

            if (inventory == null || inventory.Content == null) return;

            // 2. Re‑instantiate for every filled slot
            foreach (var slot in inventory.Content)
            {
                if (InventoryItem.IsNull(slot) || slot.Quantity <= 0) continue;

                var row = Instantiate(rowPrefab, listRoot);
                _rows.Add(row);

                var presenter = row.GetComponent<ItemElementBase>();
                if (presenter == null)
                    Debug.LogError($"Row prefab {rowPrefab.name} lacks an ItemElementBase component");
                else
                    presenter.Bind(slot);
            }
        }
    }
}