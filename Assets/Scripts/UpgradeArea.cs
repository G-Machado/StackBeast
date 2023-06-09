using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class UpgradeArea : MonoBehaviour
{
    [SerializeField] private Animator fieldAnim;
    public UnityEvent<UpgradeArea> OnUpgradePurchase;
    private bool purchaseEnabled = true;
    [SerializeField] private bool disabled = false;
    [SerializeField] private GameObject costUI;

    public void TryPurchaseUpgrade()
    {
        if (disabled) return;

        purchaseEnabled = false;
        OnUpgradePurchase?.Invoke(this);
    }

    public void ExitArea()
    {
        if (disabled) return;

        // Reanables purchase attempt
        purchaseEnabled = true;

        fieldAnim.SetBool("fade", true);
        fieldAnim.SetBool("refused", false);
        costUI.SetActive(true);
    }

    public void DeactivateArea()
    {
        fieldAnim.SetBool("fade", false);

        // Disables completely the upgrade area
        disabled = true;
    }

    public void RefuseUpgrade()
    {
        fieldAnim.SetBool("refused", true);
    }

    public void ProcessPurchase()
    {
        costUI.SetActive(false);
        fieldAnim.SetBool("fade", false);
    }
}

