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
    [SerializeField] private int currency;

    [Header("Stacking Upgrades")]
    public List<stackStat> stackUpgrades = new List<stackStat>();
    
    public int stackLevel = -1;
    public stackStat currentStackStats
    {
        get { return stackLevel >= 0 ? stackUpgrades[stackLevel] : null; }
    }
    [SerializeField] private TextMeshProUGUI stackCostDisplay;

    [Header("Punch Upgrades")]
    public List<punchStat> punchUpgrades = new List<punchStat>();
    
    public int punchLevel = -1;
    public punchStat currentPunchStats
    {
        get { return punchLevel >= 0 ? punchUpgrades[punchLevel] : null; }
    }
    [SerializeField] private TextMeshProUGUI punchCostDisplay;

    void Start()
    {
        // LoadData();
    }

    public void InitializeUpgrades()
    {
        UpgradeStack(null);
        UpgradePunch(null);
    }

    public void AddCurrency(int ammount)
    {
        currency += ammount;
        currencyDisplay.text = currency.ToString();
    }

    public void UpgradeStack(UpgradeArea area)
    {
        stackLevel++;

        if (currency < currentStackStats.cost)
        {
            stackLevel--;
            if (area) area.RefuseUpgrade();
            return;
        }

        if (area) area.ProcessPurchase();

        // Shows new items to stack
        for (int i = 0; i < ItemsSetup.instance.items.Count; i++)
        {
            bool meshEnabled = false;
            if (i < currentStackStats.maxStack) meshEnabled = true;

            ItemsSetup.instance.items[i].GetComponent<MeshRenderer>().enabled = meshEnabled;
        }

        currency -= currentStackStats.cost;
        UpdateUIStats();

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

        currency -= currentPunchStats.cost;
        UpdateUIStats();

        if (punchLevel >= punchUpgrades.Count - 1)
        {
            if (area) area.DeactivateArea();
            return;
        }
    }

    private void UpdateUIStats()
    {
        currencyDisplay.text = currency.ToString();
        stackCostDisplay.text = stackUpgrades[Mathf.Min(stackUpgrades.Count-1 , stackLevel+1)].cost.ToString();
        punchCostDisplay.text = punchUpgrades[Mathf.Min(punchUpgrades.Count-1 , punchLevel+1)].cost.ToString();
    }
}

[System.Serializable]
public class stackStat
{
    public int cost;
    public int maxStack;
}

[System.Serializable]
public class punchStat
{
    public int cost;
    public int punchRadius;
    public int punchForce;
}
