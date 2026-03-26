using System.Collections.Generic;
using Helpers.Events;
using Manager;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.HUD.InGameTime
{
    public class TimeGUIController : MonoBehaviour, MMEventListener<InGameTimeUpdateEvent>
    {
        [SerializeField] TMP_Text minutesIntoDayText;
        [SerializeField] TMP_Text dayNumberText;
        // [SerializeField] RadialSlider orbitalPeriodSlider;
        [SerializeField] Image sliderImage;
        [SerializeField] GameObject calendarBallImg;

        [SerializeField] List<RectTransform> calendarBallPositions;

        [SerializeField] int numberOfDayPositions = 8;

        // float _cooldownTimer = 0f;

        int _previousDayNumber = -1;

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }


        public void OnMMEvent(InGameTimeUpdateEvent updateEventType)
        {
            minutesIntoDayText.text = updateEventType.MinutesIntoDay.ToString();

            dayNumberText.text = updateEventType.DayNumber.ToString();
            sliderImage.fillAmount = updateEventType.DayNumber /
                                     (float)InGameTimeManager.Instance.orbitalPeriodInDays;

            var index = updateEventType.DayNumber / 2;

            calendarBallImg.transform.position =
                calendarBallPositions[index % numberOfDayPositions].position;


            _previousDayNumber = updateEventType.DayNumber;
        }
    }
}
