using System.Collections.Generic;
using AmbientSounds;
using Events;
using Helpers.Events;
using ModeControllers;
using MoreMountains.Tools;
using Rewired;
using SharedUI.Utilities;
using Structs;
using UnityEngine;

namespace Manager.Global
{
    public class GameStateManager : MonoBehaviour, MMEventListener<MyUIEvent>,
        MMEventListener<ModeLoadEvent>
    {
        const string OverviewModeAmbienceEvent = "OverviewMode";
        const string DirigibleModeAmbienceEvent = "DirigibleMode";
        const string FirstPersonModeAmbienceEvent = "FirstPersonMode";
        [SerializeField] List<GameObject> pawns;

        public string DirigibleCategoryName = "DirigibleFlight";
        public string FirstPersonCategoryName = "FirstPerson";
        public string OverviewCategoryName = "LocationOverview";
        public string DefaultCategoryName = "Default";
        public string InGameUICategoryName = "InGameUI";


        ModeController _current;

        string _currentGameplayCategory; // default at boot


        Dictionary<GameMode, GameObject> _prefabs;

        public Transform PlayerRoot { get; private set; }

        public GameMode CurrentMode => _current?.Mode ?? GameMode.None;

        public static GameStateManager Instance { get; private set; }


        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                // if (SaveManager.Instance.saveManagersDontDestroyOnLoad)
                //     DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }


            // 3. Build the lookup table once
            _prefabs = new Dictionary<GameMode, GameObject>
            {
                { GameMode.DirigibleFlight, pawns[0] },
                { GameMode.FirstPerson, pawns[1] },
                { GameMode.Overview, pawns[2] }
            };


            EnableUIMaps(false);
        }

        public void OnEnable()
        {
            this.MMEventStartListening<MyUIEvent>();
            this.MMEventStartListening<ModeLoadEvent>();
        }

        public void OnDisable()
        {
            this.MMEventStopListening<MyUIEvent>();
            this.MMEventStopListening<ModeLoadEvent>();
        }


        public void OnMMEvent(ModeLoadEvent eventType)
        {
            if (eventType.EventType == ModeLoadEventType.Enabled) SwitchTo(eventType.ModeName);
        }

        public void OnMMEvent(MyUIEvent eventType)
        {
            if (eventType.uiActionType == UIActionType.Open)
                EnableUIMaps(true);
            else if (eventType.uiActionType == UIActionType.Close)
                EnableUIMaps(false);
        }


        public void SwitchTo(GameMode mode)
        {
            // Prevent switching to the same mode
            // if (_current != null && _current.Mode == mode)
            // {
            //     Debug.Log($"Already in {mode} mode, ignoring switch request");
            //     return;
            // }

            // Enable input maps FIRST
            SwitchInput(mode);

            if (_current != null)
            {
                _current.Detach();
                Destroy(_current.gameObject);
            }

            switch (mode)
            {
                case GameMode.Overview:
                    _currentGameplayCategory = "LocationOverview";

                    break;
                case GameMode.DirigibleFlight:
                    _currentGameplayCategory = "DirigibleFlight";
                    break;
                case GameMode.FirstPerson:
                    _currentGameplayCategory = "FirstPerson";
                    break;
                default:
                    _currentGameplayCategory = "None";
                    break;
            }

            SwitchAmbienceEvent(mode);

            // Then instantiate the pawn
            var go = Instantiate(_prefabs[mode], PlayerRoot, false);
            go.transform.localPosition = Vector3.zero;
            go.transform.localRotation = Quaternion.identity;

            _current = go.GetComponent<ModeController>();
            StartCoroutine(_current.Attach());
        }
        static void SwitchAmbienceEvent(GameMode mode)
        {
            switch (mode)
            {
                case GameMode.Overview:
                    AmbienceManager.ActivateEvent(OverviewModeAmbienceEvent);
                    // AmbienceManager.DeactivateEvent(DirigibleModeAmbienceEvent);
                    // AmbienceManager.DeactivateEvent(FirstPersonModeAmbienceEvent);
                    break;
                case GameMode.DirigibleFlight:
                    // AmbienceManager.ActivateEvent(DirigibleModeAmbienceEvent);
                    AmbienceManager.DeactivateEvent(OverviewModeAmbienceEvent);
                    // AmbienceManager.DeactivateEvent(FirstPersonModeAmbienceEvent);
                    break;
                case GameMode.FirstPerson:
                    // AmbienceManager.ActivateEvent(FirstPersonModeAmbienceEvent);
                    // AmbienceManager.DeactivateEvent(DirigibleModeAmbienceEvent);
                    AmbienceManager.DeactivateEvent(OverviewModeAmbienceEvent);
                    break;
                case GameMode.None:
                    AmbienceManager.DeactivateEvent(OverviewModeAmbienceEvent);
                    // AmbienceManager.DeactivateEvent(DirigibleModeAmbienceEvent);
                    // AmbienceManager.DeactivateEvent(FirstPersonModeAmbienceEvent);
                    break;
            }
        }

        public void RegisterPlayerRoot(Transform t)
        {
            PlayerRoot = t;
        }


        public void SwitchInput(GameMode mode)
        {
            var player = ReInput.players.GetPlayer(0);

            // Disable all first
            player.controllers.maps.SetAllMapsEnabled(false);

            player.controllers.maps.SetMapsEnabled(true, "Default");

            // Then enable the one we need
            switch (mode)
            {
                case GameMode.DirigibleFlight: // dirigible
                    player.controllers.maps.SetMapsEnabled(true, DirigibleCategoryName);
                    break;

                case GameMode.FirstPerson: // on-foot
                    player.controllers.maps.SetMapsEnabled(true, FirstPersonCategoryName);
                    break;

                case GameMode.Overview: // settlement view
                    player.controllers.maps.SetMapsEnabled(true, OverviewCategoryName);
                    break;
            }
        }

        void EnableUIMaps(bool enable)
        {
            var p = ReInput.players.GetPlayer(0);

            if (enable)
            {
                p.controllers.maps.SetMapsEnabled(false, _currentGameplayCategory);
                p.controllers.maps.SetMapsEnabled(true, "InGameUI");
                CursorUtils.SetLocked(false); // always unlock for UI
            }
            else
            {
                p.controllers.maps.SetMapsEnabled(false, "InGameUI");
                p.controllers.maps.SetMapsEnabled(true, _currentGameplayCategory);

                var inOverview = CurrentMode == GameMode.Overview;
                CursorUtils.SetLocked(!inOverview); // keep free pointer in Overview
            }
        }
    }
}
