using UnityEngine;

namespace SharedUI.Alert
{
    [CreateAssetMenu(
        fileName = "PauseAndGiveInfoDetails", menuName = "Scriptable Objects/Alerts/PauseAndGiveInfoDetails",
        order = 1)]
    public class PauseAndGiveInfoDetails : ScriptableObject
    {
        public string title;
        public Sprite descriptorImage;
        [TextArea] public string shortDescription;
        [TextArea] public string longDescription;
    }
}
