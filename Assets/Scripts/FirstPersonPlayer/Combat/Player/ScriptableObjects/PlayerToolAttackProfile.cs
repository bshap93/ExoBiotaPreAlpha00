using UnityEngine;

namespace FirstPersonPlayer.Combat.Player.ScriptableObjects
{
    [CreateAssetMenu(
        fileName = "PlayerToolAttackProfile",
        menuName = "Scriptable Objects/Character/First Person Player/Player Tool Attack Profile",
        order = 0)]
    public class PlayerToolAttackProfile : ScriptableObject
    {
        public PlayerAttack basicAttack;
        public PlayerAttack heavyAttack;
        public PlayerAttack basicStunAttack;
        public PlayerAttack heavyStunAttack;
        public float dexterityReductionFactor = 0.05f;
        public float agilityReductionFactor = 0.05f;
    }
}
