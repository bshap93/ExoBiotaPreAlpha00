using EditorScripts;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "MetaTerminal", menuName = "Scriptable Objects/Machines/MetaTerminal",
        order = 1)]
    public class MetaTerminalInfoSO : ScriptableObject
    {
        public string gameObjectUniqueID;
        public string terminalName;

        [SerializeField] [InlineProperty] [HideLabel]
        SpawnInfoEditor overrideSpawnInfo;
        public string shortBlurb;
    }
}
