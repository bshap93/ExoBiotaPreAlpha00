using EditorScripts;
using Helpers.Events.Spawn;
using LevelConstruct.Spawn;
using Manager;
using SharedUI.Interface;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.Interactable.HoloInteractable
{
    public class MapShowingDialogueInteractable : DialogueInteractable,
        IBillboardable
    {
        // [ValueDropdown("GetNpcIdOptions")] public
        //     string npcId;

        // [SerializeField] MMFeedbacks startDialogueFeedback;

        [SerializeField] SpawnPoint spawnPoint;

        // [SerializeField] string defaultStartNode = "NavigationServerSwitch";

        // [SerializeField] string nodeToUse;

        [SerializeField] Sprite mapSprite;

        // [SerializeField] string uniqueID;

        [SerializeField] Vector2 positionOnMap;

        [SerializeField] bool showPlayerPosition;
        [SerializeField] bool showAdditionalOverlay;

// #if UNITY_EDITOR
        // [ValueDropdown(nameof(GetAllRewiredActions))]
// #endif
        // public int actionId;

        [SerializeField] [InlineProperty] [HideLabel]
        SpawnInfoEditor overrideSpawnInfo;

        [ShowIf("showAdditionalOverlay")] [SerializeField]
        Sprite additionalOverlaySprite;
        SceneObjectData _sceneObjectData;
        public override string GetName()
        {
            return "Navigation Server";
        }
        public override Sprite GetIcon()
        {
            return PlayerUIManager.Instance.defaultIconRepository.navigationServerIcon;
        }


        public override void Interact()
        {
            base.Interact();


            SpawnAssignmentEvent.Trigger(
                SpawnAssignmentEventType.SetMostRecentSpawnPoint, overrideSpawnInfo.SceneName,
                overrideSpawnInfo.SpawnPointId);
        }
    }
}
