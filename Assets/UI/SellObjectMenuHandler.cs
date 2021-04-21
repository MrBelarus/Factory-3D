using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SellObjectMenuHandler : MonoBehaviour
{
    [SerializeField] private Text sellObjName;
    [SerializeField] private Image sellObjImage;
    [SerializeField] private Text sellObjCost;

    public void SetUpMenu(SellObject sellObject)
    {
        sellObjName.text = sellObject.material.ToString();
        sellObjImage.sprite = Resources.Load<Sprite>("UISprites/" + sellObject.material);
        sellObjCost.text = "Cost: " + sellObject.cost.ToString() + "$";
    }
}
