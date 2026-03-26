using UnityEngine;
using Yarn.Unity;

namespace FirstPersonPlayer.UI.LocationButtonBase.Test
{
    public class VarProbeYSES3 : MonoBehaviour
    {
        public VariableStorageBehaviour storage;

        public void TryGet()
        {
            storage.TryGetValue<float>("$gold", out var gold);
            storage.TryGetValue<bool>("$met", out var met);
            storage.TryGetValue<string>("$name", out var name);
            Debug.Log($"gold={gold}, met={met}, name={name}");
        }
    }
}