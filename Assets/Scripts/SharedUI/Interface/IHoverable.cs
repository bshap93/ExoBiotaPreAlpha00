using UnityEngine;

namespace SharedUI.Interface
{
    public interface IHoverable
    {
        public bool OnHoverStart(GameObject go);
        public bool OnHoverStay(GameObject go);
        public bool OnHoverEnd(GameObject go);
    }
}