using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Seller : FactoryObj
{
    //connect with UI manager, it will calculate cost and destroy objs
    private CashManager cashManager;

    public override bool IsFree 
    {
        get { return true; }
    }

    //private void OnTriggerEnter(Collider other)
    //{
    //    SellObject sellObj = other.GetComponent<SellObject>();

    //    if (sellObj)
    //    {
    //        //send info to manager (cost, what and etc)

    //        Destroy(sellObj.gameObject);
    //    }
    //}

    protected new void Awake()
    {
        base.Awake();
        cashManager = CashManager.instance;
    }

    public void SellObj(SellObject sellObj)
    {
        //send info to manager (cost, what and etc)
        cashManager.Earn(sellObj.cost);

        Destroy(sellObj.gameObject);
    }

    private new void OnDestroy()
    {
        base.OnDestroy();
    }
}
