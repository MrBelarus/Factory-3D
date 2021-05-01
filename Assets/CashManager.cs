using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class CashManager : MonoBehaviour
{
    public static CashManager instance;
    public static event Action<int> OnMoneyEarned;
    public static event Action<int> OnMoneySpent;
    public static event Action<int> OnMoneyChanged;

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
    }

    private void Start()
    {
        money = SaveSystem.instance.Data.money;

        gameUIManager = GameUIManager.instance;
        gameUIManager.UpdateMoneyText(money);
    }

    public bool IsEnoughToSpend(int amountOfMoneyToSpend)
    {
        return amountOfMoneyToSpend <= money;
    }

    public void Spend(int amount)
    {
        money -= amount;

        OnMoneySpent?.Invoke(amount);
        OnMoneyChanged?.Invoke(money);

        gameUIManager.UpdateMoneyText(money);
    }

    public void Earn(int amount)
    {
        money += amount;

        OnMoneyEarned?.Invoke(amount);
        OnMoneyChanged?.Invoke(money);

        gameUIManager.UpdateMoneyText(money);
    }
}
