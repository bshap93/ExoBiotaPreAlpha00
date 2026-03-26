using System;
using FirstPersonPlayer.Interface;
using UnityEngine;

public class StatefulChest : MonoBehaviour, IInteractable
{
    // Lid rotates on the X axis from -89.98 to -132.011
    [SerializeField] GameObject lid;
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    public void Interact()
    {
        throw new NotImplementedException();
    }
    public void Interact(string param)
    {
        throw new NotImplementedException();
    }
    public void OnInteractionStart()
    {
        throw new NotImplementedException();
    }
    public void OnInteractionEnd(string param)
    {
        throw new NotImplementedException();
    }
    public bool CanInteract()
    {
        throw new NotImplementedException();
    }
    public bool IsInteractable()
    {
        throw new NotImplementedException();
    }
    public void OnFocus()
    {
        throw new NotImplementedException();
    }
    public void OnUnfocus()
    {
        throw new NotImplementedException();
    }
    public float GetInteractionDistance()
    {
        throw new NotImplementedException();
    }
}
