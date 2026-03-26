using System;
using SharedUI;
using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;

namespace Helpers.ScriptableObjects.Tutorial
{
    [Serializable]
    [CreateAssetMenu(fileName = "MainTutBitWindowArgs", menuName = "Scriptable Objects/UI/Main Tut Bit Window Args")]
    public class MainTutBitWindowArgs : ScriptableObject
    {
        [SerializeField] [OnValueChanged("OnTutFieldsIdChanged")]
        public string mainTutID;
        public string tutBitName;
        public string subheader;
        public string nameInterglot;
        public string img1Caption;
        public Sprite img1Image;
        public string img2Caption;
        public Sprite img2Image;
        public string img3Caption;
        public Sprite img3Image;
        [TextArea] public string paragraph1;
        [TextArea] public string paragraph2;
        public MainTutorialWindow.TutBitStyle style;
        string _lastKnownName;

#if UNITY_EDITOR
        void OnValidate()
        {
            // Detect if the asset was renamed externally (header bar, AssetDatabase, etc.)
            if (_lastKnownName != name)
            {
                mainTutID = name; // sync ID to match external rename
                _lastKnownName = name;
                EditorUtility.SetDirty(this);
            }
        }
        void OnTutFieldsIdChanged()
        {
            if (string.IsNullOrWhiteSpace(mainTutID)) return;

            name = mainTutID; // sync name to match ID change
            _lastKnownName = name;
            EditorUtility.SetDirty(this);
            AssetDatabase.SaveAssets();
        }
#endif
    }
}
