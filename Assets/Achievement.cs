using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Achievement
{
    public string title;
    public string description;
    public bool achieved = false;
    public bool subscribeOnLoad = true;

    public abstract void SubscribeEvent();
    public abstract void UnsubscribeEvent();
}

/// <summary>
/// Base class for Purchaser buy achievements
/// </summary>
[System.Serializable]
public class PurchaserBuyAchievement : Achievement
{
    public int boughtCount = 0;
    public int boughtGoal;
    public Materials materialToBuy;

    public PurchaserBuyAchievement(Materials material, string title,
        string description, int boughtGoal)
    {
        this.title = title;
        this.description = description;
        this.materialToBuy = material;
        this.boughtGoal = boughtGoal;
    }

    public void Logic(SellObject sellObject)
    {
        if (sellObject.material == materialToBuy)
        {
            boughtCount++;
            if (boughtCount > boughtGoal)
            {
                achieved = true;

                if (AchievementController.instance)
                {
                    AchievementController.instance.ShowAchievement(this);
                }
                UnsubscribeEvent();
            }
        }
    }

    //will be called in the controller while settuping all game achievements
    public override void SubscribeEvent()
    {
        Purchaser.OnPurchaseItemEnqueued += Logic;
    }

    public override void UnsubscribeEvent()
    {
        Purchaser.OnPurchaseItemEnqueued -= Logic;
        Debug.Log(title + " was unsubscribed!");
    }
}

/// <summary>
/// Base class for Factory produce achievements
/// </summary>
[System.Serializable]
public class FactoryProduceAchievement : Achievement
{
    public int producedCount = 0;
    public int producedGoal;
    public Materials materialToProduce;

    public FactoryProduceAchievement(Materials material, string title,
        string description, int producedGoal)
    {
        this.title = title;
        this.description = description;
        this.materialToProduce = material;
        this.producedGoal = producedGoal;
    }

    public void Logic(SellObject sellObject)
    {
        if (sellObject.material == materialToProduce)
        {
            producedCount++;
            if (producedCount > producedGoal)
            {
                achieved = true;

                if (AchievementController.instance)
                {
                    AchievementController.instance.ShowAchievement(this);
                }
                UnsubscribeEvent();
            }
        }
    }

    //will be called in the controller while settuping all game achievements
    public override void SubscribeEvent()
    {
        Factory.OnFactoryObjProduced += Logic;
    }

    public override void UnsubscribeEvent()
    {
        Factory.OnFactoryObjProduced -= Logic;
        Debug.Log(title + " was unsubscribed!");
    }
}

/// <summary>
/// Base class for keyboard key pressed [with delay < 1sec] achievements
/// </summary>
[System.Serializable]
public class KeyboardAchievement : Achievement
{
    public int keyPressCount = 0;
    public int keyPressGoalCount;

    public int keyCodeToPress;

    [System.NonSerialized]
    private float lastClickedTime;

    [System.NonSerialized]
    private float maxClickedTimeDelay = 1f;

    public KeyboardAchievement(KeyCode keyToPress, string title,
        string description, int keyPressGoal)
    {
        this.title = title;
        this.description = description;
        this.keyCodeToPress = (int)keyToPress;
        this.keyPressGoalCount = keyPressGoal;
    }

    public void Logic(KeyCode pressedKey, float pressedTime)
    {
        if (pressedTime - lastClickedTime > maxClickedTimeDelay)
        {
            keyPressCount = 0;
            lastClickedTime = pressedTime;
        }

        if (keyCodeToPress == (int)pressedKey && pressedTime - lastClickedTime < maxClickedTimeDelay)
        {
            Debug.Log(pressedKey.ToString() + " pressed. Delay = " + (pressedTime - lastClickedTime) + "| lastTime = " + lastClickedTime);
            keyPressCount++;
            if (keyPressCount > keyPressGoalCount)
            {
                achieved = true;

                if (AchievementController.instance)
                {
                    AchievementController.instance.ShowAchievement(this);
                }
                UnsubscribeEvent();
            }

            lastClickedTime = pressedTime;
        }

    }

    //will be called in the controller while settuping all game achievements
    public override void SubscribeEvent()
    {
        GameManager.OnKeyPressed += Logic;
    }

