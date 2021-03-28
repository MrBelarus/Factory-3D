using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Factory : FactoryObj
{
    public SellObject whatNeedToProduce;//what we should get after process iterations
    
    //!!! will changes by UI if we change whatNeedToProduce!
    public int amountOfItemsToProduceOne = 1;   //depends on count of sell object material list to create it!

    private GameObject processResult;
    public Transform processedObjSpawnPoint;

    private bool itemToProduceWasChanged = false;   //IMPLEMENT WHEN WE HAVE USED THE UI CHANGE ITEM

    public FactoryObj nextObj;
    public List<FactoryObj> previousObjs =  new List<FactoryObj>();

    //container of objects (materials or components) to produce item
    private List<SellObject> factoryContainer = new List<SellObject>();

    private bool isBusy = false;
    private bool readyToProcess = false;
    public override bool IsFree { get { return !isBusy; } }

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

        yield return new WaitForSeconds(whatNeedToProduce.timeToProduce);

        //проверки на комбинацию из чего формируется что будет в result
        GameObject result = Resources.Load(whatNeedToProduce.name) as GameObject;

        processResult = Instantiate(result, processedObjSpawnPoint.position, 
            processedObjSpawnPoint.rotation);

        isNextObjFree = nextObj != null && nextObj.IsFree;

        isBusy = false;
    }

    public void CheckIsReadyToProcess()
    {
        if (factoryContainer.Count == amountOfItemsToProduceOne)
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

        List<Materials> neededMaterials = whatNeedToProduce.canBeCreatedWith;

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
                    Pipeline pipeline = (Pipeline)factoryObj;
                    pipeline.previousObj = this;
                    nextObj = pipeline;
                    isNextObjFree = pipeline.IsFree;
                    break;

                case FactoryObjTypes.Factory:
                    Factory factory = (Factory)factoryObj;
                    factory.previousObjs.Add(this);
                    nextObj = factory;
                    isNextObjFree = factory.IsFree;
                    break;

                case FactoryObjTypes.Seller:
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
}
