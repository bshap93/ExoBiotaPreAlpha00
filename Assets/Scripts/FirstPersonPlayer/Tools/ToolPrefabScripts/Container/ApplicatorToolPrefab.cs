using System;
using FirstPersonPlayer.Tools.Animation;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes.CompositeObjects;
using Helpers.Events.ManagerEvents;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts.Container
{
    public enum ApplicatorType
    {
        LiquidDispenser
    }

    [RequireComponent(typeof(ToolBob))]
    [RequireComponent(typeof(ToolObjectController))]
    public abstract class ApplicatorToolPrefab : MonoBehaviour, IRuntimeTool
    {
        public float toolUsingRange = 5f;

        public bool toolIsUsedOnRelease;

        public MMFeedbacks effectFeedback;
        public GameObject effectVFXPrefab;

        [SerializeField] protected Sprite defaultReticleForTool;

        [SerializeField] protected CanBeAreaScannedType detectableType = CanBeAreaScannedType.BasicBioScanner;

        [SerializeField] protected LiquidType liquidType;
        [SerializeField] protected ApplicatorType applicatorType;
        protected RaycastHit LastHit;

        protected float LastTimeOfEffect = -999f;

        protected Camera MainCamera;
        public abstract void Initialize(PlayerEquipment owner);


        public abstract void Use();
        public void Unequip()
        {
        }
        public void Equip()
        {
            
        }
        public abstract bool CanInteractWithObject(GameObject colliderGameObject);
        public abstract Sprite GetReticleForTool(GameObject colliderGameObject);
        public bool ToolIsUsedOnRelease()
        {
            return toolIsUsedOnRelease;
        }
        public bool ToolMustBeHeldToUse()
        {
            return false;
        }

        public abstract bool CanAbortAction();
        public SecondaryActionType GetSecondaryActionType()
        {
            return SecondaryActionType.None;
        }

        public abstract MMFeedbacks GetEquipFeedbacks();
        public CanBeAreaScannedType GetDetectableType()
        {
            return detectableType;
        }
        public abstract MMFeedbacks GetUnequipFeedbacks();
        public void ChargeUse(bool justPressed)
        {
            throw new NotImplementedException();
        }


        public abstract void PerformToolAction();
    }
}