    public override void UnsubscribeEvent()
    {
        GameManager.OnKeyPressed -= Logic;
        Debug.Log(title + " was unsubscribed!");
    }
}


/// <summary>
/// Base class for Factory produce achievements
/// </summary>
[System.Serializable]
public class FactoryObjBuyAchievement : Achievement
{
    public int boughtCount = 0;
    public int boughtGoalCount;
    public FactoryObjTypes factoryType;

    public FactoryObjBuyAchievement(FactoryObjTypes factory, string title,
        string description, int boughtGoalCount)
    {
        this.title = title;
        this.description = description;
        this.factoryType = factory;
        this.boughtGoalCount = boughtGoalCount;
    }

    public void Logic(FactoryObj factoryObj)
    {
        if (factoryObj.type == factoryType)
        {
            boughtCount++;
            if (boughtCount > boughtGoalCount)
            {
                achieved = true;

                if (AchievementController.instance)
                {
                    AchievementController.instance.ShowAchievement(this);
                }
                UnsubscribeEvent();
            }
        }
    }

    //will be called in the controller while settuping all game achievements
    public override void SubscribeEvent()
    {
        Builder.OnFactoryObjBuy += Logic;
    }

    public override void UnsubscribeEvent()
    {
        Builder.OnFactoryObjBuy -= Logic;
        Debug.Log(title + " was unsubscribed!");
    }
}


/// <summary>
/// Base class for Factory delete achievements
/// </summary>
[System.Serializable]
public class FactoryObjDeleteAchievement : Achievement
{
    public int deletedCount = 0;
    public int deletedGoalCount;
    public FactoryObjTypes? factoryType;    //if it is null -> count if any factory obj was deleted

    public FactoryObjDeleteAchievement(FactoryObjTypes? factory, string title,
        string description, int deletedGoalCount)
    {
        this.title = title;
        this.description = description;
        this.factoryType = factory;
        this.deletedGoalCount = deletedGoalCount;
    }

    public void Logic(FactoryObj factoryObj)
    {
        if (factoryType == null || factoryObj.type == factoryType)
        {
            deletedCount++;
            if (deletedCount > deletedGoalCount)
            {
                achieved = true;

                if (AchievementController.instance)
                {
                    AchievementController.instance.ShowAchievement(this);
                }
                UnsubscribeEvent();
            }
        }
    }

    //will be called in the controller while settuping all game achievements
    public override void SubscribeEvent()
    {
        Builder.OnFactoryObjDeleted += Logic;
    }

    public override void UnsubscribeEvent()
    {
        Builder.OnFactoryObjDeleted -= Logic;
        Debug.Log(title + " was unsubscribed!");
    }
}


/// <summary>
/// Base class for Money Count achievements
/// </summary>
[System.Serializable]
public class MoneyCountAchievement : Achievement
{
    public int totalMoneyGoal;
    public MoneyCountAchievement nextStage;    //if it is null -> next achievement will not subscribe [it's empty]

    public MoneyCountAchievement(MoneyCountAchievement nextAchievement, string title,
        string description, int totalMoneyGoal, bool subscribeOnLoad)
    {
        this.nextStage = nextAchievement;
        this.title = title;
        this.description = description;
        this.totalMoneyGoal = totalMoneyGoal;
        this.subscribeOnLoad = subscribeOnLoad;
    }

    public void Logic(int money)
    {
        Debug.Log(description + " " + money);
        if (money >= totalMoneyGoal)
        {
            achieved = true;

            if (AchievementController.instance)
            {
                AchievementController.instance.ShowAchievement(this);
            }
            UnsubscribeEvent();

            this.subscribeOnLoad = false;
            if (nextStage != null)
            {
                nextStage.subscribeOnLoad = true;
                nextStage.SubscribeEvent();
            }
        }
    }

    //will be called in the controller while settuping all game achievements
    public override void SubscribeEvent()
    {
        CashManager.OnMoneyChanged += Logic;
    }

    public override void UnsubscribeEvent()
    {
        CashManager.OnMoneyChanged -= Logic;
        Debug.Log(title + " was unsubscribed!");
    }
}