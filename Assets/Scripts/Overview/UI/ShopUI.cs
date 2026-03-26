using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

namespace Overview
{
    public class ShopUI : MonoBehaviour
    {
        [FormerlySerializedAs("shopCanvas")] public CanvasGroup shopUIPanel;

        private void Start()
        {
            // Ensure the shop UI is initially hidden
            HideShopUI();
        }

        public void CloseShopAndReturn(Transform dockAnchor)
        {
            HideShopUI();

            StartCoroutine(ReturnToDock(dockAnchor));
        }

        private IEnumerator ReturnToDock(Transform dockAnchor)
        {
            throw new NotImplementedException("ReturnToDock method is not implemented yet.");
        }

        public void ShowShopUI()
        {
            shopUIPanel.alpha = 1;
            shopUIPanel.blocksRaycasts = true;
            shopUIPanel.interactable = true;
        }

        public void HideShopUI()
        {
            shopUIPanel.alpha = 0;
            shopUIPanel.blocksRaycasts = false;
            shopUIPanel.interactable = false;
        }
    }
}