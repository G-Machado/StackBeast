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

    public void PurchaseUpgrade()
    {
        if (disabled) return;

        purchaseEnabled = false;
        OnUpgradePurchase?.Invoke(this);
    }

    public void ExitArea()
    {
        if (disabled) return;

        purchaseEnabled = true;
        fieldAnim.SetBool("fade", true);
    }

    public void DeactivateArea()
    {
        fieldAnim.SetBool("fade", false);
        disabled = true;
    }

    public void RefuseUpgrade()
    {
        fieldAnim.SetTrigger("refused");
    }

    public void ProcessPurchase()
    {
        fieldAnim.SetBool("fade", false);
    }
}

