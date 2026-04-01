using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI.BaseElement
{
    public abstract class SelectionListViewNavigableWithActiveElement<TE>
        : MonoBehaviour, MMEventListener<TE> where TE : struct
    {
        public Transform listTransform;
        public GameObject listViewElementPrefab;

        public virtual void OnEnable()
        {
            this.MMEventStartListening();
        }
        public virtual void OnDisable()
        {
            this.MMEventStopListening();
        }

        public abstract void OnMMEvent(TE eventType);
        public abstract void Refresh();
    }
}
