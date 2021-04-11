using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pipeline : FactoryObj
{
    public SellObject itemToMove = null;
    public float moveDelay = 1f;
    private float timer = 0f;

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

                if (previousObjs[0])
                {
                    print(previousObjs[0].name + " now knows next is free.");
                    previousObjs[0].isNextObjFree = true;
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
            if (previousObjs[0] && previousObjs[0].type == FactoryObjTypes.Purchaser
                && !isNextObjFree && itemToMove)
            {
                print("Yep");
                previousObjs[0].isNextObjFree = false;
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
                switch (factoryObj.type)
                {
                    case FactoryObjTypes.Pipeline:

                        //Pipeline pipeline = (Pipeline)factoryObj;
                        //pipeline.previousObjs[0] = this;
                        //this.nextObj = pipeline;
                        //GetNextObjectFreeInfo();

                        factoryObj.previousObjs[0] = this;
                        this.nextObj = factoryObj;
                        GetNextObjectFreeInfo();

                        break;

                    case FactoryObjTypes.Factory:

                        //Factory factory = (Factory)factoryObj;
                        //factory.previousObjs.Add(this);
                        //this.nextObj = factory;
                        //GetNextObjectFreeInfo();

                        factoryObj.previousObjs.Add(this);
                        this.nextObj = factoryObj;
                        GetNextObjectFreeInfo();
                        //...

                        break;

                    case FactoryObjTypes.SellPort:

                        factoryObj.previousObjs[0] = this;
                        this.nextObj = factoryObj;
                        GetNextObjectFreeInfo();

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
        //if (previousObjs[0])
        //{
        //    switch(previousObjs[0].type)
        //    {
        //        case FactoryObjTypes.Purchaser:
        //            Purchaser purchaser = (Purchaser)previousObjs[0];
        //            purchaser.pipeline = null;
        //            break;

        //        case FactoryObjTypes.Pipeline:
        //            Pipeline previousPipeline = (Pipeline)previousObjs[0];
        //            previousPipeline.nextObj = null;
        //            break;
        //    }
        //}

        DetachNextWithThis();

        //purchaser fix
        if (previousObjs[0] && previousObjs[0].type == FactoryObjTypes.Purchaser)
        {
            previousObjs[0].nextObj = null;
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
