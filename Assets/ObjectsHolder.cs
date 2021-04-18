using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectsHolder : MonoBehaviour //will keep factory game objs data
{
    public static ObjectsHolder instance;

    List<FactoryObj> factoryObjs = new List<FactoryObj>();

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
        }
    }
}
