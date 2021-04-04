using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryObj : MonoBehaviour
{
    public FactoryObjTypes type;

    public string prefabName;   //takes enum [type] in Start method
    public float cost;
    public FactoryObj nextObj;
    public List<FactoryObj> previousObjs = new List<FactoryObj>();

    public bool isNextObjFree = true;
    public float ObjectMoveTime = 5f;

    virtual public bool IsFree { get; set; }

    private void Start()
    {
        prefabName = type.ToString();
    }

    public void DetachNextWithThis()
    {
        if (nextObj)
        {
            int count = nextObj.previousObjs.Count;
            for (int i = 0; i < count; i++)
            {
                if (nextObj.previousObjs[i] == this)
                {
                    nextObj.previousObjs[i] = null;

                    if (nextObj.type == FactoryObjTypes.Factory)
                    {
                        nextObj.previousObjs.RemoveAt(i);
                    }

                    print("ASDASDWAE21312421" + "|||  " + i);
                    break;
                }
            }
        }
    }
}

public enum FactoryObjTypes { Pipeline, Factory, Purchaser, SellPort };

