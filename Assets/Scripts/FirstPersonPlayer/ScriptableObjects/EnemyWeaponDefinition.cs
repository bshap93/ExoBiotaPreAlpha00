using System;
using System.Linq;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using Helpers.ScriptableObjects.Animation;
using UnityEngine;

namespace FirstPersonPlayer.ScriptableObjects
{
    [Serializable]
    public enum EnemyWeaponType
    {
        Melee,
        Ranged
    }

    [CreateAssetMenu(
        fileName = "EnemyWeaponSO", menuName = "Scriptable Objects/Character/Enemy NPC/Enemy Weapon Definition",
        order = 0)]
    public class EnemyWeaponDefinition : ScriptableObject
    {
        public string displayName;
        public string enemyWeaponId;
        // public EnemyAttack[] attacks;
        public GameObject enemyWeaponPrefab;
        public EnemyWeaponType enemyWeaponType;
        public EnemyToolWeaponAnimationSet animationSet;
        public AttackEntry[] attackEntries;

        public AnimationClip HoldPoseClip => animationSet.holdPoseClip;

        public AttackEntry GetAttackEntry(string id)
        {
            return attackEntries?.FirstOrDefault(e => e.attackEntryId == id);
        }

        [Serializable]
        public class AttackEntry
        {
            public string attackEntryId;
            public EnemyAttack attack;
            public bool shouldFreeWeaponPoseDuringAttack = true;
            public EnemyToolWeaponAnimationSet.ActionClip actionClip;
            public float hitboxStartWindowTime;
            public float hitboxEndWindowTime;
            public float AttackDuration => actionClip?.Duration ?? 0f;
        }
    }
}
