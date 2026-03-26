using Helpers.Events;
using MoreMountains.Tools;
using UnityEngine;

namespace Interactable.Train
{
    public class ModularTrainController : MonoBehaviour,
        MMEventListener<InventoryEvent>
    {
        TrainSegmentController trainSegment;

        // [OdinSerialize] private Queue<TrainSegmentController> trainSegments = new();

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }


        public void OnMMEvent(InventoryEvent eventType)
        {
            if (eventType.EventType == InventoryEventType.SellAllItems) SendOffHeadOfTrainQueue();
        }

        public void SendOffHeadOfTrainQueue()
        {
            // var trainSegment = trainSegments.Dequeue();
            StartCoroutine(trainSegment.SendOff());
        }

        // public void EnqueueTrainSegment(TrainSegmentController trainSegment)
        // {
        //     trainSegments.Enqueue(trainSegment);
        // }
    }
}
