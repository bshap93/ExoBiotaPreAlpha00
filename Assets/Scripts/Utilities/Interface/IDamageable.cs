using System;
using DG.Tweening;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using UnityEngine;

namespace Utilities.Interface
{
    public interface IDamageable
    {
        public void PlayHitAnimation(AnimationClip value);

        public void OnDeath();

        public void ProcessAttackDamage(PlayerAttack playerAttack, Vector3 origin);

        void PlayHitTween(Func<Transform, Tween> buildTween, bool killPrevious = true);
    }
}
