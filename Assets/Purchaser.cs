using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Purchaser : FactoryObj
{
    public static event Action<SellObject> OnPurchaseItemEnqueued;
    public static event Action<SellObject> OnPurchaseItemInstantiated;

    public SellObject itemToPurchase;
    public Transform objSpawnPoint;

    public float timeToPurchaseItem;
    private float timer = 0f;

    public List<Materials> AvailableMaterials;
    public bool AutoBuy { set; get; }

    private Queue<SellObject> purchaseQueue;
    public int PurchaseQueueCount { get { return purchaseQueue.Count; } }

    private CashManager cashManager;

    private new void Awake()
    {
        this.type = FactoryObjTypes.Purchaser;

        purchaseQueue = new Queue<SellObject>();

        cashManager = CashManager.instance;

        base.Awake();
    }

    ////имитация покупки через UI
    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.Space))
    //    {
    //        itemToPurchase = Resources.Load("IronOre") as GameObject;

    //        BuyMaterial(itemToPurchase);
    //    }
    //}

    private void FixedUpdate()
    {
        //autobuy implementation
        if (isNextObjFree)
        {
            if (AutoBuy && purchaseQueue.Count == 0 && itemToPurchase 
                && cashManager.IsEnoughToSpend(itemToPurchase.cost))
            {
                purchaseQueue.Enqueue(itemToPurchase);
                OnPurchaseItemEnqueued?.Invoke(itemToPurchase);

                cashManager.Spend(itemToPurchase.cost);
            }
        }

        if (isNextObjFree && purchaseQueue.Count > 0)
        {
            if (timer >= timeToPurchaseItem && nextObj)
            {
                isNextObjFree = nextObj.IsFree;
                if(!isNextObjFree)
                {
                    print("+");
                    return;
                }

                print("It's purchased!");
                itemToPurchase = purchaseQueue.Dequeue();   //inqueue if we have itemToPurchase but timer < timeToPurchaseItem

                GameObject purchasedItem = Instantiate(itemToPurchase.gameObject, objSpawnPoint.position, objSpawnPoint.rotation);
                purchasedItem.GetComponent<SellObject>().MoveTo(transform.forward, nextObj, ObjectMoveTime);

                OnPurchaseItemInstantiated?.Invoke(itemToPurchase);

                //if (purchaseQueue.Count == 0)
                //{
                //    itemToPurchase = null;
                //}

                timer = 0f;
            }

            timer += Time.deltaTime;

            Mathf.Clamp(timer, 0, timeToPurchaseItem + 1f);
        }
    }

    public void BuyMaterial(SellObject material)
    {
        if (cashManager.IsEnoughToSpend(material.cost))
        {
            purchaseQueue.Enqueue(material);
            OnPurchaseItemEnqueued?.Invoke(itemToPurchase);

            cashManager.Spend(material.cost);
        }
        else
        {
            //sound which means no money or error
        }

        //first obj to buy & purchaser connected to pipeline
        //if (purchaseQueue.Count == 1 && pipeline)
        //{
        //    isNextObjFree = pipeline.IsFree;
        //}
    }

    public void ClearPurchaseQueue()
    {
        foreach (SellObject obj in purchaseQueue)
        {
            cashManager.Earn(obj.cost / 2);
        }

        purchaseQueue.Clear();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent)
        {
            FactoryObj factoryObj = other.transform.parent.GetComponent<FactoryObj>();

            if (factoryObj == null)
            {
                Debug.LogWarning("smth went wrong, object: " + other.name);
                return;
            }

            switch (factoryObj.type)
            {
                case FactoryObjTypes.Pipeline:
                    nextObj = factoryObj;
                    nextObj.previousObjs.Add(this);
                    isNextObjFree = nextObj.IsFree; //TODO: тут может быть фикс
                    break;

                default:
                    Debug.LogError("error connection! [purchase can connect only with pipeline]");
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent)
        {
            FactoryObj factoryObj = other.transform.parent.GetComponent<FactoryObj>();
            print("Удалили объект после продавщика");
            if (factoryObj)
            {
                nextObj = null;
            }
        }
    }

    private new void OnDestroy()
    {
        base.OnDestroy();

        DetachNextWithThis();

        Collider[] overlaps = Physics.OverlapBox(transform.position + Vector3.up / 2, Vector3.one / 2.05f);
        foreach (Collider overlap in overlaps)
        {
            if (overlap.CompareTag("SellObj"))
            {
                Destroy(overlap.gameObject);
            }
        }
    }
}
