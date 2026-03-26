using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.AnimancerHelper;
using Helpers.Events.ManagerEvents;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class AbilitySyringeTool : MonoBehaviour, IRuntimeTool, IToolAnimationControl
    {
        public SyringeItemObjectTool scriptableObject;
        [Header("Feedbacks")]
        [SerializeField] MMFeedbacks equippedFeedbacks;
        [SerializeField] MMFeedbacks unequippedFeedbacks;
        
        [SerializeField] MMFeedback injectionFeedbacks;
        
        
        AnimancerArmController _animController; 


        public void Initialize(PlayerEquipment owner)
        {
            _animController = owner.animancerPrimaryArmsController;

        }
        public void Use()
        {
            _animController.PlayToolUseSequence();
        }
        public void Unequip()
        {
        }
        public void Equip()
        {
        }
        public bool CanInteractWithObject(GameObject colliderGameObject)
        {
            return false;
        }
        public Sprite GetReticleForTool(GameObject colliderGameObject)
        {
            return null;
        }
        public bool ToolIsUsedOnRelease()
        {
            return true;
        }
        public bool ToolMustBeHeldToUse()
        {
            return true;
        }
        public bool CanAbortAction()
        {
            return false;
        }
        public MMFeedbacks GetEquipFeedbacks()
        {
            return equippedFeedbacks;
        }
        public CanBeAreaScannedType GetDetectableType()
        {
            return CanBeAreaScannedType.NotDetectableByScan;    
        }
        public MMFeedbacks GetUnequipFeedbacks()
        {
            return unequippedFeedbacks;
        }
        public void ChargeUse(bool justPressed = false)
        {
        }
        public void OnEquipped()
        {
        }
        public void OnUseStarted()
        {
        }
        public void OnUseStopped()
        {
        }
    }
}
