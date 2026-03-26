using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes
{
    [CreateAssetMenu(fileName = "CoreItemObject", menuName = "Scriptable Objects/Items/Core Item Object")]
    public class OuterCoreItemObject : MyBaseItem
    {
        [Serializable]
        public enum CoreObjectValueGrade
        {
            StandardGrade,
            Radiant,
            Stellar,
            Unreasonable,
            MiscExotic
        }

        public enum CoreReactivity
        {
            MostReactive,
            HighlyReactive,
            Reactive,
            Resistant,
            HighlyResistant
        }


        [FormerlySerializedAs("innerObjectValueGrade")] [FormerlySerializedAs("kernelGrade")]
        public CoreObjectValueGrade coreObjectValueGrade = CoreObjectValueGrade.StandardGrade;

        // [FormerlySerializedAs("coreGrade")] public CoreReactivity coreReactivity = CoreReactivity.HighlyReactive;

        // TODO most likely just use UniqueIds
        // public List<CoreKernelItemObject> possibleKernelCoreObjects;

        public float dissolveSpeed = 2.0f;


        [Header("Sorting")] public int corePriorityLevel;
    }
}
