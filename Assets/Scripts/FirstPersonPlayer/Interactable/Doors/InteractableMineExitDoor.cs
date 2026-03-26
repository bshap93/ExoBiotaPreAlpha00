using EditorScripts;
using Helpers.Events;
using Helpers.Events.UI;
using LevelConstruct;
using Manager;
using MoreMountains.Feedbacks;
using Objectives.ScriptableObjects;
using SharedUI.Interface;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace FirstPersonPlayer.Interactable.Doors
{
    public class InteractableMineExitDoor : InteractableDoor
    {
        [SerializeField] MMFeedbacks denyEntryFeedbacks;
        [SerializeField] SpawnInfoEditor spawnInfo;
        [SerializeField] ObjectiveObject objectiveIfActiveToComplete;
        [SerializeField] bool inaccessible = true;
        protected override bool OnHoverStart(GameObject obj)
        {
            var nameToShow = GetName();
            var iconToShow = GetIcon();
            var shortToShow = ShortBlurb();
            var icon = GetActionIcon();

            data = new SceneObjectData(nameToShow, iconToShow, shortToShow, icon, "Use Door");
            data.Id = uniqueId;

            BillboardEvent.Trigger(data, BillboardEventType.Show);
            if (actionId != 0 && !inaccessible)
                ControlsHelpEvent.Trigger(
                    ControlHelpEventType.Show, actionId
                );

            return true; // return false to cancel hover highlight
        }

        public override void Interact()
        {
            if (!TryOpenWithAccess()) return;
            BillboardEvent.Trigger(data, BillboardEventType.Hide);
            AlertEvent.Trigger(
                AlertReason.UseDoor,
                "Exit the mine and return to the dirigible?", "Use Door", AlertType.ChoiceModal, 0f,
                onConfirm: () =>
                {
                    MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
                    SceneTransitionUIEvent.Trigger(SceneTransitionUIEventType.Show);
                    SaveDataEvent.Trigger();

                    // Set the bridge target before loading the bridge scene
                    BridgeData.SetTarget(
                        spawnInfo.SceneName,
                        spawnInfo.Mode,
                        spawnInfo.SpawnPointId
                    );

                    // Load the universal Bridge scene
                    SceneManager.LoadScene("Bridge");

                    if (objectiveIfActiveToComplete != null)
                        ObjectiveEvent.Trigger(
                            objectiveIfActiveToComplete.objectiveId, ObjectiveEventType.ObjectiveCompleted);
                },
                onCancel: () => { });
        }

        public override string GetName()
        {
            if (inaccessible) return "Inaccessible";

            return "To " + spawnInfo.SceneName;
        }

        public override Sprite GetIcon()
        {
            return ExaminationManager.Instance.iconRepository.dockIcon;
        }

        public override string ShortBlurb()
        {
            return string.Empty;
        }

        public override Sprite GetActionIcon()
        {
            return ExaminationManager.Instance.iconRepository.doorIcon;
        }

        public override string GetActionText()
        {
            return "Enter";
        }
    }
}
