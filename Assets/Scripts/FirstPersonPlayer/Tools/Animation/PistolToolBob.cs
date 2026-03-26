using UnityEngine;

namespace FirstPersonPlayer.Tools.Animation
{
    public class PistolToolBob : ToolBob
    {
        [SerializeField] float timeOffset = 0.5f;
        [SerializeField] bool randomizeOffset = true;

        float _timeOffset;

        public override void Initialize()
        {
            base.Initialize();

            // Randomize the time offset to ensure different tools never sync
            _timeOffset = randomizeOffset ? Random.Range(0f, Mathf.PI * 2f) : timeOffset;
        }

        // Override the time calculation to add offset
        protected override float GetBobTime()
        {
            return Time.time + _timeOffset;
        }
    }
}
