using System;

namespace PhysicsHandlers.Triggers
{
    [Serializable]
    public enum TriggerType
    {
        OnEnter,
        OnExit,
        Both,
        Script
    }

    [Serializable]
    public enum TriggerColliderType
    {
        Spontaneous,
        Tutorial,
        Objective,
        Dialogue
    }
}
