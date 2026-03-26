using System;
using System.Collections;
using DG.Tweening;
using Helpers.Events;
using MoreMountains.Feedbacks;
using MoreMountains.Tools;
using UnityEngine;
using Utilities.Interface;

namespace NewScript
{
    public class EmergingObstacle : MonoBehaviour, IRequiresUniqueID, MMEventListener<SpontaneousTriggerEvent>
    {
        [SerializeField] string emergeEventID;
        [SerializeField] GameObject childObject;
        [SerializeField] MMFeedbacks emergeFeedbacks;
        [SerializeField] Transform initialPosition;
        [SerializeField] float duration = 1f;
        [SerializeField] bool shouldEmerge;

        bool _emerged;

        void Start()
        {
            if (initialPosition != null && shouldEmerge)
            {
                childObject.transform.position = initialPosition.position;
                childObject.transform.rotation = initialPosition.rotation;
            }
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }

        void OnTriggerEnter(Collider other)
        {
            if (!shouldEmerge) return;
            if (other.CompareTag("Player") || other.CompareTag("FirstPersonPlayer"))
                StartCoroutine(Emerge());
        }
        public string UniqueID => emergeEventID;
        public void SetUniqueID()
        {
            emergeEventID = Guid.NewGuid().ToString();
        }
        public bool IsUniqueIDEmpty()
        {
            return string.IsNullOrEmpty(emergeEventID);
        }
        public void OnMMEvent(SpontaneousTriggerEvent eventType)
        {
        }

        IEnumerator Emerge()
        {
            if (_emerged) yield return null;
            _emerged = true;
            emergeFeedbacks?.PlayFeedbacks();
            if (childObject != null)
            {
                childObject.transform.DOLocalMove(Vector3.zero, duration).SetEase(Ease.InExpo);

                yield return new WaitForSeconds(duration);
            }
        }
    }
}
