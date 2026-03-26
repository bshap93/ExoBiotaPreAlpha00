using UnityEngine;

namespace Manager.SceneManagers.Dock
{
    [CreateAssetMenu(fileName = "DockInteractionFeedbacks",
        menuName = "Scriptable Objects/Feedback Helpers/DockInteractionFeedbacks", order = 1)]
    public class DockInteractionFeedbacks : ScriptableObject
    {
        public GameObject startConversationFeedbacksPrefab;
        public GameObject endConversationFeedbacksPrefab;
    }
}