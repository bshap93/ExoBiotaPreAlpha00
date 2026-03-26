using System.Collections;
using FirstPersonPlayer.Tools;
using Helpers.Events;
using Helpers.Events.Feedback;
using MoreMountains.Feedbacks;
using MoreMountains.InventoryEngine;
using MoreMountains.Tools;
using SharedUI.InputsD;
using UnityEngine;

namespace Manager.FeedbackControllers
{
    public class ConsumableFeedbackController : MonoBehaviour, MMEventListener<ConsumableFeedbackEvent>
    {
        [SerializeField] MMFeedbacks injectableAbilityItemUsedFeedback;
        [SerializeField] MMSoundManagerSound injectableAbilityItemUsedSound;
        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(ConsumableFeedbackEvent eventType)
        {
            if (eventType.FeedbackEventType == ConsumableFeedbackEventType.InjectableAbilityItemUsed)
                StartCoroutine(EquipJustInjectedAbilityAfterDelay(eventType));
        }

        IEnumerator EquipJustInjectedAbilityAfterDelay(ConsumableFeedbackEvent eventType)
        {
            Debug.Log("Starting coroutine to equip injected ability after delay.");
            yield return new WaitForSeconds(0.1f); // Adjust the delay as needed


            DefaultInput.ToggleIGUI();

            // yield return new WaitForSeconds(2f);

            injectableAbilityItemUsedFeedback?.PlayFeedbacks();

            var playerEquipment = PlayerEquipment.Instance;
            if (playerEquipment != null)
                if (playerEquipment.IsRanged)
                {
                    AlertEvent.Trigger(
                        AlertReason.RangedWeaponInUse,
                        "Cannot equip biotic ability while a ranged weapon is equipped.",
                        "Cannot Equip Ability");

                    yield break;
                }


            var index = -1;

            var playerMainInventory =
                MoreMountains.InventoryEngine.Inventory.FindInventory("PlayerMainInventory", "Player1");

            if (playerMainInventory == null)
            {
                Debug.LogError("PlayerMainInventory not found for Player1.");
                yield break;
            }

            // Find the index of the just injected ability in the inventory
            for (var i = 0; i < playerMainInventory.Content.Length; i++)
            {
                var item = playerMainInventory.Content[i];
                if (item != null && item.ItemID == eventType.BioticAbilityInvItem.ItemID)
                {
                    index = i;
                    Debug.Log($"Found injected ability at index {index} in PlayerMainInventory.");
                    break;
                }
            }

            if (index == -1)
            {
                Debug.LogError("Injected ability item not found in PlayerMainInventory.");
                yield break;
            }


            MMInventoryEvent.Trigger(
                MMInventoryEventType.EquipRequest, null, "PlayerMainInventory", eventType.BioticAbilityInvItem, 1,
                index,
                "Player1");
        }
    }
}
