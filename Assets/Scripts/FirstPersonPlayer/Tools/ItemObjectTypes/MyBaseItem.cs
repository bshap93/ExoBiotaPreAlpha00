using Plugins.InventoryEngine.InventoryEngine.InventoryEngine.Scripts.Items;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(fileName = "MyBaseItem", menuName = "Scriptable Objects/Items/MyBaseItem")]
    public class MyBaseItem : BaseItem
    {
        public enum EquippableCtx
        {
            FirstPerson,
            Dirigible,
            None
        }

        public enum StateCategory
        {
            None,
            RhizomicCore
        }

        public float weight = 1.0f;
        public EquippableCtx equippableContext = EquippableCtx.None;

        [Header("Buying/Selling Settings")] public float normalBuyPrice = 1f;
        public float normalSellPrice = 0.5f;
        [FormerlySerializedAs("sellable")] public bool legalSellable = true;
        public bool illegalSellable;

        public StateCategory stateCategory = StateCategory.None;

        public bool isQuestItem;


        // Default should be at 0, so no need for explicit default
        // public int defaultStateIndex;

        public bool gatedPickup;

        public bool IsStateful()
        {
            return stateCategory != StateCategory.None;
        }
    }
}
