using System;
using Helpers.ScriptableObjects.Animation;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ItemObjectTypes.Tools
{
    [CreateAssetMenu(fileName = "BaseTool", menuName = "Scriptable Objects/Items/BaseTool", order = 0)]
    [Serializable]
    public class BaseTool : MyBaseItem
    {
        public enum ArmsType
        {
            Primary,
            Secondary
        }

        [Header("Runtime")] public GameObject FPToolPrefab; // must have an IRuntimeTool on it

        [FormerlySerializedAs("Cooldown")] public float cooldown; // optional, leave 0 to ignore

        public ToolAnimationSet toolAnimationSet;
        public bool hasObjectivesEquipping;
        [Header("Arm Visibility")] public bool hidesArmWhenEquipped;

        public ArmsType armsType = ArmsType.Primary;
        public bool canBeAimed;

        public bool occupiesBothHands;
        public bool canBlock;
        [ShowIf("canBlock")] public float blockDamageMultiplierAgainstMelee = 1.0f;
        [ShowIf("canBlock")] public float blockDamageMultiplierAgainstRanged = 1.0f;

        public bool usesStamina;
        [ShowIf("usesStamina")] public float staminaRestoreRateMultiplier = 1.0f;
        public bool canUseSingleShotSecondaryAction;
    }
}
