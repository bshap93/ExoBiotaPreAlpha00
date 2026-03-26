using Manager.Global;
using UnityEngine;

namespace LevelConstruct.Spawn
{
    public class PlayerRoot : MonoBehaviour
    {
        private void Awake()
        {
            GameStateManager.Instance.RegisterPlayerRoot(transform);
        }
    }
}