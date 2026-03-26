// DeathMenuController.cs

using Michsky.MUIP;
using UnityEngine;
using UnityEngine.Serialization;

namespace SharedUI
{
    public class DeathMenuController : MonoBehaviour
    {
        [SerializeField] private ButtonManager respawnButton;
        [SerializeField] private ButtonManager quitButton;

        [FormerlySerializedAs("_bootLoader")] [SerializeField]
        private BootLoader bootLoader;


        private void Awake()
        {
            // unlock cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            bootLoader = FindFirstObjectByType<BootLoader>();
        }

        public void OnRespawnButtonPressed()
        {
            _ = bootLoader.ConductBootLoad();
        }
    }
}