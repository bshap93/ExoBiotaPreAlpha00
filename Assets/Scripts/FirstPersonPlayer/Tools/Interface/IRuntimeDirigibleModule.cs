using Dirigible;
using UnityEngine;

namespace FirstPersonPlayer.Tools.Interface
{
    public interface IRuntimeDirigibleModule
    {
        /// Called right after the prefab is spawned & parented.
        void Initialize(DirigibleEquipment owner);

        /// Fire / use the tool.
        void Use();

        /// Called before being destroyed / unequipped.
        void Unequip();

        bool CanInteractWithObject(GameObject colliderGameObject);
        // int GetCurrentTextureIndex();
        // bool CanInteractWithTextureIndex(int terrainIndex);

        int GetMainActionID();
    }
}