using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seller : FactoryObj
{
    //connect with UI manager, it will calculate cost and destroy objs
    private CashManager cashManager;

    public static event Action<SellObject> OnSellerSoldObj;

    public override bool IsFree 
    {
        get { return true; }
    }

    protected new void Awake()
    {
        base.Awake();
        cashManager = CashManager.instance;
    }

    public void SellObj(SellObject sellObj)
    {
        //send info to manager (cost, what and etc)
        cashManager.Earn(sellObj.cost);

        OnSellerSoldObj?.Invoke(sellObj);

        Destroy(sellObj.gameObject);
    }

    private new void OnDestroy()
    {
        base.OnDestroy();
    }
}
