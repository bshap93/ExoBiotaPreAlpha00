using System;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.Interface;
using JournalData.Objectives.ScriptableObjects;
using Michsky.MUIP;
using Objectives;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace FirstPersonPlayer.UI.LocationButtonBase
{
    public abstract class OverviewModeLocationButtons : MonoBehaviour, IInteractable
    {
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;


        [FormerlySerializedAs("_locationText")] [SerializeField]
        protected TMP_Text locationText;
        [FormerlySerializedAs("ButtonManagers")]
        public List<ButtonManager> buttonManagers;
        public Image emphasizedIcon;

        [Header("Conditional Dialogue Nodes")] public
            DialogueCondition[] dialogueConditions;
        public string defaultStartNode;
        CanvasGroup _enclosingCanvasGroup;
        protected Transform CameraAnchorTransform;
        protected string LocationId;

        protected virtual void Awake()
        {
            _enclosingCanvasGroup = GetComponentInParent<CanvasGroup>();
            if (_enclosingCanvasGroup == null) Debug.LogError("CanvasGroup component is missing on the GameObject.");
        }


        public abstract void Interact();
        public void Interact(string param)
        {
            throw new NotImplementedException();
        }


        public virtual void OnInteractionStart()
        {
        }
        public void OnInteractionEnd(string param)
        {
        }

        public virtual bool CanInteract()
        {
            return true;
        }

        public virtual bool IsInteractable()
        {
            return true;
        }

        public virtual void OnFocus()
        {
        }

        public virtual void OnUnfocus()
        {
        }
        public float GetInteractionDistance()
        {
            return 2f;
        }

        public virtual void OnInteractionEnd()
        {
        }
#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }
#endif

        public void SetEmphasized(bool emphasized)
        {
            // Optional: Implement visual feedback for emphasis
            // For example, change color or scale of the button
            if (locationText != null)
            {
                var locationTextColorNormal = new Color32(0xEE, 0xF2, 0xBA, 0xFF);
                var locationTextColorEmphasized = new Color32(0xFF, 0x75, 0x00, 0xFF);
                if (emphasized)
                {
                    // Specify an RGB by hex code
                    locationText.color = locationTextColorEmphasized;
                    locationText.fontStyle = FontStyles.Italic | FontStyles.Bold;
                    emphasizedIcon.gameObject.SetActive(true);
                }
                else
                {
                    locationText.color = locationTextColorNormal;
                    locationText.fontStyle = FontStyles.Normal;
                    emphasizedIcon.gameObject.SetActive(false);
                }

                locationText.color = emphasized ? locationTextColorEmphasized : locationTextColorNormal;
            }
        }

        // protected bool RequireAccessPass()
        // {
        //     if (access == null) return true; // no requirement
        //     if (access.CanOpen())
        //     {
        //         access.MarkOpenedIfPermanent();
        //         return true;
        //     }
        //
        //     // TODO: plug your “LOCKED” UI/audio here
        //     Debug.Log("Locked: missing key(s)");
        //     AlertEvent.Trigger(
        //         AlertReason.KeyAccessDenied,
        //         "Your inserts lack the requisite permissions to enter this door.", "Digital Key Not Foudnd");
        //
        //     return false;
        // }

        public virtual void HideCanvasGroup()
        {
            if (_enclosingCanvasGroup != null)
            {
                _enclosingCanvasGroup.alpha = 0f;
                _enclosingCanvasGroup.interactable = false;
                _enclosingCanvasGroup.blocksRaycasts = false;
            }
            else
            {
                Debug.LogError("CanvasGroup is not initialized.");
            }
        }

        public virtual void ShowCanvasGroup()
        {
            if (_enclosingCanvasGroup != null)
            {
                _enclosingCanvasGroup.alpha = 1f;
                _enclosingCanvasGroup.interactable = true;
                _enclosingCanvasGroup.blocksRaycasts = true;
            }
            else
            {
                Debug.LogError("CanvasGroup is not initialized.");
            }
        }

        protected string GetAppropriateStartNode()
        {
            var objectivesManager = ObjectivesManager.Instance;
            if (objectivesManager == null)
            {
                Debug.LogWarning("[CommsConsole] ObjectivesManager not found, using default node");
                return defaultStartNode;
            }

            // Check each condition in order
            if (dialogueConditions != null)
                foreach (var condition in dialogueConditions)
                    if (condition.CheckCondition(objectivesManager))
                        return condition.startNode;

            // Fallback to original override
            return defaultStartNode;
        }
    }
}
