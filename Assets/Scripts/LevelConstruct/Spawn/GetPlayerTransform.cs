using Manager.Global;
using NodeCanvas.Framework;
using UnityEngine;

namespace LevelConstruct.Spawn
{
    public class GetPlayerTransform : ActionTask
    {
        public BBParameter<Transform> storePlayer;
        protected override void OnExecute()
        {
            if (GameStateManager.Instance == null 
                || GameStateManager.Instance.PlayerRoot == null) 
            {
                Debug.LogWarning("PlayerRoot is not yet registered.");
                EndAction(false);
                return;
            }

            storePlayer.value = GameStateManager.Instance.PlayerRoot;
            EndAction(true);
        }
    }
}
