using System;
using DG.Tweening;
using MoreMountains.Feedbacks;
using UnityEngine;
using Utilities.Interface;

namespace LevelConstruct.Interactable.Door
{
    public class AutoDoorSliding : MonoBehaviour, IRequiresUniqueID
    {
        [SerializeField] GameObject rightDoor;
        [SerializeField] GameObject leftDoor;
        [SerializeField] Vector3 rightDoorOpenPosition;
        [SerializeField] Vector3 leftDoorOpenPosition;
        [SerializeField] Vector3 rightDoorClosedPosition;
        [SerializeField] Vector3 leftDoorClosedPosition;
        [SerializeField] float openCloseDuration = 1f;
        [SerializeField] MMFeedbacks doorOpenFeedbacks;
        [SerializeField] MMFeedbacks doorCloseFeedbacks;

        [SerializeField] string uniqueID;

        public string UniqueID => uniqueID;
        public void SetUniqueID()
        {
            uniqueID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(uniqueID);
        }

        public void OpenDoor()
        {
            // DoTween
            doorOpenFeedbacks?.PlayFeedbacks();
            rightDoor.transform.DOLocalMoveX(rightDoorOpenPosition.x, openCloseDuration);
            leftDoor.transform.DOLocalMoveX(leftDoorOpenPosition.x, openCloseDuration);
        }

        public void CloseDoor()
        {
            // DoTween
            doorCloseFeedbacks?.PlayFeedbacks();
            rightDoor.transform.DOMove(rightDoorClosedPosition, openCloseDuration);
            leftDoor.transform.DOMove(leftDoorClosedPosition, openCloseDuration);
        }
    }
}
