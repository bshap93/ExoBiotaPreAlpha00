using System;
using JetBrains.Annotations;
using MoreMountains.Tools;
using UnityEngine;

namespace Helpers.Events
{
    [Serializable]
    public enum AlertReason
    {
        InsufficientFunds,
        OutOfFuel,
        SavingGame,
        Died,

        InventotryEmpty,
        KeyAccessChange,
        Test,
        KeyAccessDenied,
        SampleLimitExceeded,
        InRangeOfDockingStation,
        ContaminationWarning,
        Decontamination,
        CannotSequenceSample,
        InvalidAction,
        SuccessfulChemicalApplication,
        NewObjective,
        HealthWarning,
        InventoryFull,
        GateInteractable,
        UseDoor,
        CannotSave,
        SampleChangeHands,
        MaxHealthDecrease,
        HoldingItemAlready,
        ItemsRemoved,
        ItemMoved,
        ItemNotReady,
        LackToolForInteraction,
        NotEnoughStamina,
        CannotPlaceQuestItem,
        MachineInteraction,
        MaxContaminationReached,
        ContaminationMaxedOut,
        GatedUIActionInvalid,
        ElevatorIssue,
        BrokenMachine,
        MachineLacksPower,
        StatusEffectApplied,
        InfectionContracted,
        InfectionContracted_Skin01,
        InfectionContracted_Lung01,
        InfectionContracted_Heart01,
        InfectionContracted_Brain01,
        InfectionContracted_Eyes01,
        TooFarFromDirigible,
        ElevatorSceneChangePermission,
        DoorLocked,
        PlayTestEndYesOrNo,
        BreakableToolIneffective,
        InRangeOfOverworldNPCDirect,
        CurrencyGained,
        NotEnoughAmmo,
        NotEnoughContamination,
        StatUpgradePurchased,
        ClassSelected,
        NewAttributePoints,
        NewStatUpgrade,
        AttributePointSpent,
        HealtMaxIncrease,
        HotbarFull,
        AutoSave,
        RangedWeaponInUse,
        InsufficientResources
    }

    [Serializable]
    public enum AlertType
    {
        Basic,
        ChoiceModal,
        ControlsPrompt,
        TutorialWindow,
        PauseAndGiveInfo
    }

    [Serializable]
    public enum ActionType
    {
        StartAlert,
        EndAlert
    }

    [Serializable]
    public class AlertContent
    {
        public AlertReason alertReason;
        public AlertType alertType;
        public string alertTitle;
        public string alertMessage;
        public Sprite alertIcon;
    }

    public struct AlertEvent
    {
        public static AlertEvent _e;

        public AlertType AlertType;

        public AlertReason AlertReason;
        public string AlertMessage;
        public float AlertDuration; // Default duration for the alert
        [CanBeNull] public string AlertTitle;
        [CanBeNull] public Sprite AlertIcon;
        [CanBeNull] public AudioClip AlertSound;
        [CanBeNull] public Sprite AlertImage;
        public ActionType ActionType;

        // NEW: optional callbacks
        public Action OnConfirm;
        public Action OnCancel;

        public static void Trigger(AlertReason alertReason, string alertMessage, string alertTitle = "Alert",
            AlertType alertType = AlertType.Basic,
            float alertDuration = 3f,
            Sprite alertIcon = null,
            AudioClip alertSound = null,
            Action onConfirm = null,
            Action onCancel = null,
            ActionType actionType = ActionType.StartAlert,
            Sprite alertImage = null
        )
        {
            _e.AlertReason = alertReason;
            _e.AlertMessage = alertMessage;
            _e.AlertTitle = alertTitle;
            _e.AlertIcon = alertIcon;
            _e.AlertSound = alertSound;
            _e.AlertDuration = alertDuration;
            _e.AlertType = alertType;
            _e.OnConfirm = onConfirm;
            _e.OnCancel = onCancel;
            _e.ActionType = actionType;
            _e.AlertImage = alertImage;
            MMEventManager.TriggerEvent(_e);
        }
        public AlertContent ToAlertContent()
        {
            return new AlertContent
            {
                alertReason = AlertReason,
                alertType = AlertType,
                alertTitle = AlertTitle ?? "Alert",
                alertMessage = AlertMessage,
                alertIcon = AlertIcon
            };
        }
    }
}
