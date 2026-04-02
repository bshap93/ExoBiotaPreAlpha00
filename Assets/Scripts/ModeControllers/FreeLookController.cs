using System.Collections;
using Helpers.Events;
using Structs;
using UnityEngine;

namespace ModeControllers
{
    public class FreeLookController : ModeController
    {
        [Header("Movement")] [SerializeField] float moveSpeed = 10f;
        [SerializeField] float fastMultiplier = 3f; // hold Shift to boost
        [SerializeField] float mouseSensitivity = 2f;
        [SerializeField] float scrollSpeed = 5f;

        [Header("Behaviour")] [SerializeField] bool hideUIOnEnter = true;

        float _yaw, _pitch;

        void Awake()
        {
            Mode = GameMode.FreeLook;
        }

        void Update()
        {
            HandleLook();
            HandleMove();
        }

        public override IEnumerator Attach()
        {
            // Unlock and hide cursor for fly-cam feel
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            // Inherit current world rotation so the camera doesn't snap
            var euler = transform.eulerAngles;
            _yaw = euler.y;
            _pitch = euler.x;

            if (hideUIOnEnter)
                // fire your existing UI close event here if you have one
                MyUIEvent.Trigger(UIType.Any, UIActionType.Close);

            yield break;
        }

        public override void Detach()
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        void HandleLook()
        {
            _yaw += Input.GetAxis("Mouse X") * mouseSensitivity;
            _pitch -= Input.GetAxis("Mouse Y") * mouseSensitivity;
            _pitch = Mathf.Clamp(_pitch, -89f, 89f);
            transform.rotation = Quaternion.Euler(_pitch, _yaw, 0f);
        }

        void HandleMove()
        {
            var speed = moveSpeed;
            if (Input.GetKey(KeyCode.LeftShift)) speed *= fastMultiplier;

            // Scroll wheel for speed adjustment on the fly
            speed += Input.GetAxis("Mouse ScrollWheel") * scrollSpeed;

            var dir = new Vector3(
                Input.GetAxis("Horizontal"),
                0f,
                Input.GetAxis("Vertical"));

            // Q/E for vertical
            if (Input.GetKey(KeyCode.E)) dir.y = 1f;
            if (Input.GetKey(KeyCode.Q)) dir.y = -1f;

            transform.position += transform.TransformDirection(dir) * (speed * Time.deltaTime);
        }
    }
}
