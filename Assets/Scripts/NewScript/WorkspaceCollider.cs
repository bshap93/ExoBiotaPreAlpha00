using System.Collections.Generic;
using Dirigible.Input;
using Helpers.Events;
using LevelConstruct.Interactable.ItemInteractables.ItemPicker;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

public class WorkspaceCollider : MonoBehaviour
{
    [SerializeField] MMFeedbacks enterIntendedItemFeedbacks;

#if UNITY_EDITOR
    [ValueDropdown(nameof(GetAllRewiredActions))]
#endif
    [SerializeField]
    int altActionId;
    [SerializeField] string additionalInstruction;
    [SerializeField] Sprite toolIcon;
    [SerializeField] string additionalText;

    readonly HashSet<string> _itemPickerUniqueIDs = new();

    // float _timeElapsed = 0f;

    void OnTriggerEnter(Collider other)
    {
        foreach (var varTag in tagsCheckedFor)
            if (other.CompareTag(varTag))
            {
                var uniqueID = other.GetComponent<ItemPicker>().uniqueID;
                if (_itemPickerUniqueIDs.Contains(uniqueID)) return;
                enterIntendedItemFeedbacks?.PlayFeedbacks();
                _itemPickerUniqueIDs.Add(uniqueID);
            }
    }

#if UNITY_EDITOR
    public IEnumerable<ValueDropdownItem<int>> GetAllRewiredActions()
    {
        return AllRewiredActions.GetAllRewiredActions();
    }

#endif

    public void PromptPlayerToPlaceWithAltInteractButton()
    {
        ControlsHelpEvent.Trigger(
            ControlHelpEventType.Show, altActionId);
    }

    public void ClearPromptPlayerToPlaceWithAltInteractButton()
    {
        ControlsHelpEvent.Trigger(ControlHelpEventType.Hide, altActionId);
    }


#if UNITY_EDITOR
    List<string> GetAllTags()
    {
        var tags = new List<string>();
        foreach (var tag in InternalEditorUtility.tags) tags.Add(tag);
        return tags;
    }

    [ValueDropdown(nameof(GetAllTags))]
#endif
    [SerializeField]
    List<string> tagsCheckedFor;
}
