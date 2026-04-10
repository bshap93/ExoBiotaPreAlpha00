using FirstPersonPlayer.Combat.AINPC.EnemyWeapon;
using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.FPNPCs.AlienNPC;
using FirstPersonPlayer.ScriptableObjects;
using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.ActionTasks.Humacoid
{
    [Category("AttackMoves")]
    public class SingleMeleeToolEnemyHumanoidAttack : ActionTask
    {
        public readonly BBParameter<float> LocalCooldownAfterAttack = 0.0f;
        AlienNPCAnimancerController _animancerAlienController;
        EnemyAttack _attack;
        bool _attackFinished;

        float _beginningOfHitBoxWindow;
        HumanoidNPCCreature _creature;
        float _endOfHitBoxWindow;

        bool _hasAttacked;
        bool _inCooldown;
        float _timer;

        EnemyWeaponPrefab _weaponPrefab;

        public BBParameter<string> AttackEntryId;


        EnemyWeaponDefinition.AttackEntry entry;
        public BBParameter<AnimationClip> EquippedHoldPose;


        protected override string OnInit()
        {
            _creature = agent.GetComponent<HumanoidNPCCreature>();
            _animancerAlienController = agent.GetComponent<AlienNPCAnimancerController>();
            _weaponPrefab = _creature.CurrentWeaponInstance?.GetComponent<EnemyMeleeWeaponPrefab>();
            return null;
        }

        protected override void OnExecute()
        {
            _timer = 0f;
            _attackFinished = false;
            _creature.IsAttacking = true;


            entry = _creature.EquippedWeapon.GetAttackEntry(AttackEntryId.value);

            _attack = entry?.attack;


            if (entry == null)
            {
                Debug.LogWarning($"No attack entry found for {AttackEntryId.value} on {_creature.EquippedWeapon.name}");
                EndAction(false);
                return;
            }

            if (_attack == null)
            {
                Debug.LogWarning(
                    $"Attack entry {AttackEntryId.value} on {_creature.EquippedWeapon.name} has no attack assigned.");

                EndAction(false);
                return;
            }

            _beginningOfHitBoxWindow = entry.hitboxStartWindowTime;
            _endOfHitBoxWindow = entry.hitboxEndWindowTime;

            if (entry.shouldFreeWeaponPoseDuringAttack)
                _animancerAlienController.ClearUpperBody();
            
            _hasAttacked = true;

            _animancerAlienController.PlayAttackClip(
                entry.actionClip,
                null,
                () =>
                {
                    // Called by Animancer end event — play hold pose immediately so
                    // the end event doesn't re-fire on the next frame (EndEventInterrupt).
                    _attackFinished = true;
                    ReturnToHoldPose();
                }
            );
        }

        protected override void OnUpdate()
        {
            _timer += Time.deltaTime;
            
            

            if (entry == null) return;

            // Safety timer: end the action if the animation end event never fires
            // (e.g. clip shorter than AttackDuration, or end event missed).
            if (_timer > entry.AttackDuration && !_attackFinished)
            {
                _attackFinished = true;
                ReturnToHoldPose();
            }

            if (_timer >= _beginningOfHitBoxWindow && _timer <= _endOfHitBoxWindow)
            {
                // Hitbox should be active
                if (!_weaponPrefab.IsHitBoxActive)
                {
                    _weaponPrefab.SetAttack(_attack);
                    _weaponPrefab.SetHitBoxActive(true);
                }
            }
            else
            {
                // Hitbox should be inactive
                if (_weaponPrefab.IsHitBoxActive) _weaponPrefab.SetHitBoxActive(false);
            }
        }

        void ReturnToHoldPose()
        {
            _creature.IsAttacking = false;
            if (EquippedHoldPose.value != null)
                _animancerAlienController.PlayWeaponHoldPose(EquippedHoldPose.value);

            _timer = 0f;
            EndAction(true); // Call AFTER hold pose so the task isn't torn down first
        }

        protected override void OnStop()
        {
            _creature.IsAttacking = false;
            _weaponPrefab?.SetHitBoxActive(false);
            _attackFinished = false;
            _timer = 0f;
        }

        protected override void OnPause()
        {
        }
    }
}
