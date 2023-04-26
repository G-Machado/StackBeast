using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;

public class UpgradeManager : MonoBehaviour
{
    public static UpgradeManager instance;
    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(this);
    }

    [Header("Currency feature")]
    [SerializeField] private TextMeshProUGUI currencyDisplay;
    [SerializeField] private Animator currencyDisplayAnim;
    [SerializeField] private int currency;

    [Header("Stacking Upgrades")]
    public List<stackStat> stackUpgrades = new List<stackStat>();
    public int stackLevel = -1;
    public stackStat currentStackStats
    {
        get { return stackLevel >= 0 ? stackUpgrades[stackLevel] : null; }
    }
    [SerializeField] private TextMeshProUGUI stackValueDisplay;
    [SerializeField] private Animator stackDisplayAnim;
    [SerializeField] private TextMeshProUGUI stackCostDisplay;
    [SerializeField] private SkinnedMeshRenderer skinned_renderer;

    [Header("Punch Upgrades")]
    public List<punchStat> punchUpgrades = new List<punchStat>();
    public int punchLevel = -1;
    public punchStat currentPunchStats
    {
        get { return punchLevel >= 0 ? punchUpgrades[punchLevel] : null; }
    }
    [SerializeField] private TextMeshProUGUI punchValueDisplay;
    [SerializeField] private TextMeshProUGUI punchCostDisplay;
    [SerializeField] private Animator punchDisplasyAnim;

    public void InitializeUpgrades()
    {
        // Upgrade stack and punch to level 0
        UpgradeStack(null);
        UpgradePunch(null);
    }

    public void UpdateStackUI()
    {
        stackValueDisplay.text = $"{PlayerMovement.instance.enemiesStacked.Count}/{currentStackStats.maxStack}";
        stackDisplayAnim.SetTrigger("uiChange");
    }

    public void AddCurrency(int ammount)
    {
        currency += ammount;

        // Updates UI
        currencyDisplay.text = $"${currency}";
        currencyDisplayAnim.SetTrigger("uiChange");
    }

    public void UpgradeStack(UpgradeArea area)
    {
        stackLevel++;

        // Configure upgrade area if player doesn't have money
        if (currency < currentStackStats.cost)
        {
            stackLevel--;
            if (area) area.RefuseUpgrade();
            return;
        }

        // Configure upgrade area if a purchase is sucessfull
        if (area) area.ProcessPurchase();

        // Shows new items to stack
        for (int i = 0; i < ItemsSetup.instance.items.Count; i++)
        {
            bool meshEnabled = false;
            if (i < currentStackStats.maxStack) meshEnabled = true;

            ItemsSetup.instance.items[i].GetComponent<MeshRenderer>().enabled = meshEnabled;
        }

        // Updates currency
        currency -= currentStackStats.cost;

        // Updates UI
        currencyDisplay.text = $"${currency}";
        currencyDisplayAnim.SetTrigger("uiChange");
        stackCostDisplay.text = $"${stackUpgrades[Mathf.Min(stackUpgrades.Count - 1, stackLevel + 1)].cost}";
        UpdateStackUI();

        // Updates player skin color
        skinned_renderer.material.color =
            currentStackStats.color;

        // Configure upgrade area if this was the last upgrade possible
        if (stackLevel >= stackUpgrades.Count - 1)
        {
            if (area) area.DeactivateArea();
            return;
        }

    }

    public void UpgradePunch(UpgradeArea area)
    {
        punchLevel++;

        if(currency < currentPunchStats.cost)
        {
            punchLevel--;
            if (area) area.RefuseUpgrade();
            return;
        }

        if (area) area.ProcessPurchase();

        currency -= currentPunchStats.cost;
        currencyDisplay.text = $"${currency}";
        currencyDisplayAnim.SetTrigger("uiChange");

        punchCostDisplay.text = $"${punchUpgrades[Mathf.Min(punchUpgrades.Count - 1, punchLevel + 1)].cost}";
        punchValueDisplay.text = $"{punchLevel + 1}";
        punchDisplasyAnim.SetTrigger("uiChange");
        //UpdateUIStats();

        if (punchLevel >= punchUpgrades.Count - 1)
        {
            if (area) area.DeactivateArea();
            return;
        }
    }


    

}

[System.Serializable]
public class stackStat
{
    public int cost;
    public int maxStack;
    public Color color;
}

[System.Serializable]
public class punchStat
{
    public int cost;
    public int punchRadius;
    public int punchForce;
    public GameObject explosionEffect;
}
