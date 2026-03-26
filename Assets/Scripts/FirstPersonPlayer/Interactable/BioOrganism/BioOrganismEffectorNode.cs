using FirstPersonPlayer.Interface;
using Helpers.Events;
using Helpers.Events.Status;
using Manager;
using MoreMountains.InventoryEngine;
using SharedUI.Interface;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.BioOrganism
{
    public enum EffectorType
    {
        Bargain
    }

    public enum EffectType
    {
        Damage,
        Contamination,
        Stamina
    }

    public enum BoonType
    {
        Health,
        Contamination,
        None
    }


    public class BioOrganismEffectorNode : BioOrganismBase, IInteractable
    {
        public EffectorType effectorType;
        public EffectType effectType;
        public BoonType boonType;
        [SerializeField] float effectorAmount;
        [SerializeField] float effectorDuration = 0.1f;
        [SerializeField] float boonValue;

        [SerializeField] PlayerStatsEvent.StatChangeCause causeOfEffect;

        [SerializeField] protected float interactionDistance = 2f;


        public void Interact()
        {
            if (effectType == EffectType.Damage)
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentHealth,
                    PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    effectorAmount, effectorDuration, causeOfEffect);
            else if (effectType == EffectType.Contamination)
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentContamination,
                    PlayerStatsEvent.PlayerStatChangeType.Increase,
                    effectorAmount, effectorDuration, causeOfEffect);
            else if (effectType == EffectType.Stamina)
                PlayerStatsEvent.Trigger(
                    PlayerStatsEvent.PlayerStat.CurrentStamina,
                    PlayerStatsEvent.PlayerStatChangeType.Decrease,
                    effectorAmount, effectorDuration, causeOfEffect);
        }
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
        }

        public void OnUnfocus()
        {
        }
        public float GetInteractionDistance()
        {
            return interactionDistance;
        }

        public void OnInteractionEnd()
        {
        }
        public override bool OnHoverStart(GameObject go)
        {
            if (!bioOrganismType) return true;

            var recognizable = bioOrganismType.identificationMode == IdentificationMode.RecognizableOnSight;

            var showKnown = recognizable; // later: OR with analysis progression
            var nameToShow = showKnown ? bioOrganismType.organismName : bioOrganismType.UnknownName;
            var iconToShow = showKnown
                ? bioOrganismType.organismIcon
                : bioOrganismType.organismIcon ?? ExaminationManager.Instance?.defaultUnknownIcon;

            var shortToShow = showKnown ? bioOrganismType.shortDescription : string.Empty;

            data = new SceneObjectData(
                nameToShow,
                iconToShow,
                shortToShow,
                ExaminationManager.Instance?.iconRepository.bioOrganismIcon,
                GetActionText(recognizable)
            );

            data.Id = bioOrganismType.organismID;

            BillboardEvent.Trigger(data, BillboardEventType.Show);
            if (actionId != 0)
                if (ExaminationManager.Instance != null)
                    ControlsHelpEvent.Trigger(
                        ControlHelpEventType.Show, actionId,
                        string.IsNullOrEmpty(actionText) ? null : actionText,
                        ExaminationManager.Instance.iconRepository.pushIcon);

            return true;
        }


        protected override string GetActionText(bool recognizableOnSight)
        {
            return "Hurt Me";
        }
    }
}
