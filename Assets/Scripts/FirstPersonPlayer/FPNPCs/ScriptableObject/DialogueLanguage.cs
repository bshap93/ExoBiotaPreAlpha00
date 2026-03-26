using TMPro;
using UnityEngine;

namespace FirstPersonPlayer.FPNPCs.ScriptableObject
{
    [CreateAssetMenu(
        fileName = "DialogueLanguage", menuName = "Scriptable Objects/Character/DialogueLanguage", order = 1)]
    public class DialogueLanguage : UnityEngine.ScriptableObject
    {
        [SerializeField] TMP_FontAsset defaultFont;
        [SerializeField] TMP_FontAsset secondaryFont;
    }
}
