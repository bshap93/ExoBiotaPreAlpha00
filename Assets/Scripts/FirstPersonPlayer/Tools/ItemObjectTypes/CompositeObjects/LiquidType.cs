using System;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects
{
    [Serializable]
    [CreateAssetMenu(fileName = "LiquidType", menuName = "Scriptable Objects/CompositeTypes/LiquidType", order = 3)]
    public class LiquidType : ScriptableObject
    {
        public string UniqueID;
        public string LiquidName;
        public Sprite LiquidIcon;
        public string ShortDescription;
        public string FullDescription;
    }
}