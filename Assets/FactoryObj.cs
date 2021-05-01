using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FactoryObj : MonoBehaviour
{
    public FactoryObjTypes type;

    public string prefabName;   //setups in editor for each obj
    public int cost;

    [HideInInspector] public FactoryObj nextObj;
    [HideInInspector] public List<FactoryObj> previousObjs = new List<FactoryObj>();
    [HideInInspector] public bool isNextObjFree = true;

    public float ObjectMoveTime = 5f;

    virtual public bool IsFree { get; set; }

    [SerializeField] private GameObject[] directionArrows;
    public bool DirectionArrows
    {
        set
        {
            for (int i = 0; i < directionArrows.Length; i++)
            {
                directionArrows[i].SetActive(value);
            }
        }
    }

    protected void Awake()
    {
        if (directionArrows == null || directionArrows.Length == 0)
        {
            Debug.LogError("Arrows isn't setup on " + type + "!");
        }

        ObjectsHolder.instance.AddObject(this);
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

                    if (nextObj.type == FactoryObjTypes.Factory
                        || nextObj.type == FactoryObjTypes.Pipeline)
                    {
                        nextObj.previousObjs.RemoveAt(i);
                    }

                    break;
                }
            }
        }
    }

    protected void OnDestroy()
    {
        ObjectsHolder.instance.RemoveObject(this);
    }
}

[System.Serializable]
public enum FactoryObjTypes { Pipeline, Factory, Purchaser, SellPort };

