using System.Collections;
using FirstPersonPlayer.Interactable.HoloInteractable;
using Helpers.Events.Journal;
using Helpers.Events.NPCs;
using Manager.StateManager;
using Sirenix.OdinInspector;
using UnityEngine;

namespace FirstPersonPlayer.Interactable
{
    public class EnvironmentalJournalEntryProvider : DialogueInteractable
    {
        public bool destroyAfterProviding;
        public string textToRepresentEntry;
        public Sprite iconToRepresentEntry;


        // Move Cam 2
        public bool hasDialogueFocusPoint;
        [Header("Dialogue Camera")]
        [ShowIf("hasDialogueFocusPoint")]
        [Tooltip(
            "Transform the dialogue camera will look at during conversation. " +
            "Drag a child bone/empty here (e.g. head or chest). " +
            "If left null, the NPC's root transform is used as a fallback.")]
        [SerializeField]
        Transform dialogueFocusPoint;

        public JournalEntryProviderManager.EntryProviderState initialEntryProviderInitialization;
        void Start()
        {
            StartCoroutine(InitializeAfterJournalEntryProviderManager());
        }

#if UNITY_EDITOR
        /// Draws a gizmo so you can visually confirm focus point placement in the editor.
        void OnDrawGizmosSelected()
        {
            var target = dialogueFocusPoint != null ? dialogueFocusPoint.position : transform.position;
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(target, 0.08f);
            Gizmos.DrawLine(transform.position, target);
        }
#endif

        IEnumerator InitializeAfterJournalEntryProviderManager()
        {
            yield return null;

            var journalEntryProviderManager = JournalEntryProviderManager.Instance;

            if (journalEntryProviderManager != null)
            {
                var journalProviderState = journalEntryProviderManager.GetJournalProviderState(uniqueID);
                if (journalProviderState == JournalEntryProviderManager.EntryProviderState.None)
                    journalProviderState = initialEntryProviderInitialization;

                if (journalProviderState == JournalEntryProviderManager.EntryProviderState.ShouldBeDestroyed)
                {
                    Destroy(gameObject);
                    yield break;
                }
            }
        }
        public override string GetName()
        {
            return textToRepresentEntry;
        }
        public override Sprite GetIcon()
        {
            return iconToRepresentEntry;
        }

        public void TriggerJournalEntryProviderStateEvent(int newState)
        {
            JournalEntryProviderStateEvent.Trigger(
                JournalEntryProviderStateEventType.SetNewJournalEntryProviderState, uniqueID,
                (JournalEntryProviderManager.EntryProviderState)newState);

            Destroy(gameObject);
        }

        public override void Interact()
        {
            base.Interact();
            if (hasDialogueFocusPoint)
            {
                var focusTarget = dialogueFocusPoint != null ? dialogueFocusPoint : transform;
                DialogueCameraEvent.Trigger(DialogueCameraEventType.FocusOnTarget, focusTarget);
            }
        }

        public override void OnInteractionEnd(string param)
        {
            DialogueCameraEvent.Trigger(DialogueCameraEventType.ReleaseFocus);
        }
    }
}
