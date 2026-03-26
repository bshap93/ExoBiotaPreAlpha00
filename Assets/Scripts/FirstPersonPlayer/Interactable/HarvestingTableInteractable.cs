using System;
using FirstPersonPlayer.Interface;
using Helpers.Events;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable
{
    [DisallowMultipleComponent]
    public class HarvestingTableInteractable : MonoBehaviour, IRequiresUniqueID, IInteractable, IHoverable,
        IBillboardable
    {
        [SerializeField] float interactionDistance = 3f;
        public string harvestingTableId;
        [SerializeField] Sprite icon;
        [SerializeField] string shortBlurb;

#if UNITY_EDITOR
        [ValueDropdown("@AllRewiredActions.GetAllRewiredActions()")]
#endif
        public int actionId;

        SceneObjectData _data;
        public string GetName()
        {
            return "Harvesting Table";
        }
        public Sprite GetIcon()
        {
            return icon;
        }
        public string ShortBlurb()
        {
            return shortBlurb;
        }
        public Sprite GetActionIcon()
        {
            return null;
        }
        public string GetActionText()
        {
            return "Use";
        }
        public bool OnHoverStart(GameObject go)
        {
            _data = new SceneObjectData(
                GetName(), GetIcon(), shortBlurb, GetActionIcon(), GetActionText());

            BillboardEvent.Trigger(_data, BillboardEventType.Show);

            return true;
        }
        public bool OnHoverStay(GameObject go)
        {
            return true;
        }
        public bool OnHoverEnd(GameObject go)
        {
            BillboardEvent.Trigger(_data, BillboardEventType.Hide);
            return true;
        }
        public void Interact()
        {
            Debug.Log("Starting interaction");
        }
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }
        public void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }
        public bool CanInteract()
        {
            return true;
        }
        public bool IsInteractable()
        {
            return true;
        }
        public void OnFocus()
        {
        }
        public void OnUnfocus()
        {
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }

        public string UniqueID => harvestingTableId;
        public void SetUniqueID()
        {
            harvestingTableId = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(harvestingTableId);
        }
    }
}
