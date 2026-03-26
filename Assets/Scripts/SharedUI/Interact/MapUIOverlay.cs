using Helpers.Events.UI;
using MoreMountains.Tools;
using UnityEngine;

namespace SharedUI.Interact
{
    public class MapUfIOverlay : MonoBehaviour, MMEventListener<MapEvent>
    {
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        public void OnMMEvent(MapEvent eventType)
        {
            throw new System.NotImplementedException();
        }
    }
}
