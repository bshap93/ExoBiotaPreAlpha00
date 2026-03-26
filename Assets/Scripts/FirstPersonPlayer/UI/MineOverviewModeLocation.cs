using System;
using FirstPersonPlayer.Tools.ItemObjectTypes;
using FirstPersonPlayer.UI.LocationButtonBase;
using Helpers.Events;
using Helpers.Events.UI;
using Inventory;
using Structs;
using UnityEngine;

namespace FirstPersonPlayer.UI
{
    public class MineOverviewModeLocation : OverviewModeLocationButtons
    {
        public string spawnPointId;

        public string sceneName;

        KeyItemObject _keyItem;

        // Start is called once before the first execution of Update after the MonoBehaviour is created

        public void Initialize(string spawnPointIdVar, string sceneNameVar, object mineName,
            KeyItemObject keyItem) //= null)
        {
            spawnPointId = spawnPointIdVar;
            sceneName = sceneNameVar;
            locationText.text = mineName.ToString();
            _keyItem = keyItem;
        }

        public override void Interact()
        {
            if (string.IsNullOrEmpty(spawnPointId) || string.IsNullOrEmpty(sceneName))
            {
                Debug.LogError("MineOverviewModeLocation missing spawnPointId/sceneName");
                return;
            }

            if (_keyItem == null)
            {
                SceneTransitionUIEvent.Trigger(SceneTransitionUIEventType.Show);

                SpawnEvent.Trigger(SpawnEventType.ToMine, sceneName, GameMode.FirstPerson, spawnPointId);
            }
            else
            {
                if (GlobalInventoryManager.Instance.HasKeyForDoor(_keyItem.KeyID))
                {
                    // lockedDoor.isLocked = false;
                    // Debug.Log(lockedDoor.keyID);
                    SceneTransitionUIEvent.Trigger(SceneTransitionUIEventType.Show);

                    SpawnEvent.Trigger(SpawnEventType.ToMine, sceneName, GameMode.FirstPerson, spawnPointId);
                }
                else
                {
                    AlertEvent.Trigger(
                        AlertReason.DoorLocked,
                        "The mine entrance is locked. You need a key to enter.");
                }
            }
        }

        public override void ShowCanvasGroup()
        {
            throw new NotImplementedException();
        }

        public override void HideCanvasGroup()
        {
            throw new NotImplementedException();
        }
    }
}
