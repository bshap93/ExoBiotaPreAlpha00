using System;
using Unity.Cinemachine;
using UnityEngine;

namespace Dirigible
{
    public class DockCameraPan : MonoBehaviour
    {
        [Tooltip("Main overview camera that follows the docked dirigible")]
        public CinemachineCamera overviewCam;

        [Tooltip("Anchor directly outside the mine door")]
        public Transform mineDoorAnchor;

        [Tooltip("Pan duration in seconds")] public float blendTime = 1.2f;

        public static DockCameraPan Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        public void PanFromMineDoorToOverview()
        {
            throw new NotImplementedException();
        }
    }
}