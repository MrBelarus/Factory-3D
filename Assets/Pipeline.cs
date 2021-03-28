using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pipeline : FactoryObj
{
    public SellObject itemToMove = null;
    public float moveDelay = 1f;
    private float timer = 0f;

    public FactoryObj nextObj, previousObj;

    //can we send to this pipeline an item or not
    public override bool IsFree 
    { 
        get { return itemToMove == null ? true : false; }
    }

    private void Awake()
    {
        this.type = FactoryObjTypes.Pipeline;
    }

    private void FixedUpdate()
    {
        if (isNextObjFree && itemToMove)
        {
            if (timer >= moveDelay && nextObj)
            {
                print(itemToMove.name + " transporting by " + gameObject.name);


                if (itemToMove.MoveTo(transform.forward, nextObj, ObjectMoveTime))
                {
                    itemToMove = null;
                }
                else
                {
                    isNextObjFree = false;
                    return;
                }

                if (previousObj)
                {
                    print(previousObj.name + " now knows next is free.");
                    previousObj.isNextObjFree = true;
                }

                timer = 0f;
            }

            timer += Time.deltaTime;
            Mathf.Clamp(timer, 0, moveDelay + 1f);
        }
    }

    public void GetNextObjectFreeInfo()
    {
        if (nextObj)
        {
            isNextObjFree = nextObj.IsFree;

            //purchaser queue fix
            if (previousObj && previousObj.type == FactoryObjTypes.Purchaser
                && !isNextObjFree && itemToMove)
            {
                print("Yep");
                previousObj.isNextObjFree = false;
            }
        }
        else
        {
            isNextObjFree = false;
        }

        print("узнали, занят ли след. объект? - " + isNextObjFree + "\nНа объекте - " + gameObject.name);
    }

    private void OnTriggerEnter(Collider other)
    {
        print(other.name);

        if (other.transform.parent)
        {
            FactoryObj factoryObj = other.transform.parent.GetComponent<FactoryObj>();

            if (factoryObj)
            {
                switch(factoryObj.type)
                {
                    case FactoryObjTypes.Pipeline:

                        Pipeline pipeline = (Pipeline)factoryObj;
                        pipeline.previousObj = this;
                        this.nextObj = pipeline;
                        GetNextObjectFreeInfo();

                        break;

                    case FactoryObjTypes.Factory:

                        Factory factory = (Factory)factoryObj;
                        factory.previousObjs.Add(this);
                        this.nextObj = factory;
                        GetNextObjectFreeInfo();
                        //...

                        break;
                }

                nextObj = factoryObj;
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

    private void OnDestroy()
    {
        if (previousObj)
        {
            switch(previousObj.type)
            {
                case FactoryObjTypes.Purchaser:
                    Purchaser purchaser = (Purchaser)previousObj;
                    purchaser.pipeline = null;
                    break;

                case FactoryObjTypes.Pipeline:
                    Pipeline previousPipeline = (Pipeline)previousObj;
                    previousPipeline.nextObj = null;
                    break;
            }
        }

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
