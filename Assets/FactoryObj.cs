using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryObj : MonoBehaviour
{
    public FactoryObjTypes type;

    public float cost;

    public bool isNextObjFree = true;
    public float ObjectMoveTime = 5f;

    virtual public bool IsFree { get; set; }
}

public enum FactoryObjTypes { Pipeline, Factory, Purchaser, Seller };

