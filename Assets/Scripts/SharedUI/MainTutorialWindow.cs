using Helpers.Events;
using Helpers.Events.Tutorial;
using Helpers.ScriptableObjects.Tutorial;
using Michsky.MUIP;
using MoreMountains.Tools;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SharedUI
{
    public class MainTutorialWindow : MonoBehaviour, MMEventListener<MainTutorialBitEvent>
    {
        public enum TutBitStyle
        {
            Brief,
            OnePart,
            TwoPart
        }

        [SerializeField] CanvasGroup canvasGroup;
        [Header("Tutorial Bit Fields")] [SerializeField]
        TMP_Text tutorialBitName;
        [SerializeField] TMP_Text subheader;
        [SerializeField] TMP_Text nameInterglot;
        [SerializeField] TMP_Text img1Caption;
        [SerializeField] TMP_Text img2Caption;
        [SerializeField] TMP_Text img3Caption;
        [SerializeField] Image img1Image;
        [SerializeField] Image img2Image;
        [SerializeField] Image img3Image;
        [SerializeField] TMP_Text paragraph1;
        [SerializeField] TMP_Text paragraph2;
        [SerializeField] ButtonManager closeButton;
        MainTutBitWindowArgs _mainFieldsObject;

        void Start()
        {
            closeButton.onClick.AddListener(() => Close(_mainFieldsObject.mainTutID));
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(MainTutorialBitEvent bitEventType)
        {
            if (bitEventType.BitEventType == MainTutorialBitEventType.ShowMainTutBit)
            {
                // if (!TutorialManager.Instance.AreTutorialsEnabled()) return;
                _mainFieldsObject = Resources.Load<MainTutBitWindowArgs>($"MainTutBits/{bitEventType.MainTutID}");

                Open(_mainFieldsObject);
            }
        }


        void Close(string id)
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            // MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
            ResetFields();
            MainTutorialBitEvent.Trigger(id, MainTutorialBitEventType.FinishTutBit);
            // MyUIEvent.Trigger(UIType.TutorialWindow, UIActionType.Close);
        }

        public void Open(MainTutBitWindowArgs mainTutBitWindowArgs)
        {
            SetTutFields(mainTutBitWindowArgs);
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            MyUIEvent.Trigger(UIType.Any, UIActionType.Open);
        }

        void SetTutFields(MainTutBitWindowArgs mainTutBitWindowArgs)
        {
            tutorialBitName.text = mainTutBitWindowArgs.tutBitName;
            subheader.text = mainTutBitWindowArgs.subheader;
            nameInterglot.text = mainTutBitWindowArgs.nameInterglot;
            img1Caption.text = mainTutBitWindowArgs.img1Caption;
            img2Caption.text = mainTutBitWindowArgs.img2Caption;
            img1Image.sprite = mainTutBitWindowArgs.img1Image;
            img2Image.sprite = mainTutBitWindowArgs.img2Image;
            paragraph1.text = mainTutBitWindowArgs.paragraph1;
            paragraph2.text = mainTutBitWindowArgs.paragraph2;
            img3Caption.text = mainTutBitWindowArgs.img3Caption;
            img3Image.sprite = mainTutBitWindowArgs.img3Image;
        }

        void ResetFields()
        {
            tutorialBitName.text = "";
            subheader.text = "";
            nameInterglot.text = "";
            img1Caption.text = "";
            img2Caption.text = "";
            img1Image.sprite = null;
            img2Image.sprite = null;
            paragraph1.text = "";
            paragraph2.text = "";
            img3Caption.text = "";
            img3Image.sprite = null;
            _mainFieldsObject = null;
        }
    }
}
