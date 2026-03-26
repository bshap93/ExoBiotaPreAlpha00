using System;
using System.Collections.Generic;
using UnityEngine;

namespace FirstPersonPlayer.UI.Stats
{
    [CreateAssetMenu(fileName = "ProgressBarColors", menuName = "Scriptable Objects/UI/Progress Bar Colors")]
    public class ProgressBarColors : ScriptableObject
    {
        public List<ColorPair> colorPairs = new();

        public Color defaultBgColor = Color.black;
        public Color defaultFillColor = Color.white;

        [Serializable]
        public struct ColorPair
        {
            public Color bgColor;
            public Color fillColor;
        }
    }
}
