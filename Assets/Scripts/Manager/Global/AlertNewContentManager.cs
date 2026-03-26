using System;
using Helpers.Events;
using Helpers.Interfaces;
using MoreMountains.Tools;
using UnityEngine;

namespace Manager.Global
{
    [Serializable]
    public class AlertNewContent
    {
        public string LocationId;
        public string DockId;
    }

    public class AlertNewContentManager : MonoBehaviour, ICoreGameService, MMEventListener<AlertNewContentEvent>
    {
        #region Event Handling

        public void OnMMEvent(AlertNewContentEvent eventType)
        {
            Debug.Log(
                $"AlertNewContentManager received event: {eventType.Type} at Location: {eventType.LocationId}, Dock: {eventType.DockId}");
            // Handle the event here
        }

        #endregion

        #region Lifecylcle

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
        }

        void OnEnable()
        {
            this.MMEventStartListening();
        }

        void OnDisable()
        {
            this.MMEventStopListening();
        }

        #endregion


        #region IGameService

        public void Save()
        {
            throw new NotImplementedException();
        }

        public void Load()
        {
            throw new NotImplementedException();
        }

        public void Reset()
        {
            throw new NotImplementedException();
        }

        public void ConditionalSave()
        {
            throw new NotImplementedException();
        }

        public void MarkDirty()
        {
            throw new NotImplementedException();
        }

        public string GetSaveFilePath()
        {
            throw new NotImplementedException();
        }

        public void CommitCheckpointSave()
        {
            throw new NotImplementedException();
        }

        public bool HasSavedData()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
