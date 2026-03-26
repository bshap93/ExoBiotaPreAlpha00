using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "LiquidContainer", menuName = "Scriptable Objects/Items/LiquidContainer", order = 2)]
    public class LiquidContainerSObject : MyBaseItem
    {
        [FormerlySerializedAs("ContainedLiquidType")]
        public LiquidType containedLiquidType;

        public bool disposable; // if true, item is destroyed when empty

        public float capacityLiters = 1f; // max capacity in liters
    }
}
