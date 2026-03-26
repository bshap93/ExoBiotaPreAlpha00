using System;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events;
using Manager;
using Manager.ProgressionMangers;
using MoreMountains.Feedbacks;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class SwordToolPrefab : MeleeToolPrefab, IRuntimeTool
    {
        [Header("Sword Settings")] [Tooltip("Tags this sword is allowed to affect (e.g., BioObstacle, Vegetation).")]
        public string[] allowedTags;
        public float baseStaminaCostPerConnectingSwing = 1.5f;

        [Tooltip("Number of seconds between swings.")]
        public float swingCooldown = 0.8f;

        public int swordPower = 1;

        [SerializeField] Sprite defaultReticleForTool;

        [SerializeField] protected float lastSwingTime = -999f;

        float StaminaCostPerNormalConnectingSwing
        {
            get
            {
                var attrMgr = AttributesManager.Instance;
                if (attrMgr == null) return baseStaminaCostPerConnectingSwing;

                var agility = attrMgr.Agility;
                var reduction = toolAttackProfile.agilityReductionFactor * agility; // Example: 0.05
                var finalCost = baseStaminaCostPerConnectingSwing * (1f - reduction);

                return Mathf.Max(0.1f, finalCost); // Ensure a minimum cost
            }
        }
        public override void Use()
        {
            if (PlayerMutableStatsManager.Instance.CurrentStamina < StaminaCostPerNormalConnectingSwing)
            {
                // Not enough stamina
                AlertEvent.Trigger(
                    AlertReason.NotEnoughStamina, "Not enough stamina to use pickaxe.", "Insufficient Stamina");

                return;
            }

            if (attributesManager == null) attributesManager = AttributesManager.Instance;

            PerformToolAction();
        }
        public override void ChargeUse(bool justPressed)
        {
            throw new NotImplementedException();
        }
        public override bool CanInteractWithObject(GameObject target)
        {
            if (target == null) return false;

            // Component gate
            if (target.TryGetComponent<IDamageable>(out _)) return true;

            // Tag gate
            if (allowedTags != null && allowedTags.Length > 0)
            {
                var t = target.tag;
                for (var i = 0; i < allowedTags.Length; i++)
                    if (!string.IsNullOrEmpty(allowedTags[i]) && t == allowedTags[i])
                        return true;
            }

            return false;
        }
        public override void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            AnimController = owner.animancerPrimaryArmsController;
        }
        public override Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            // Check if the object has a tag that should show inability reticle
            if (tagsWhichShouldShowInabilityReticle != null)
                foreach (var tagName in tagsWhichShouldShowInabilityReticle)
                    if (colliderGameObject.CompareTag(tagName))
                        return reticleForHittable;

            // Default to the normal reticle
            return defaultReticleForTool;
        }
        public override MMFeedbacks GetEquipFeedbacks()
        {
            return equipFeedbacks;
        }
        public override MMFeedbacks GetUnequipFeedbacks()
        {
            return unequippedFeedbacks;
        }
        public override void Unequip()
        {
            // no-op for now
        }
        public override void ApplyHit(HitType hitType = HitType.Normal)
        {
            throw new NotImplementedException();
        }
        public override void PerformToolAction()
        {
            throw new NotImplementedException();
        }

        public override void PerformHeavyChargedToolAction()
        {
            throw new NotImplementedException();
        }
    }
}
