using Helpers.Events;
using Helpers.Events.Tutorial;
using MoreMountains.Tools;
using SharedUI.Interface;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI.Billboard
{
    public class BillboardUI : MonoBehaviour, MMEventListener<BillboardEvent>, MMEventListener<MainTutorialBitEvent>
    {
        public enum BillboardState
        {
            Hidden,
            Visible
        }

        [SerializeField] Image mainIcon;
        [SerializeField] TextMeshProUGUI nameText;
        [SerializeField] TextMeshProUGUI blurbText;

        BillboardState _billboardState;

        Camera _cam;

        CanvasGroup _canvasGroup;

        void Awake()
        {
            _cam = Camera.main;
        }

        void Start()
        {
            _canvasGroup = GetComponent<CanvasGroup>();
            if (_canvasGroup == null) _canvasGroup = gameObject.AddComponent<CanvasGroup>();

            _billboardState = BillboardState.Hidden;

            _canvasGroup.alpha = 0f;
            _canvasGroup.interactable = false;
            _canvasGroup.blocksRaycasts = false;
        }


        void OnEnable()
        {
            this.MMEventStartListening<BillboardEvent>();
            this.MMEventStartListening<MainTutorialBitEvent>();
        }

        void OnDisable()
        {
            this.MMEventStopListening<BillboardEvent>();
            this.MMEventStopListening<MainTutorialBitEvent>();
        }

        public void OnMMEvent(BillboardEvent eventType)
        {
            if (eventType.EventType == BillboardEventType.Show)
            {
                Init(eventType.SceneObjectData);

                _billboardState = BillboardState.Visible;
                _canvasGroup.alpha = 1f;
                _canvasGroup.interactable = true;
                _canvasGroup.blocksRaycasts = true;
            }

            if (eventType.EventType == BillboardEventType.Hide)
            {
                Init(SceneObjectData.Empty());

                _billboardState = BillboardState.Hidden;
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }

            if (eventType.EventType == BillboardEventType.Update) Init(eventType.SceneObjectData);
        }
        public void OnMMEvent(MainTutorialBitEvent bitEventType)
        {
            if (bitEventType.BitEventType == MainTutorialBitEventType.ShowMainTutBit)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.interactable = false;
                _canvasGroup.blocksRaycasts = false;
            }
            else
            {
                if (_billboardState == BillboardState.Visible)
                {
                    _canvasGroup.alpha = 1f;
                    _canvasGroup.interactable = true;
                    _canvasGroup.blocksRaycasts = true;
                }
                else if (_billboardState == BillboardState.Hidden)
                {
                    _canvasGroup.alpha = 0f;
                    _canvasGroup.interactable = false;
                    _canvasGroup.blocksRaycasts = false;
                }
            }
        }

        public void Init(SceneObjectData source)
        {
            if (source == null)
            {
                Debug.LogError("BillboardableData source is null");
                return;
            }

            mainIcon.sprite = source.Icon;
            nameText.text = source.Name;
            blurbText.text = source.ShortBlurb;
            // actionIcon.sprite = source.ActionIcon;
            // actionText.text = source.ActionText;

            gameObject.SetActive(true);
        }
    }
}
