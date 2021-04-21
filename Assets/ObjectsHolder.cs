﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsHolder : MonoBehaviour //will keep factory game objs data
{
    public static ObjectsHolder instance;

    public List<FactoryObj> factoryObjs = new List<FactoryObj>();
    public List<SellObject> sellObjs = new List<SellObject>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private bool arrowsOn = false;
    /// <summary>
    /// Enables or disables direction arrows for each factory object
    /// </summary>
    public bool DirectionArrows
    {
        set 
        {
            foreach (FactoryObj factoryObj in factoryObjs)
            {
                factoryObj.DirectionArrows = value;
            }

            arrowsOn = value;
        }
        get { return arrowsOn; }
    }

    private void Start()
    {
        DirectionArrows = false;
    }

    //private void Update()
    //{
    //    if (Input.GetKeyDown(KeyCode.A))
    //    {
    //        DirectionArrows = !arrowsOn;
    //    }
    //}

    public void AddObject(FactoryObj obj)
    {
        factoryObjs.Add(obj);
    }

    public void AddObject(SellObject obj)
    {
        sellObjs.Add(obj);
    }

    public void RemoveObject(FactoryObj obj)
    {
        factoryObjs.Remove(obj);
    }

    public void RemoveObject(SellObject obj)
    {
        sellObjs.Remove(obj);
    }
}
