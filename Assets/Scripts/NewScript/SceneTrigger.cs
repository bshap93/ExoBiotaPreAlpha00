using Helpers.Events;
using UnityEngine;

public class SceneTrigger : MonoBehaviour
{
    public enum TriggerType
    {
        Quit,
        MainTitle
    }

    public TriggerType triggerType;

    public void Trigger()
    {
        switch (triggerType)
        {
            case TriggerType.Quit:
                SceneEvent.Trigger(SceneEventType.PlayerRequestsQuit);
                break;
            case TriggerType.MainTitle:
                SceneEvent.Trigger(SceneEventType.PlayerRequestsMainMenu);
                break;
        }
    }
}
