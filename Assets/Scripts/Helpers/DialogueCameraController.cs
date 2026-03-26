using Helpers.Events.NPCs;
using MoreMountains.Tools;
using Unity.Cinemachine;
using UnityEngine;

namespace Helpers
{
    /// <summary>
    ///     Attach to the same GameObject as (or alongside) your main brain camera rig.
    ///     Requires a second CinemachineCamera ("Dialogue Camera") wired up in the inspector.
    ///     When a DialogueCameraEvent.FocusOnTarget fires, that camera's priority is raised
    ///     above the gameplay camera so Cinemachine blends to it automatically.
    ///     Cinemachine 3.x priority: higher int = wins. Gameplay camera should sit at ~10.
    ///     Set dialogueCameraPriority to something like 20.
    /// </summary>
    public class DialogueCameraController : MonoBehaviour,
        MMEventListener<DialogueCameraEvent>
    {
        [Header("Cinemachine")]
        [Tooltip(
            "A dedicated CinemachineCamera used only during dialogue. " +
            "Configure its Body/Aim settings for the framing you want.")]
        [SerializeField]
        CinemachineCamera dialogueCamera;

        [Tooltip(
            "Priority given to the dialogue camera when active. " +
            "Must be higher than your gameplay camera's priority (usually 10).")]
        [SerializeField]
        int activePriority = 20;

        [Tooltip("Priority when idle — should be lower than gameplay camera.")] [SerializeField]
        int inactivePriority;

        [Header("Framing")]
        [Tooltip(
            "Optional world-space offset applied on top of the focus target position. " +
            "Useful to nudge framing up toward a head if your focus point is at chest height.")]
        [SerializeField]
        Vector3 lookAtOffset = new(0f, 0.15f, 0f);

        // A small proxy transform we reposition each frame so we can apply the offset
        // without modifying the NPC's actual transform.
        Transform _lookAtProxy;

        void Awake()
        {
            // Create a hidden proxy transform for offsetting LookAt
            var proxy = new GameObject("[DialogueCamera_LookAtProxy]") { hideFlags = HideFlags.HideInHierarchy };
            DontDestroyOnLoad(proxy);
            _lookAtProxy = proxy.transform;

            // Ensure dialogue camera starts inactive
            if (dialogueCamera != null)
                SetDialogueCameraActive(false, null, null);
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }
        void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(DialogueCameraEvent e)
        {
            switch (e.Type)
            {
                case DialogueCameraEventType.FocusOnTarget:
                    SetDialogueCameraActive(true, e.FocusTarget, e.FollowTarget);
                    break;

                case DialogueCameraEventType.ReleaseFocus:
                    SetDialogueCameraActive(false, null, null);
                    break;
            }
        }

        void SetDialogueCameraActive(bool active, Transform focusTarget, Transform followTarget)
        {
            if (dialogueCamera == null)
            {
                Debug.LogWarning("DialogueCameraController: no dialogueCamera assigned.");
                return;
            }

            if (active && focusTarget != null)
            {
                // Snap proxy to target + offset so the camera frames correctly
                _lookAtProxy.position = focusTarget.position + lookAtOffset;

                // Wire up the Cinemachine targets
                dialogueCamera.LookAt = _lookAtProxy;
                dialogueCamera.Follow = followTarget != null ? followTarget : dialogueCamera.Follow;

                // Raise priority → Cinemachine blends in automatically
                dialogueCamera.Priority = new PrioritySettings
                {
                    Value = activePriority,
                    Enabled = true
                };
            }
            else
            {
                // Drop priority → Cinemachine blends back to gameplay camera
                dialogueCamera.Priority = new PrioritySettings
                {
                    Value = inactivePriority,
                    Enabled = true
                };

                dialogueCamera.LookAt = null;
            }
        }

        /// <summary>
        ///     If your focus point moves during dialogue (e.g. the NPC has idle animation),
        ///     call this from Update or subscribe to an animation event to keep the proxy in sync.
        /// </summary>
        public void UpdateProxyPosition(Transform focusTarget)
        {
            if (focusTarget != null && _lookAtProxy != null)
                _lookAtProxy.position = focusTarget.position + lookAtOffset;
        }
    }
}
