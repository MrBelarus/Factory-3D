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
    //время, затраченное на покупку предмета (1с из 2с-timeToPurchaseItem, например)
    public float BuyTimer { get => timer; set => timer = value; }

    public List<Materials> AvailableMaterials;
    public bool AutoBuy { set; get; }

    private Queue<SellObject> purchaseQueue;
    public Queue<SellObject> PurchaseQueue { get { return purchaseQueue; } }
    public int PurchaseQueueCount { get { return purchaseQueue.Count; } }
    public Materials[] MaterialsQueue
    {
        get
        {
            SellObject[] queue = purchaseQueue.ToArray();
            Materials[] materials = new Materials[queue.Length];

            for (int i = 0; i < queue.Length; i++)
            {
                materials[i] = queue[i].material;
            }

            return materials;
        }
    }

    private CashManager cashManager;
    private GameObject purchasedItem;   //объект, который находится в закупщике (ждет)
    public GameObject PurchasedItem { get => purchasedItem; set => purchasedItem = value; }

    private new void Awake()
    {
        this.type = FactoryObjTypes.Purchaser;

        purchaseQueue = new Queue<SellObject>();

        cashManager = CashManager.instance;

        base.Awake();
    }

    private void FixedUpdate()
    {
        if (isNextObjFree)
        {
            //autobuy implementation
            if (AutoBuy && purchaseQueue.Count == 0 && itemToPurchase 
                && cashManager.IsEnoughToSpend(itemToPurchase.cost))
            {
                purchaseQueue.Enqueue(itemToPurchase);
                OnPurchaseItemEnqueued?.Invoke(itemToPurchase);

                cashManager.Spend(itemToPurchase.cost);
            }

            if (purchaseQueue.Count > 0 || purchasedItem != null)
            {
                if (timer >= timeToPurchaseItem && nextObj)
                {
                    if (purchasedItem == null)
                    {
                        //new temp itemToPurchase to prevent issues with autobuy
                        SellObject itemToPurchase = purchaseQueue.Dequeue();   //inqueue if we have itemToPurchase but timer < timeToPurchaseItem
                        purchasedItem = Instantiate(itemToPurchase.gameObject, objSpawnPoint.position, objSpawnPoint.rotation);
                        OnPurchaseItemInstantiated?.Invoke(itemToPurchase);
                    }

                    if (purchasedItem.GetComponent<SellObject>().MoveTo(transform.forward, nextObj, ObjectMoveTime))
                    {
                        purchasedItem = null;
                    }
                    else
                    {
                        isNextObjFree = false;
                        return;
                    }

                    timer = 0f;
                }

                timer += Time.deltaTime;

                Mathf.Clamp(timer, 0, timeToPurchaseItem + 1f);
            }
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
    }

    public void ClearPurchaseQueue()
    {
        foreach (SellObject obj in purchaseQueue)
        {
            cashManager.Earn(obj.cost / 2);
        }

        purchaseQueue.Clear();
    }

    public void CopyValues(Purchaser purchaser)
    {
        timer = purchaser.timer;
        purchaseQueue = purchaser.PurchaseQueue;
        itemToPurchase = purchaser.itemToPurchase;
        AutoBuy = purchaser.AutoBuy;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent)
        {
            FactoryObj factoryObj = other.transform.parent.GetComponent<FactoryObj>();

            if (factoryObj == null)
            {
                Debug.LogError("smth went wrong, object: " + other.name);
                return;
            }

            switch (factoryObj.type)
            {
                case FactoryObjTypes.Pipeline:
                    nextObj = factoryObj;
                    nextObj.previousObjs.Add(this);
                    isNextObjFree = nextObj.IsFree; //TODO: тут может быть фикс
                    break;

                case FactoryObjTypes.Factory:
                    factoryObj.previousObjs.Add(this);
                    nextObj = factoryObj;
                    isNextObjFree = factoryObj.IsFree;
                    break;

                case FactoryObjTypes.SellPort:
                    factoryObj.previousObjs[0] = this;
                    nextObj = factoryObj;
                    isNextObjFree = factoryObj.IsFree;
                    break;

                default:
                    Debug.LogError("smth went wrong with Purchaser connection!");
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.transform.parent)
        {
            FactoryObj factoryObj = other.transform.parent.GetComponent<FactoryObj>();

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
