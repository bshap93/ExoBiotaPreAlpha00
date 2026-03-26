using Helpers.Events;
using Sirenix.OdinInspector;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditorInternal;
#endif

public class ObjectiveStartCheckpoint : MonoBehaviour
{
    [SerializeField] string objectiveId;
#if UNITY_EDITOR
    [ValueDropdown("GetListOfTags")]
#endif
    [SerializeField]
    string tagName;

    void OnTriggerEnter(Collider other)
    {
        if (string.IsNullOrEmpty(objectiveId)) return;
        if (other.CompareTag(tagName)) ObjectiveEvent.Trigger(objectiveId, ObjectiveEventType.ObjectiveAdded);
    }
#if UNITY_EDITOR
    public static string[] GetListOfTags()
    {
        return InternalEditorUtility.tags;
    }
#endif
}
