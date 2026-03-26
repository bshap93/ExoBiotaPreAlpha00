using Helpers.Events;
using UnityEngine;

public class TriggerCloseUI : MonoBehaviour
{
    public void CloseUI()
    {
        MyUIEvent.Trigger(UIType.Any, UIActionType.Close);
    }
}