using TMPro;
using UnityEngine;

namespace SharedUI.Progression
{
    public class LevelNotify : MonoBehaviour
    {
        [SerializeField] TMP_Text newLevelText;
        public void SetLevelText(int levelAmount)
        {
            newLevelText.text = levelAmount.ToString();
        }
    }
}
