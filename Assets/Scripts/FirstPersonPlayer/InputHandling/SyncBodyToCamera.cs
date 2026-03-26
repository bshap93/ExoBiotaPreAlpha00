using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

namespace FirstPersonPlayer.InputHandling
{
    public class SyncBodyToCamera : MonoBehaviour
    {
        [SerializeField] private CharacterActor actor;
        [SerializeField] private Transform cam;

        private void LateUpdate()
        {
            var dir = Vector3.ProjectOnPlane(cam.forward, actor.Up);
            if (dir.sqrMagnitude > 0.0001f)
                actor.SetYaw(dir.normalized);
        }
    }
}