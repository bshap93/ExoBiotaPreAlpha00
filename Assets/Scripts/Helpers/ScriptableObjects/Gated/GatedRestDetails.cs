using UnityEngine;
using UnityEngine.Serialization;

namespace Helpers.ScriptableObjects.Gated
{
    [CreateAssetMenu(
        fileName = "GatedRestDetails", menuName = "Scriptable Objects/UI/Gated Rest Details", order = 1)]
    public class GatedRestDetails : GatedInteractionDetailsBase
    {
        [FormerlySerializedAs("StaminaRestoredPerMinute")]
        public float staminaRestoredPerMinute = 0.1f;
        public bool restUntilStaminaFull;
        [Header("Default Values")] [Range(0, 800)]
        public int defaultRestTimeMinutes;
    }
}
