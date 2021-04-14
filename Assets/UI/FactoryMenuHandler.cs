using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryMenuHandler : MonoBehaviour
{
    [SerializeField] private Text factoryName;
    [SerializeField] private Text materialsInQueueCount;
    [SerializeField] private Text itemToProduceName;
    [SerializeField] private Image itemToProduceImage;

    private Factory factory;

    public void SetUpMenu(Factory factory)
    {
        factoryName.text = factory.prefabName;
        materialsInQueueCount.text = factory.MaterialsAmountIn.ToString();
        itemToProduceName.text = factory.whatNeedToProduce.name;
        itemToProduceImage.sprite = Resources.Load<Sprite>("UISprites/" + factory.whatNeedToProduce.name);

        this.factory = factory;
    }

    public void ChangeItemToProduce(SellObject sellObject)
    {
        itemToProduceImage.sprite = Resources.Load<Sprite>("UISprites/" + sellObject.name);
        itemToProduceName.text = sellObject.name;
        factory.whatNeedToProduce = sellObject;
    }

    public void ClearFactoryQueue() //on Button event
    {
        if (factory == null)
        {
            Debug.LogError("UI Factory handler doesn't have factory!");
        }

        factory.ClearFactoryQueue();
        materialsInQueueCount.text = factory.MaterialsAmountIn.ToString();
    }
}
