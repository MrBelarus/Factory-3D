using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Stages
{
    fromPurchaser,
    processed,
    ready,
    readyWithImprovements
};
public enum Materials   //fromPurchaser stage 
{
    Undefined,
    Plastic,		
    Water,			//model
    IronOre,		
    Sand,			
    Rubber,			
    Bottle,			//model
    GlassBottle,	
    Glass,			
    Metal,			
    Gear,			
};
public enum FinalItems   //finish processed stage 
{
    Undefined,
    Car,			//model
    Soda,			//model
    Clock,			
    Toy,			//model
}

[RequireComponent(typeof(Rigidbody))]
public class SellObject : MonoBehaviour
{
    public Stages stage;
    public Materials material;
    public FinalItems? finalItem;

    public List<Materials> canBeCreatedWith;

    [HideInInspector] public Transform transformComponent;
    public float cost;
    public float timeToProduce;
    private Rigidbody rb;

    public bool isMoving = false;

    private void Awake()
    {
        transformComponent = gameObject.transform;
        rb = GetComponent<Rigidbody>();
    }

    private void OnDestroy()
    {
        StopAllCoroutines();
    }

    /// <summary>
    /// move obj foward 1 unit.
    /// </summary>
    /// <param name="direction"></param>
    /// <param name="deliver"></param>
    /// <param name="transportTime"></param>
    /// <returns>true if it will move & false if it's not possible</returns>
    public bool MoveTo(Vector3 direction, FactoryObj deliver, float transportTime)
    {
        if (!isMoving)
        {
            if (deliver.type == FactoryObjTypes.Factory)
            {
                Factory factory = (Factory)deliver;
                //можем ли мы двигать если там норм шаблон 
                //и мы под него попадаем либо уже есть объект такой
                if(!factory.CanFactoryGetObj(this))
                {
                    return false;
                }
            }
            else if (deliver.type == FactoryObjTypes.Pipeline)
            {
                Pipeline pipeline = (Pipeline)deliver;
                pipeline.isRecievingItem = true;
            }

            StopAllCoroutines();
            StartCoroutine(MoveObj(direction, deliver, transportTime));

            return true;
        }

        return false;
    }

    IEnumerator MoveObj(Vector3 direction, FactoryObj deliver, float time)
    {
        isMoving = true;

        if (time <= 0)
        {
            transform.position += direction;
        }
        else
        {
            Vector3 currentPos = transform.position;
            Vector3 target = transform.position + direction;

            for (float i = 0; i <= 1; i += Time.deltaTime / time)
            {
                rb.MovePosition(Vector3.Lerp(currentPos, target, i));
                yield return new WaitForFixedUpdate();
            }

            transform.position = target;
        }

        //fix if user deletes deliver obj 
        //and sellObj is reached deliver destination
        if (deliver == null)
        {
            Destroy(gameObject);
            yield break;
        }

        //when object was tranported to deliver
        switch (deliver.type)
        {
            case FactoryObjTypes.Pipeline:
                Pipeline pipeline = (Pipeline)deliver;
                pipeline.itemToMove = this;
                pipeline.isRecievingItem = false;
                pipeline.GetNextObjectFreeInfo();
                break;

            case FactoryObjTypes.Factory:
                Factory factory = (Factory)deliver;
                factory.AddSellObjToContainer(this);
                factory.CheckIsReadyToProcess();
                break;

            case FactoryObjTypes.SellPort:
                Seller seller = (Seller)deliver;
                seller.SellObj(this);
                break;
        }

        //print(gameObject.name + " transported");

        isMoving = false;
    }

    //public SellObject Upgrade()
    //{
    //    return null;
    //}

    //public SellObject CombineWith(SellObject obj)
    //{
    //    switch(component)
    //    {
    //        case Components.Bottle:

    //            if (obj.material == Materials.Water)
    //            {
    //                this.component = Components.Undefined;
    //                this.finalItem = FinalItems.Soda;
    //            }

    //            break;
    //        case Components.Gear:

    //            if (obj.)

    //            break;
    //        case Components.Glass:

    //            break;
    //        case Components.GlassBottle:

    //            break;
    //        case Components.Metal:

    //            break;
    //    }

    //    return;
    //}
}
