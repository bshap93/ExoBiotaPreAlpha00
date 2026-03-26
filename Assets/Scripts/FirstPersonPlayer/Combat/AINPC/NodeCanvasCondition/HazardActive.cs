using NodeCanvas.Framework;
using ParadoxNotion.Design;
using UnityEngine;

namespace FirstPersonPlayer.Combat.AINPC.NodeCanvasCondition
{
    // Check if player is within detection range
    [Category("BioOrganism")]
    [Description("Checks if player is within detection radius")]
    public class CheckPlayerInRange : ConditionTask
    {
        public BBParameter<float> detectionRadius;
        public BBParameter<Transform> player;

        protected override bool OnCheck()
        {
            if (player.value == null) return false;

            var distance = Vector3.Distance(agent.transform.position, player.value.position);
            return distance <= detectionRadius.value;
        }
    }

    // Check if gas has already been released
    [Category("BioOrganism")]
    [Description("Checks if gas has already been released")]
    public class CheckGasReleased : ConditionTask
    {
        [Tooltip("If true, checks if gas has NOT been released")]
        readonly bool _myInvert = false;
        public BBParameter<bool> GasReleased;

        protected override bool OnCheck()
        {
            return _myInvert ? !GasReleased.value : GasReleased.value;
        }
    }

    // Check if hazard is currently active
    [Category("BioOrganism")]
    [Description("Checks if the gas hazard is currently active (smoke is alive)")]
    public class CheckHazardActive : ConditionTask
    {
        public BBParameter<bool> HazardActive;

        [Tooltip("If true, checks if hazard is NOT active")]
        public bool Invert = false;

        protected override bool OnCheck()
        {
            return Invert ? !HazardActive.value : HazardActive.value;
        }
    }

    // Check if can release multiple times
    [Category("BioOrganism")]
    [Description("Checks if this organism can release gas multiple times")]
    public class CheckCanReleaseMultipleTimes : ConditionTask
    {
        public BBParameter<bool> canReleaseMultipleTimes;

        protected override bool OnCheck()
        {
            return canReleaseMultipleTimes.value;
        }
    }
}
