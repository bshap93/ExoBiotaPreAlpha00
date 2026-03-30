using FirstPersonPlayer.Interface;
using FirstPersonPlayer.Tools;
using FirstPersonPlayer.Tools.Interface;
using Helpers.Events.NPCs;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.UI;

// [WIP] Placeholder for revised First Person Interaction-Inventory system	

namespace FirstPersonPlayer.UI
{
    public class ReticleController : MonoBehaviour, MMEventListener<DialogueCameraEvent>
    {
        [Header("Reticle States")] public ReticleState defaultState;

        public ReticleState interactableState;
        public ReticleState mineableState;
        public ReticleState switchToolState;
        public ReticleState validTerrainState;
        public ReticleState scannerState;
        public ReticleState scannerNotCalibratedState;
        public ReticleState IRuntimeToolUseableState;
        public ReticleState IRuntimeToolUnuseableState; // Add this for red/inability reticle


        [Header("Reticle UI")] public Image reticle;

        [SerializeField] CanvasGroup reticleCanvasGroup;

        ReticleState currentState;
        IRuntimeTool currentTool;
        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }
        public void OnMMEvent(DialogueCameraEvent eventType)
        {
            if (eventType.Type == DialogueCameraEventType.FocusOnTarget)
                HideReticle();
            else if (eventType.Type == DialogueCameraEventType.ReleaseFocus) ShowReticle();
        }


        public void UpdateReticle(RaycastHit? hit, bool terrainBlocking)
        {
            var targetState = defaultState;

            var activePE = PlayerEquipment.GetWithActiveToolOrRight();

            currentTool = activePE?.CurrentRuntimeTool;


            if (hit.HasValue)
            {
                var valueCollider = hit.Value.collider;

                // Priority 1: Interactables
                if (!terrainBlocking)
                {
                    if (valueCollider.CompareTag("DiggerChunk")) return;

                    if (currentTool != null && currentTool.CanInteractWithObject(valueCollider.gameObject))
                    {
                        IRuntimeToolUseableState.reticleSprite =
                            currentTool.GetReticleForTool(valueCollider.gameObject);

                        targetState = IRuntimeToolUseableState;
                        ApplyReticleState(targetState);
                        return;
                    }

                    var interactable = valueCollider.GetComponent<IInteractable>();
                    if (interactable != null)
                    {
                        targetState = interactableState;
                        ApplyReticleState(targetState);
                        return;
                    }

                    if (currentTool != null && !currentTool.CanInteractWithObject(valueCollider.gameObject))
                    {
                        IRuntimeToolUnuseableState.reticleSprite =
                            currentTool.GetReticleForTool(valueCollider.gameObject);

                        targetState = IRuntimeToolUnuseableState;
                        ApplyReticleState(targetState);
                        return;
                    }
                }
            }

            ApplyReticleState(targetState);
        }


        void ApplyReticleState(ReticleState state)
        {
            if (state != currentState)
            {
                currentState = state;
                if (state != null)
                {
                    reticle.sprite = state.reticleSprite;
                    reticle.color = state.reticleColor;
                }
                else
                {
                    reticle.sprite = defaultState.reticleSprite;
                    reticle.color = defaultState.reticleColor;
                }
            }
        }

        void HideReticle()
        {
            reticleCanvasGroup.alpha = 0f;
        }

        void ShowReticle()
        {
            reticleCanvasGroup.alpha = 1f;
        }
    }
}
