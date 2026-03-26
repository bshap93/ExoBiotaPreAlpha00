using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Interface;
using Sirenix.OdinInspector;
using UnityEngine;

namespace LevelConstruct.Interactable
{
    public abstract class ButtonActivatedBase : MonoBehaviour, IInteractable
    {
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        [SerializeField] protected float interactionDistance = 2f;
        public abstract void Interact();
        public void Interact(string param)
        {
            throw new System.NotImplementedException();
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
            // This method can be used to highlight the button or show some UI feedback
            // when the player focuses on the interactable object.
        }

        public void OnUnfocus()
        {
            // This method can be used to remove any highlight or UI feedback
            // when the player unfocuses from the interactable object.
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }
#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }
#endif

        public void OnInteractionEnd()
        {
        }
    }
}
