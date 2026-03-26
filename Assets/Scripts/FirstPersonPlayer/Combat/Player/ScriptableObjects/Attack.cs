using UnityEngine;

namespace FirstPersonPlayer.Combat.Player.ScriptableObjects
{
    public abstract class Attack : ScriptableObject
    {
        public float rawDamage;
        public float critChance;
        public float critMultiplier;

        public float rawKnockbackForce;
    }
}
