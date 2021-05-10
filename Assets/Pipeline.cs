using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Pipeline : FactoryObj
{
    public SellObject itemToMove = null;
    public float moveDelay = 1f;
    private float timer = 0f;

    public bool isRecievingItem = false;

    //can we send to this pipeline an item or not
    public override bool IsFree
    {
        get { return itemToMove == null && !isRecievingItem; }
    }

    protected new void Awake()
    {
        this.type = FactoryObjTypes.Pipeline;
        base.Awake();
    }

    private void FixedUpdate()
    {
        if (isNextObjFree && itemToMove)
        {
            if (timer >= moveDelay && nextObj)
            {
                if (itemToMove.MoveTo(transform.forward, nextObj, ObjectMoveTime))
                {
                    itemToMove = null;
                }
                else
                {
                    isNextObjFree = false;
                    return;
                }

                TellPreviousObjsIAmFree();

                timer = 0f;
            }

            timer += Time.deltaTime;
            Mathf.Clamp(timer, 0, moveDelay + 1f);
        }
    }

    public void TellPreviousObjsIAmFree()
    {
        for (int i = 0; i < previousObjs.Count; i++)
        {
            previousObjs[i].isNextObjFree = true;
        }
    }

    public void GetNextObjectFreeInfo()
    {
        if (nextObj)
        {
            isNextObjFree = nextObj.IsFree;
        }
        else
        {
            isNextObjFree = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.transform.parent)
        {
            FactoryObj factoryObj = other.transform.parent.GetComponent<FactoryObj>();

            if (factoryObj)
            {
                switch (factoryObj.type)
                {
                    case FactoryObjTypes.Pipeline:

                        factoryObj.previousObjs.Add(this);
                        this.nextObj = factoryObj;
                        GetNextObjectFreeInfo();

                        break;

                    case FactoryObjTypes.Factory:

                        factoryObj.previousObjs.Add(this);
                        this.nextObj = factoryObj;
                        GetNextObjectFreeInfo();

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

    protected new void OnDestroy()
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
