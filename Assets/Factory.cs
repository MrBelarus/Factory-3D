using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Factory : FactoryObj
{
    //on factory object [Bake|Composer|Drill Machine] produces itemToProduce event
    public static event Action<SellObject> OnFactoryObjProduced = delegate { };

    public List<Materials> AvailableMaterialsToProduce;
    public SellObject whatNeedToProduce;//what we should get after process iterations

    [SerializeField] private ParticleSystem workParticles;
    [SerializeField] private Animator workAnimation;

    private GameObject processResult;
    public Transform processedObjSpawnPoint;

    private bool itemToProduceWasChanged = false;   //IMPLEMENT WHEN WE HAVE USED THE UI CHANGE ITEM

    //container of objects (materials or components) to produce item
    private List<SellObject> factoryContainer = new List<SellObject>();
    public int MaterialsAmountIn { get { return factoryContainer.Count; } }

    private bool isBusy = false;
    private bool readyToProcess = false;
    public override bool IsFree { get { return !isBusy; } }

    protected new void Awake()
    {
        base.Awake();

        if (AvailableMaterialsToProduce == null || AvailableMaterialsToProduce.Count == 0)
        {
            Debug.LogError("Need to setup materials to produce in " + this.name);
        }
    }

    private void FixedUpdate()
    {
        if (readyToProcess && !isBusy)
        {
            StopAllCoroutines();
            StartCoroutine(ProcessItem());
            readyToProcess = false;
        }

        if(processResult && isNextObjFree)
        {
            SellObject objScript = processResult.GetComponent<SellObject>();
            if (objScript.MoveTo(processedObjSpawnPoint.forward, nextObj, ObjectMoveTime))
            {
                processResult = null;
                factoryContainer.Clear();

                foreach (FactoryObj previousObj in previousObjs)
                {
                    previousObj.isNextObjFree = true;
                }
            }
            else
            {
                isNextObjFree = false;
            }
        }
    }

    IEnumerator ProcessItem()
    {
        isBusy = true;

        if (workParticles)
        {
            var main = workParticles.main;
            main.loop = true;
            workParticles.Play();
        }
        if (workAnimation)
        {
            workAnimation.enabled = true;
        }

        yield return new WaitForSeconds(whatNeedToProduce.timeToProduce);

        //проверки на комбинацию из чего формируется что будет в result
        GameObject result = Resources.Load(whatNeedToProduce.name) as GameObject;

        processResult = Instantiate(result, processedObjSpawnPoint.position, 
            processedObjSpawnPoint.rotation);

        OnFactoryObjProduced?.Invoke(whatNeedToProduce);

        if (workParticles)
        {
            var main = workParticles.main;
            main.loop = false;
        }
        if (workAnimation)
        {
            workAnimation.enabled = false;
        }

        isNextObjFree = nextObj != null && nextObj.IsFree;

        isBusy = false;
    }

    public void CheckIsReadyToProcess()
    {
        if (factoryContainer.Count == whatNeedToProduce.canBeCreatedWith.Count)
        {
            readyToProcess = true;
        }
    }

    public void AddSellObjToContainer(SellObject senderObj)
    {
        if (itemToProduceWasChanged && 
            whatNeedToProduce.canBeCreatedWith.Count(neededMat => neededMat == senderObj.material) == 0)
        {
            print("ITEM TO PRODUCE WAS CHANGED AND SELLOBJ WAS DELIVERED BADLY SO IT'S REMOVED");
            Destroy(senderObj.gameObject);
            return;
        }

        Destroy(senderObj.gameObject);
        factoryContainer.Add(senderObj);
    }

    public bool CanFactoryGetObj(SellObject senderObj)
    {
        bool isItNeeded = false;

        List<Materials> neededMaterials = new List<Materials>();

        if (whatNeedToProduce)
        {
            neededMaterials = whatNeedToProduce.canBeCreatedWith;
        }

        foreach (Materials neededMaterial in neededMaterials)
        {
            if (senderObj.material == neededMaterial)
            {
                isItNeeded = true;  //нужен ли вообще этот материал
                break;
            }
        }

        if (!isItNeeded)
        {
            //UI Warning message?
            //it's completely incopatible obj
            return false;
        }

        int neededMaterialCraftCount = neededMaterials.Count(material => material == senderObj.material);
        int neededMaterialInFactoryCount = factoryContainer.Count(obj => obj.material == senderObj.material);

        if (neededMaterialCraftCount > neededMaterialInFactoryCount)
        {
            return true;
        }
        return false;
    }

    public void ClearFactoryQueue()
    {
        CashManager cashManager = CashManager.instance;
        foreach (SellObject obj in factoryContainer)
        {
            cashManager.Earn(obj.cost / 2);
        }

        foreach (FactoryObj previousObj in previousObjs)
        {
            previousObj.isNextObjFree = true;
        }

        factoryContainer.Clear();
    }

    //connections implementations will be on OnTrigger events
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
                    factoryObj.previousObjs.Add(this);
                    nextObj = factoryObj;
                    isNextObjFree = factoryObj.IsFree;
                    break;

                case FactoryObjTypes.Factory:
                    factoryObj.previousObjs.Add(this);
                    nextObj = factoryObj;
                    isNextObjFree = factoryObj.IsFree;
                    break;

                case FactoryObjTypes.SellPort:
                    factoryObj.previousObjs[0] = this;
                    nextObj = factoryObj;
                    isNextObjFree = factoryObj.IsFree;
                    break;

                default:
                    Debug.LogError("Smth went wrong with Factory && " + factoryObj.type.ToString());
                    break;
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

    private new void OnDestroy()
    {
        base.OnDestroy();

        DetachNextWithThis();
    }
}
