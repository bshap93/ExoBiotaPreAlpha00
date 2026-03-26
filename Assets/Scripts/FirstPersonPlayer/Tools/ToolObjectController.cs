using Domains.Gameplay.Mining.Events;
using MoreMountains.Tools;
using UnityEngine;
using UnityEngine.Events;

namespace FirstPersonPlayer.Tools
{
    public enum ToolType
    {
        Arms,
        Shovel,
        Pickaxe,
        Scanner,
        Jetpack,
        DemoGift,
        OtherTool
    }

    public enum ToolIteration
    {
        First,
        Second
    }

    public class ToolObjectController : MonoBehaviour, MMEventListener<ToolEvent>
    {
        public UnityEvent OnToolUseAction;


        public ToolType toolType;

        public ToolIteration toolIteration;


        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(ToolEvent eventType)
        {
            // if (eventType.ToolType == ToolType.Shovel)
            //     switch (eventType.ToolIteration)
            //     {
            //         case ToolIteration.First:
            //             OnToolUseAction.Invoke();
            //             break;
            //         case ToolIteration.Second:
            //             Debug.Log("Tool used with second smallest iteration");
            //             OnToolUseAction.Invoke();
            //             break;
            //     }
            // else if (eventType.ToolType == ToolType.Pickaxe)
            //     switch (eventType.ToolIteration)
            //     {
            //         case ToolIteration.First:
            //             Debug.Log("Tool used with first smallest iteration");
            //             OnToolUseAction.Invoke();
            //             break;
            //         case ToolIteration.Second:
            //             Debug.Log("Tool used with second smallest iteration");
            //             OnToolUseAction.Invoke();
            //             break;
            //     }
            // else if (eventType.ToolType == ToolType.Scanner)
            //     Debug.Log("Tool used with scanner");
        }


        public void OnToolUse()
        {
            Debug.Log("Tool used");
        }
    }
}