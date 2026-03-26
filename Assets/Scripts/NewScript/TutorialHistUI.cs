using Manager;
using SharedUI.Tutorial;
using TMPro;
using UnityEngine;

public class TutorialHistUI : MonoBehaviour
{
    [SerializeField] TMP_Text headerText;
    [SerializeField] Transform listRoot;
    [SerializeField] GameObject itemElementPrefab;

    TutorialManager _tutorialManager;

    void Start()
    {
        _tutorialManager = TutorialManager.Instance;
        if (_tutorialManager == null)
        {
            Debug.Log("TutorialManager instance not found.");
            return;
        }

        Initialize();
    }

    void OnEnable()
    {
        Initialize();
    }


    public void Initialize()
    {
        // headerText.text = "Tutorial History";

        foreach (Transform child in listRoot) Destroy(child.gameObject);

        if (_tutorialManager == null)
        {
            Debug.LogError("TutorialManager instance is not set.");
            return;
        }

        foreach (var tutsArgs in _tutorialManager.GetAllTutBits())
        {
            var element = Instantiate(itemElementPrefab, listRoot);
            var elementScript = element.GetComponent<TutorialHistoryElementUI>();
            if (elementScript != null)
                elementScript.Initialize(tutsArgs.mainTutID);
            else
                Debug.LogError("TutorialHistElementUI component not found on the prefab.");
        }
    }
}
