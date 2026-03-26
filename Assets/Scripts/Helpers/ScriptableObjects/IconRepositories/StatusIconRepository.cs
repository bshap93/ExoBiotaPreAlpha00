using UnityEngine;

namespace Helpers.ScriptableObjects.IconRepositories
{
    [CreateAssetMenu(fileName = "IconRepository", menuName = "Scriptable Objects/Helpers/IconRepository", order = 0)]
    public class StatusIconRepository : ScriptableObject
    {
        public Sprite healthIcon;
        public Sprite staminaIcon;
        public Sprite contaminationIcon;
        public Sprite visionIcon;
    }
}
