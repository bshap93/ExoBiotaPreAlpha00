using UnityEngine;
using Yarn.Unity;

namespace Objectives
{
    [CreateAssetMenu(fileName = "YarnInitScriptSet", menuName = "Scriptable Objects/Dialogue/Yarn Init Script Set")]
    public class YarnInitScriptSet : ScriptableObject
    {
        public YarnProject yarnProject;
        public string[] nodesToRun;
    }
}