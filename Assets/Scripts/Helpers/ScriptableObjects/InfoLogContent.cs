using UnityEngine;

namespace Helpers.ScriptableObjects
{
    [CreateAssetMenu(fileName = "InfoLogContent", menuName = "Scriptable Objects/UI/InfoLogContent", order = 1)]
    public class InfoLogContent : ScriptableObject
    {
        public string title;
        public string dateText;
        public string author;
        public Sprite authorIcon;
        [TextArea(5, 20)] public string body;
    }
}
