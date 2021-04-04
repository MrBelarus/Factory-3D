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

    public Queue<GameObject> purchaseQueue;

    public Pipeline pipeline;       //game rule is to connect Purchaser with at least 1 pipeline!

    private void Awake()
    {
        this.type = FactoryObjTypes.Purchaser;

        purchaseQueue = new Queue<GameObject>();
    }

    //имитация покупки через UI
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            itemToPurchase = Resources.Load("Iron Ore") as GameObject;

            BuyMaterial(itemToPurchase);
        }
    }

    private void FixedUpdate()
    {
        if (isNextObjFree && purchaseQueue.Count > 0)
        {
            if (timer >= timeToPurchaseItem && pipeline)
            {
                print("It's purchased!");

                itemToPurchase = purchaseQueue.Dequeue();   //inqueue if we have itemToPurchase but timer < timeToPurchaseItem

                GameObject purchasedItem = Instantiate(itemToPurchase, objSpawnPoint.position, itemToPurchase.transform.rotation);
                purchasedItem.GetComponent<SellObject>().MoveTo(transform.forward, pipeline, ObjectMoveTime);

                if (purchaseQueue.Count == 0)
                {
                    itemToPurchase = null;
                }

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
                    pipeline = (Pipeline)factoryObj;
                    pipeline.previousObjs[0] = this;
                    isNextObjFree = pipeline.IsFree;
                    print("22+");
                    break;

                default:
                    Debug.LogError("Game can connect purchaser just with pipeline!");
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
                pipeline = null;
            }
        }
    }

    private void OnDestroy()
    {
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

    ////temp
    //private void OnMouseDown()
    //{
    //    print("+");
    //    //open purchase menu to buy objs[use queue]/setup purchaser
    //    //change obj to purchase
    //}
}
