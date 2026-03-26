using Helpers.Events.ManagerEvents;
using MoreMountains.Feedbacks;
using UnityEngine;

namespace FirstPersonPlayer.Tools.Interface
{
    public enum SecondaryActionType
    {
        AimRangedWeapon,
        BlockWithMeleeWeapon,
        InjectAvailableIchor,
        None
    }

    public interface IRuntimeTool
    {
        /// Called right after the prefab is spawned & parented.
        void Initialize(PlayerEquipment owner);

        /// Fire / use the tool.
        void Use();

        /// Called before being destroyed / unequipped.
        void Unequip();

        void Equip();

        bool CanInteractWithObject(GameObject colliderGameObject);
        // int GetCurrentTextureIndex();
        // bool CanInteractWithTextureIndex(int terrainIndex);

        Sprite GetReticleForTool(GameObject colliderGameObject);

        bool ToolIsUsedOnRelease();

        bool ToolMustBeHeldToUse();

        bool CanAbortAction();


        MMFeedbacks GetEquipFeedbacks();

        CanBeAreaScannedType GetDetectableType();


        //MMFeedbacks GetUseToolFeedbacks();
        MMFeedbacks GetUnequipFeedbacks();
        void ChargeUse(bool justPressed = false);
    }
}
