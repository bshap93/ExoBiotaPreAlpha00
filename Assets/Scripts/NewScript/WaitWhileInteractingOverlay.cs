using System;
using System.Collections;
using Michsky.MUIP;
using TMPro;
using UnityEngine;

public class WaitWhileInteractingOverlay : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] TMP_Text interactionTakingPlaceText;
    [SerializeField] ProgressBar interactionProgressBar;
    [SerializeField] GameObject blurVolume;
    CanvasGroup _canvasGroup;

    void Awake()
    {
        _canvasGroup = GetComponent<CanvasGroup>();
        Hide();
    }

    public void Show(string description)
    {
        interactionTakingPlaceText.text = description;
        _canvasGroup.alpha = 1;
        _canvasGroup.interactable = true;
        _canvasGroup.blocksRaycasts = true;
        interactionProgressBar.SetValue(0);
        interactionProgressBar.isOn = true;
        blurVolume.SetActive(true);
    }

    public void Hide()
    {
        _canvasGroup.alpha = 0;
        _canvasGroup.interactable = false;
        _canvasGroup.blocksRaycasts = false;
        interactionProgressBar.isOn = false;
        blurVolume.SetActive(false);
    }

    public IEnumerator SimulateProgress(float duration, Action onComplete = null)
    {
        var elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            var progress = Mathf.Clamp01(elapsed / duration);
            interactionProgressBar.SetValue(progress * 100f);
            yield return null;
        }

        // ADD: Ensure we complete even if timing is off
        interactionProgressBar.SetValue(100f);
        yield return null; // One more frame to ensure completion

        Debug.Log("[WaitOverlay] Invoking completion callback");
        onComplete?.Invoke();
    }
}
