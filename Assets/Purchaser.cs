using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Purchaser : FactoryObj
{
    public GameObject itemToPurchase;
    public Transform objSpawnPoint;

    public float timeToPurchaseItem;
    private float timer = 0f;

    public List<Materials> AvailableMaterials;
    public bool AutoBuy { set; get; }

    private Queue<GameObject> purchaseQueue;
    public int PurchaseQueueCount { get { return purchaseQueue.Count; } }

    private void Awake()
    {
        this.type = FactoryObjTypes.Purchaser;

        purchaseQueue = new Queue<GameObject>();
    }

    protected new void Start()
    {
        base.Start();
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
            if (AutoBuy && purchaseQueue.Count == 0 && itemToPurchase)
            {
                purchaseQueue.Enqueue(itemToPurchase);
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

                GameObject purchasedItem = Instantiate(itemToPurchase, objSpawnPoint.position, objSpawnPoint.rotation);
                purchasedItem.GetComponent<SellObject>().MoveTo(transform.forward, nextObj, ObjectMoveTime);

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

    public void BuyMaterial(GameObject material)
    {
        purchaseQueue.Enqueue(material);

        //first obj to buy & purchaser connected to pipeline
        //if (purchaseQueue.Count == 1 && pipeline)
        //{
        //    isNextObjFree = pipeline.IsFree;
        //}
    }

    public void ClearPurchaseQueue()
    {
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
