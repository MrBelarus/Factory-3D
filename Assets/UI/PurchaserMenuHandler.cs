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

    [SerializeField] private MaterialsMenu materialsMenu;

    private Purchaser purchaser;

    public void SetUpMenu(Purchaser purchaser)
    {
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
        BoughtObjsQueue.text = purchaser.PurchaseQueueCount.ToString();
    }

    public void TurnOnAutoBuy()
    {
        purchaser.AutoBuy = !purchaser.AutoBuy;

        if (purchaser.AutoBuy)
        {
            AutoBuyCheckMark.gameObject.SetActive(true);
        }
        else
        {
            AutoBuyCheckMark.gameObject.SetActive(false);
        }
    }

    public void OpenPurchaserItemsMenu()    //вызов метода по клику на кнопку "Item To Buy"
    {
        materialsMenu.UpdateButtons(purchaser.AvailableMaterials);

        List<Button> buttons = materialsMenu.buttons;

        for (int i = 0; i < buttons.Count; i++)
        {
            int copy = i;   //event arguments fix
            buttons[i].onClick.AddListener(() => { ChangeItemToPurchase(copy); });
        }

        materialsMenu.gameObject.SetActive(true);

        //hide purchaser menu
        gameObject.SetActive(false);
    }

    public void ChangeItemToPurchase(int materialIndex)
    {
        SellObject sellObject = Resources.Load<SellObject>(purchaser.AvailableMaterials[materialIndex].ToString());
        purchaser.itemToPurchase = sellObject;

        buyButtons.SetActive(true);
        AutoBuyCheckMark.gameObject.SetActive(purchaser.AutoBuy);

        itemToBuyName.text = purchaser.itemToPurchase.name;
        itemToBuyImage.sprite = Resources.Load<Sprite>("UISprites/" + purchaser.itemToPurchase.name);

        materialsMenu.gameObject.SetActive(false);
        
        //show purchaser menu
        gameObject.SetActive(true);
    }

    public void ClearPurchaseQueue() //on Button event
    {
        purchaser.ClearPurchaseQueue();
        BoughtObjsQueue.text = "0";
    }
}
