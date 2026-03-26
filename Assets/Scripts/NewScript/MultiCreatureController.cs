using FirstPersonPlayer.Combat.AINPC.ScriptableObjects;
using FirstPersonPlayer.FPNPCs.AlienNPC;
using FirstPersonPlayer.Interactable.BioOrganism.Creatures;
using Helpers.Events.Progression;
using LevelConstruct.Highlighting;
using MoreMountains.Feedbacks;
using UnityEngine;
using UnityEngine.Serialization;
using Utilities.Interface;

namespace NewScript
{
    public class MultiCreatureController : MonoBehaviour, IRequiresUniqueID
    {
        [SerializeField] CreatureController hostileCreatureController;
        [SerializeField] CreatureController neutralCreatureController;
        [SerializeField] AlienNPCAnimancerController alienNPCAnimancerController;
        [SerializeField] GameObject enableNeutralVFX;
        [SerializeField] HighlightEffectController highlightEffectController;
        [Header("Layer Settings")] [SerializeField]
        int neutralCreatureLayer;
        [FormerlySerializedAs("enableNeutralCreatureFeedbacks")] [Header("Feedbacks")] [SerializeField]
        MMFeedbacks additionalEnableNeutralCreatureFB;

        CreatureType creatureType;

        void Awake()
        {
        }
        public string UniqueID { get; private set; }
        public void SetUniqueID()
        {
            UniqueID = hostileCreatureController.UniqueID;
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(UniqueID);
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void EnableNeutralCreature()
        {
            additionalEnableNeutralCreatureFB?.PlayFeedbacks();

            enableNeutralVFX.SetActive(true);

            if (hostileCreatureController != null)
            {
                hostileCreatureController.SetCannotBeAttacked(true);
                creatureType = hostileCreatureController.creatureType;
                var xpToAward = 0;
                if (creatureType.givesExperienceReward)
                    xpToAward = hostileCreatureController.creatureType.experienceRewardAmount;

                if (xpToAward > 0)
                    EnemyXPRewardEvent.Trigger(xpToAward);

                hostileCreatureController.enabled = false;
            }

            if (alienNPCAnimancerController != null)
                alienNPCAnimancerController.enabled = true;

            if (neutralCreatureController != null)
            {
                neutralCreatureController.enabled = true;
                neutralCreatureController.SetCannotBeAttacked(true);
            }

            if (highlightEffectController != null)
                highlightEffectController.SetSecondaryStateHighlightColor();

            if (neutralCreatureController != null)
                SetLayerRecursively(neutralCreatureController.gameObject, neutralCreatureLayer);
        }

        void SetLayerRecursively(GameObject obj, int layer)
        {
            obj.layer = layer;

            foreach (Transform child in obj.transform) SetLayerRecursively(child.gameObject, layer);
        }
    }
}
