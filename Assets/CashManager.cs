using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CashManager : MonoBehaviour
{
    public static CashManager instance;

    private int money = 0;
    public int Money 
    {
        get => money; 
        set
        {
            money = value;
            if (money < 0)
                money = 0;
        }
    }

    private GameUIManager gameUIManager;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        money = SaveSystem.instance.Data.money;
    }

    private void Start()
    {
        gameUIManager = GameUIManager.instance;
    }

    public bool IsEnoughToSpend(int amountOfMoneyToSpend)
    {
        return amountOfMoneyToSpend <= money;
    }

    public void Spend(int amount)
    {
        money -= amount;
        gameUIManager.UpdateMoneyText(amount);
    }

    public void Earn(int amount)
    {
        money += amount;
        gameUIManager.UpdateMoneyText(amount);
    }
}
