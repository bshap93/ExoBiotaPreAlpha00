using Helpers.Events.Dialog;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

namespace NewScript
{
    public class NPCPresenterGroupListener : MonoBehaviour, MMEventListener<DialoguePresentationEvent>
    {
        [Header("Unchanging Texts")]
        // Don't change its font
        [SerializeField]
        TMP_Text characterNameTextAvatar;
        [Header("NPC Side Texts")] [FormerlySerializedAs("CharacterNameText")] [SerializeField]
        TMP_Text characterNameText;
        [SerializeField] TMP_Text npcLineText;
        [SerializeField] TMP_Text lastLineText;


        [SerializeField] TMP_FontAsset modernGalacticFont;
        [SerializeField] TMP_FontAsset sheoliteFont;


        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(DialoguePresentationEvent eventType)
        {
            if (eventType.EventType == DialoguePresentationEventType.ChangeFontsOfNPCSide)
            {
                if (eventType.Language == LanguageType.ModernGalactic)
                {
                    characterNameText.font = modernGalacticFont;
                    npcLineText.font = modernGalacticFont;
                    lastLineText.font = modernGalacticFont;
                }
                else if (eventType.Language == LanguageType.Sheolite)
                {
                    characterNameText.font = sheoliteFont;
                    npcLineText.font = sheoliteFont;
                    lastLineText.font = sheoliteFont;

                    Debug.Log("Changed NPC side fonts to Sheolite");
                }
            }
        }
    }
}
