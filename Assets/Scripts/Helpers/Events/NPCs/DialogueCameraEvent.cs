using MoreMountains.Tools;
using UnityEngine;

namespace Helpers.Events.NPCs
{
    public enum DialogueCameraEventType
    {
        FocusOnTarget,
        ReleaseFocus,
        FadeOut,
        FadeIn
    }

    /// <summary>
    ///     Fired when dialogue starts/ends so the dialogue camera can track the NPC focus point.
    /// </summary>
    public struct DialogueCameraEvent
    {
        public DialogueCameraEventType Type;

        /// <summary>
        ///     The transform the dialogue camera should look at (e.g. NPC head/chest anchor).
        ///     Only meaningful when Type == FocusOnTarget.
        /// </summary>
        public Transform FocusTarget;

        /// <summary>
        ///     Optional: where the dialogue camera should be positioned/follow from.
        ///     If null, the camera stays in place and just rotates its LookAt.
        /// </summary>
        public Transform FollowTarget;

        static DialogueCameraEvent e;

        public static void Trigger(DialogueCameraEventType type,
            Transform focusTarget = null,
            Transform followTarget = null)
        {
            e.Type = type;
            e.FocusTarget = focusTarget;
            e.FollowTarget = followTarget;
            MMEventManager.TriggerEvent(e);
        }
    }
}
