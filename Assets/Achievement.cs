using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public abstract class Achievement
{
    public string title;
    public string description;
    public bool achieved = false;

    public abstract void SubscribeEvent();
    public abstract void UnsubscribeEvent();
}

/// <summary>
/// You have bought 100+ Iron Ores
/// </summary>
[System.Serializable]
public class IronManAchievement : Achievement
{
    int ironOreBoughtCount = 0;
    const int ironOreBoughtGoal = 3;

    public IronManAchievement()
    {
        title = "Iron Man";
        description = "You have bought 100+ iron ores in the Purchaser";
    }

    public void Logic(SellObject sellObject)
    {
        if (sellObject.material == Materials.IronOre)
        {
            ironOreBoughtCount++;
            if (ironOreBoughtCount > ironOreBoughtGoal)
            {
                achieved = true;

                //achievementController.ShowAchievement(this)
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
    }
}
