using System;
using FirstPersonPlayer.Tools.Interface;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using Helpers.Events;
using Helpers.Events.ManagerEvents;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;

namespace FirstPersonPlayer.Tools.ToolPrefabScripts
{
    public class StormLanternLightTool : MonoBehaviour, IRuntimeTool
    {
        [FormerlySerializedAs("LanternMaterial")] [SerializeField]
        Material lanternMaterial; // Material for the lantern light

        [SerializeField] MMFeedbacks switchOnFB; // Feedback for switching on the lantern
        [SerializeField] MMFeedbacks switchOffFB; // Feedback for switching off the lantern


        [SerializeField] Light stormLanternPointLight; // Point light component for the lantern

        [SerializeField] MMFeedbacks stormLanternPointLightFeedback;
        [SerializeField] MMFeedbacks toggleStormLanternFeedback;
        [SerializeField] LightSourceToolItemObject stormLanternLightSourceToolItemObject;

        [SerializeField] MMFeedbacks equippedFeedbacks;


        bool _isLanternOn; // State to track if the lantern is on
        LightSourceToolItemObject _lightSourceToolItemObject;

        public void Initialize(PlayerEquipment owner)
        {
            if (!(owner is PlayerEquipment))
            {
                Debug.LogError("StormLanternLightTool: Owner is not of type LeftPlayerEquipment.");
                return;
            }

            if (lanternMaterial == null)
            {
                lanternMaterial = GetComponent<Renderer>()?.material;
                if (lanternMaterial == null)
                    Debug.LogError(
                        "StormLanternLightTool: LanternMaterial is not assigned and could not be found on the GameObject.");
            }

            if (stormLanternPointLight == null)
            {
                stormLanternPointLight = GetComponentInChildren<Light>();
                if (stormLanternPointLight == null)
                    Debug.LogError(
                        "StormLanternLightTool: stormLanternPointLight is not assigned and could not be found in children.");
            }

            if (_lightSourceToolItemObject == null)
                _lightSourceToolItemObject = owner.CurrentToolSo as LightSourceToolItemObject;

            LightEvent.Trigger(_lightSourceToolItemObject != null ? LightEventType.TurnOn : LightEventType.TurnOff);
        }

        public void Use()
        {
            // Toggle the lantern light on or off
            _isLanternOn = !_isLanternOn;
            stormLanternPointLight.enabled = _isLanternOn;
            if (_isLanternOn)
                switchOnFB?.PlayFeedbacks();
            else
                switchOffFB?.PlayFeedbacks();

            toggleStormLanternFeedback?.PlayFeedbacks();
            LightEvent.Trigger(_isLanternOn ? LightEventType.TurnOn : LightEventType.TurnOff);
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
            return false;
        }
        public bool ToolMustBeHeldToUse()
        {
            return false;
        }

        public bool CanAbortAction()
        {
            return false;
        }
        public SecondaryActionType GetSecondaryActionType()
        {
            return SecondaryActionType.None;
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
            return equippedFeedbacks;
        }
        public void ChargeUse(bool justPressed)
        {
            throw new NotImplementedException();
        }

        public int GetCurrentTextureIndex()
        {
            return -1;
        }

        public bool CanInteractWithTextureIndex(int terrainIndex)
        {
            return false;
        }
    }
}
