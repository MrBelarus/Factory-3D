using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PurchaserMenuHandler : MonoBehaviour
{
    [SerializeField] private Text itemToBuyName;
    [SerializeField] private Image itemToBuyImage;
    [SerializeField] private Text BoughtObjsQueue;
    [SerializeField] private GameObject buyButtons;
    [SerializeField] private Image AutoBuyCheckMark;
    [SerializeField] private Sprite questionSprite;

    private Purchaser purchaser;

    public void SetUpMenu(Purchaser purchaser)
    {
        //factoryName.text = factory.prefabName;
        //materialsInQueueCount.text = factory.MaterialsAmountIn.ToString();
        //itemToProduceName.text = factory.whatNeedToProduce.name;
        //itemToProduceImage.sprite = Resources.Load<Sprite>("UISprites/" + factory.whatNeedToProduce.name);

        itemToBuyName.text = purchaser.itemToPurchase == null ? "Nothing" : purchaser.itemToPurchase.name;
        itemToBuyImage.sprite = purchaser.itemToPurchase == null ? questionSprite : 
            Resources.Load<Sprite>("UISprites/" + purchaser.itemToPurchase.name);

        buyButtons.SetActive(purchaser.itemToPurchase != null);
        AutoBuyCheckMark.gameObject.SetActive(purchaser.AutoBuy);
        BoughtObjsQueue.text = purchaser.PurchaseQueueCount.ToString();

        this.purchaser = purchaser;
    }

    public void AddItemToQueue()
    {
        if (purchaser.itemToPurchase == null)
        {
            Debug.LogError("Can't add empty material!");
        }

        purchaser.BuyMaterial(purchaser.itemToPurchase);
    }

    public void ChangeItemToPurchase(SellObject sellObject)
    {
        //itemToProduceImage.sprite = Resources.Load<Sprite>("UISprites/" + sellObject.name);
        //itemToProduceName.text = sellObject.name;
        //factory.whatNeedToProduce = sellObject;

        purchaser.itemToPurchase = sellObject.gameObject;
    }

    public void ClearPurchaseQueue() //on Button event
    {
        purchaser.ClearPurchaseQueue();
    }
}
