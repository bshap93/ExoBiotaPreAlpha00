using System.Collections;
using Structs;
using UnityEngine;

namespace ModeControllers
{
    public abstract class ModeController : MonoBehaviour
    {
        public GameMode Mode;
        public abstract IEnumerator Attach();


        public abstract void Detach();
    }
}
