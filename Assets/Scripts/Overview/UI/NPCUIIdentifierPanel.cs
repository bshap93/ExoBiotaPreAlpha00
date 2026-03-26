using TMPro;
using UnityEngine;

namespace Overview.UI
{
    public class NPCUIIdentifierPanel : MonoBehaviour
    {
        [SerializeField] TMP_Text npcNameText;


        public void SetInfo(string name)
        {
            if (name == null)
            {
                Debug.LogError("NPC Definition characterName is null.");
                return;
            }

            npcNameText.text = name;
        }
    }
}
