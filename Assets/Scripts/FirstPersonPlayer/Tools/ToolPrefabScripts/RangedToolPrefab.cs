using System;
using FirstPersonPlayer.Combat.Player.ScriptableObjects;
using FirstPersonPlayer.Tools.Interface;
using Helpers.AnimancerHelper;
using Helpers.Events.ManagerEvents;
using Manager.ProgressionMangers;
using MoreMountains.Feedbacks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public abstract class RangedToolPrefab : MonoBehaviour, IRuntimeTool
    {
        [SerializeField] protected bool toolIsUsedOnRelease;

        [Header("References")] public Camera mainCamera;

        [SerializeField] protected MMFeedbacks equipFeedbacks;

        [Header("Accuracy Settings")] [Tooltip("Maximum spread angle in degrees at Dexterity 1")] [SerializeField]
        protected float maxSpreadAngleNotAimed = 5f;
        [Tooltip("Minimum spread angle in degrees at max Dexterity")] [SerializeField]
        protected float minSpreadAngleNotAimed = 0.5f;

        [SerializeField] protected float maxSpreadAngleAimed = 2f;
        [SerializeField] protected float minSpreadAngleAimed = 0.1f;
        [Tooltip("Dexterity level for perfect accuracy (0 spread)")] [SerializeField]
        protected int perfectAccuracyDexterity = 20;
        [Tooltip("Show debug lines for shot trajectory")] [SerializeField]
        protected bool debugAccuracy;

        // [SerializeField] protected float dexterityReductionFactor = 0.05f;
        protected AnimancerArmController AnimancerArmController;
        protected AnimancerArmController AnimController;
        protected RaycastHit LastHit;

        float maxSpreadAngle;
        float minSpreadAngle;


        public virtual void Initialize(PlayerEquipment owner)
        {
            mainCamera = Camera.main;
            maxSpreadAngle = maxSpreadAngleNotAimed;
            minSpreadAngle = minSpreadAngleNotAimed;
        }
        public virtual void Use()
        {
            PerformToolAction(HitType.Normal);
        }
        public abstract void Unequip();

        public abstract void Equip();

        public bool CanInteractWithObject(GameObject colliderGameObject)
        {
            return true;
        }
        public abstract Sprite GetReticleForTool(GameObject colliderGameObject);

        public bool ToolIsUsedOnRelease()
        {
            return toolIsUsedOnRelease;
        }
        public bool ToolMustBeHeldToUse()
        {
            return false;
        }
        public bool CanAbortAction()
        {
            throw new NotImplementedException();
        }
        public MMFeedbacks GetEquipFeedbacks()
        {
            return equipFeedbacks;
        }
        public CanBeAreaScannedType GetDetectableType()
        {
            throw new NotImplementedException();
        }
        public MMFeedbacks GetUnequipFeedbacks()
        {
            return equipFeedbacks;
        }
        public void ChargeUse(bool justPressed)
        {
            throw new NotImplementedException();
        }

        public void EnterIntoAimState()
        {
            maxSpreadAngle = maxSpreadAngleAimed;
            minSpreadAngle = minSpreadAngleAimed;
            // Debug.Log("Max,Min: " + maxSpreadAngle + "," + minSpreadAngle);
        }

        public void ExitFromAimState()
        {
            maxSpreadAngle = maxSpreadAngleNotAimed;
            minSpreadAngle = minSpreadAngleNotAimed;
            Debug.Log("Max,Min: " + maxSpreadAngle + "," + minSpreadAngle);
        }


        protected float CalculateSpreadAngle()
        {
            var attributesManager = AttributesManager.Instance;
            if (attributesManager == null) return maxSpreadAngle;

            var dexterity = attributesManager.Dexterity;

            // At Dexterity 1: maxSpreadAngle
            // At perfectAccuracyDexterity: minSpreadAngle
            // Linear interpolation
            var t = Mathf.Clamp01((float)(dexterity - 1) / (perfectAccuracyDexterity - 1));
            var spread = Mathf.Lerp(maxSpreadAngle, minSpreadAngle, t);

            if (debugAccuracy) Debug.Log($"[Pistol] Dex: {dexterity}, Spread: {spread:F2}°");

            return spread;
        }

        protected Vector3 ApplySpread(Vector3 direction, float spreadAngle)
        {
            // Convert angle to radians
            var spreadRad = spreadAngle * Mathf.Deg2Rad;

            // Random point in a circle (uniform distribution)
            var randomCircle = Random.insideUnitCircle * Mathf.Tan(spreadRad);

            // Create perpendicular vectors to the aim direction
            var right = Vector3.Cross(direction, Vector3.up).normalized;
            if (right.magnitude < 0.1f) // Handle edge case when aiming straight up/down
                right = Vector3.Cross(direction, Vector3.forward).normalized;

            var up = Vector3.Cross(right, direction).normalized;

            // Apply spread
            var spreadDirection = direction + right * randomCircle.x + up * randomCircle.y;
            return spreadDirection.normalized;
        }
        public SecondaryActionType GetSecondaryActionType()
        {
            return SecondaryActionType.AimRangedWeapon;
        }
        protected void SpawnHitFX(GameObject vfxPrefab, Vector3 position, Vector3 normal)
        {
            if (vfxPrefab == null) return;

            var vfxInstance = Instantiate(vfxPrefab, position, Quaternion.LookRotation(normal));
            var vfxFeedbacks = vfxInstance.GetComponent<MMFeedbacks>();
            vfxFeedbacks?.PlayFeedbacks();
            Destroy(vfxInstance, 2f);
        }


        public abstract void PerformToolAction(HitType hitType);

        public abstract void ApplyHit();
    }
}
