using FirstPersonPlayer.Interactable.ResourceBoxes;
using Manager;
using MoreMountains.Feedbacks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Trade
{
    public class ResourceNotify : MonoBehaviour
    {
        [SerializeField] Image resourceTypeIcon;
        [SerializeField] TMP_Text resourceAmtText;
        [SerializeField] TMP_Text resourceUnitsText;

        [SerializeField] MMFeedbacks addResourceFeedback;
        [SerializeField] MMFeedbacks removeResourceFeedback;

        void OnEnable()
        {
        }

        public void SetResourceAmountAndType(ResourceCollectionContainerInteractable.ResourceType resourceType,
            string resourceTypeAmount)
        {
            switch (resourceType)
            {
                case ResourceCollectionContainerInteractable.ResourceType.Scrap:
                    resourceTypeIcon.sprite = ExaminationManager.Instance.iconRepository.scrapIcon;
                    resourceAmtText.text = resourceTypeAmount;
                    resourceUnitsText.text = "KG";

                    break;

                default:
                    resourceTypeIcon.sprite = ExaminationManager.Instance.iconRepository.resourceIcon;
                    resourceAmtText.text = resourceTypeAmount;
                    resourceUnitsText.text = "?";
                    break;
            }
        }
    }
}
