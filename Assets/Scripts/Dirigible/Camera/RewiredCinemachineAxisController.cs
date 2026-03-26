using Unity.Cinemachine;
using UnityEngine;

namespace Dirigible.Camera
{
    [AddComponentMenu("Cinemachine/Helpers/Rewired Cinemachine Axis Controller")]
    public class RewiredCinemachineAxisController : InputAxisControllerBase<RewiredAxisReader>
    {
        private void Update()
        {
            if (Application.isPlaying)
                UpdateControllers(); // inherited helper
        }

        /// Optional: set sensible defaults the first time axes are discovered
        protected override void InitializeControllerDefaultsForAxis(
            in IInputAxisOwner.AxisDescriptor axis, Controller c)
        {
            // Example: map axis names to Rewired actions automatically
            if (axis.Hint == IInputAxisOwner.AxisDescriptor.Hints.X)
                c.Input.SetAction("Look X");
            else if (axis.Hint == IInputAxisOwner.AxisDescriptor.Hints.Y)
                c.Input.SetAction("Look Y");
            // else if (axis.Hint == IInputAxisOwner.AxisDescriptor.Hints.Default)
            //     c.Input.SetAction("Zoom");
        }
    }
}