using System;
using System.Collections.Generic;
using Dirigible.Input;
using FirstPersonPlayer.ScriptableObjects;
using Helpers.Events;
using Manager;
using Plugins.HighlightPlus.Runtime.Scripts;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace FirstPersonPlayer.Interactable.BioOrganism
{
    public abstract class BioOrganismBase : MonoBehaviour, IBillboardable, IHoverable, IRequiresUniqueID
    {
        [FormerlySerializedAs("UniqueID")] [FoldoutGroup("Identity")]
        public string uniqueID;

        [FoldoutGroup("Identity")] [FormerlySerializedAs("BioOrganismType")]
        public BioOrganismType bioOrganismType;

        [Header("Action Info")]
#if UNITY_EDITOR
        [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
        public int actionId;

        public string actionText;
        [SerializeField] protected HighlightTrigger trigger;


        protected SceneObjectData data;

        // NEW: capability flag the manager can query
        public virtual bool SupportsSampling => false;

        // NEW: if SupportsSampling==true, manager seeds this many attempts for the node
        public virtual int DefaultSamplingAllowance => 0;

        protected virtual void Awake()
        {
            if (trigger == null)
                trigger = GetComponent<HighlightTrigger>();
        }

        protected virtual void OnEnable()
        {
        }

        protected virtual void OnDisable()
        {
        }

        // ===== IBillboardable common helpers =====
        public virtual string GetName()
        {
            return bioOrganismType != null ? bioOrganismType.organismName : "Unknown Organism";
        }

        public virtual Sprite GetIcon()
        {
            return bioOrganismType != null ? bioOrganismType.organismIcon : null;
        }

        public virtual string ShortBlurb()
        {
            return bioOrganismType != null
                ? bioOrganismType.shortDescription
                : bioOrganismType.UnknownDescription ?? "An unrecognized organism.";
        }

        public virtual Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.bioOrganismIcon;
        }

        public string GetActionText()
        {
            return "Avoid";
        }

        // ===== Hover pipeline (shared) =====

        public virtual bool OnHoverStart(GameObject go)
        {
            data = new SceneObjectData(
                GetName(), GetIcon(), ShortBlurb(), ExaminationManager.Instance.iconRepository.bioOrganismIcon,
                GetActionText());

            BillboardEvent.Trigger(data, BillboardEventType.Show);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, actionId, string.IsNullOrEmpty(actionText) ? null : actionText,
                    ExaminationManager.Instance.iconRepository.bioOrganismIcon);

            return true;
        }

        public virtual bool OnHoverStay(GameObject go)
        {
            return true;
        }

        public virtual bool OnHoverEnd(GameObject go)
        {
            if (data == null) data = SceneObjectData.Empty();
            BillboardEvent.Trigger(data, BillboardEventType.Hide);
            if (actionId != 0)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Hide, actionId, string.IsNullOrEmpty(actionText) ? null : actionText);

            return true;
        }
        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }

#if UNITY_EDITOR
        public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
        {
            return AllRewiredActions.GetAllRewiredActions();
        }

#endif

        // Per-derived-type action text (e.g., “Sample Biomass”, “Clear Growth”, “Toxic Cloud”)
        protected abstract string GetActionText(bool recognizableOnSight);
    }
}
