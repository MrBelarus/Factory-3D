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

    [SerializeField] private Sprite questionSprite;

    [SerializeField] private MaterialsMenu materialsMenu;

    private Factory factory;

    public void SetUpMenu(Factory factory)
    {
        factoryName.text = factory.prefabName;
        materialsInQueueCount.text = factory.ContainerItemsAmount.ToString();

        itemToProduceName.text = factory.WhatNeedToProduce == null ? "Nothing" : factory.WhatNeedToProduce.name;
        itemToProduceImage.sprite = factory.WhatNeedToProduce == null ? questionSprite :
            PrefabsContainer.instance.SellObjSpritePrefabs[itemToProduceName.text];

        if (factory.WhatNeedToProduce != null && itemToProduceImage.sprite == null)
        {
            Debug.LogError("Image was't found!");
        }

        this.factory = factory;
    }

    public void OpenFactoryObjectsMenu() //вызов метода по клику на кнопку "Item To Produce"
    {
        materialsMenu.UpdateButtons(factory.AvailableMaterialsToProduce);

        List<Button> buttons = materialsMenu.buttons;

        for (int i = 0; i < buttons.Count; i++)
        {
            int copy = i;   //event arguments fix
            buttons[i].onClick.AddListener(() => { ChangeItemToProduce(copy); });
        }

        materialsMenu.gameObject.SetActive(true);
        gameObject.SetActive(false);
    }

    public void ChangeItemToProduce(int materialIndex)
    {
        SellObject sellObject = PrefabsContainer.instance.
            SellObjectPrefabs[factory.AvailableMaterialsToProduce[materialIndex]];

        if (sellObject == null)
        {
            Debug.LogError("Object to produce wasn't found in the resource folder." +
                " Maybe you should change prefab name or add it to enum names!");
        }

        factory.WhatNeedToProduce = sellObject;
        itemToProduceImage.sprite = PrefabsContainer.instance.SellObjSpritePrefabs[sellObject.name];
        itemToProduceName.text = sellObject.name;


        //fix when pipeline or smth waiting because next obj isn't free
        foreach (FactoryObj factoryObj in factory.previousObjs)
        {
            factoryObj.isNextObjFree = factory.IsFree;
        }

        materialsMenu.gameObject.SetActive(false);
        gameObject.SetActive(true);
    }

    public void ClearFactoryQueue() //on Button event
    {
        if (factory == null)
        {
            Debug.LogError("UI Factory handler doesn't have factory!");
        }

        factory.ClearFactoryQueue();
        materialsInQueueCount.text = factory.ContainerItemsAmount.ToString();
    }
}
