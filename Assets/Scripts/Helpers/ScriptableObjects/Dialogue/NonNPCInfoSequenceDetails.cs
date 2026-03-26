using UnityEngine;

namespace Helpers.ScriptableObjects.Dialogue
{
    [CreateAssetMenu(
        fileName = "NonNPCInfoSequenceDetails", menuName = "Scriptable Objects/Dialogue/NonNPCInfoSequenceDetails",
        order = 1)]
    public class NonNPCInfoSequenceDetails : ScriptableObject
    {
        public string dataSourceName;
    }
}
