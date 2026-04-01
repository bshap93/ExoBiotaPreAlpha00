using UnityEngine;

namespace SharedUI.BaseElement
{
    public abstract class SelectionListElementNavigable<T> : MonoBehaviour where T : ScriptableObject
    {
        protected T ObjectData;

        public abstract void Select();

        public abstract void Deselect();

        public abstract void Initialize(T data);
    }
}
