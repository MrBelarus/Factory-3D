using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FactoryObjsShopHandler : MonoBehaviour
{
    [SerializeField] private GameObject [] factoryObjsToDisplay;
    //need to set it in the same order as objs UI!
    [SerializeField] private FactoryObj [] factoryObjsPrefabScripts;

    [SerializeField] private RectTransform ContentRect;
    [SerializeField] private int topOffset = 75;
    [SerializeField] private int offsetBetweenItemsY = 100;
    [SerializeField] private int offsetBetweenItemAndCenter = 180;
    [SerializeField] private int itemSquareSide = 250;

    private Button[] buttons;

    private void Awake()
    {
        buttons = new Button[factoryObjsToDisplay.Length];

        //check relations and get button components
        for (int i = 0; i < factoryObjsToDisplay.Length; i++)
        {
            if (factoryObjsToDisplay[i].name != factoryObjsPrefabScripts[i].name)
            {
                Debug.LogError("wrong setup: " + 
                    factoryObjsToDisplay[i].name + "!=" + factoryObjsPrefabScripts[i].name
                    + "\nAll needs to be in the same order!");
            }
        }
    }

    private void Start()
    {
        GameUIManager gameUIManager = GameUIManager.instance;

        if (gameUIManager == null)
        {
            Debug.LogError("Game UI Manager wasn't found!");
            return;
        }

        if (factoryObjsToDisplay.Length > 0)
        {
            //calculating content size
            ContentRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x,
                topOffset + (itemSquareSide + offsetBetweenItemsY) * ((factoryObjsToDisplay.Length + 1) / 2));

            for (int i = 0; i < factoryObjsToDisplay.Length; i++)
            {
                GameObject obj = Instantiate(factoryObjsToDisplay[i], ContentRect);

                RectTransform rectTransform = obj.GetComponent<RectTransform>();
                rectTransform.sizeDelta = new Vector2(itemSquareSide, itemSquareSide);

                if (i % 2 == 0)
                {
                    rectTransform.anchoredPosition = new Vector2(-offsetBetweenItemAndCenter,
                        -(topOffset + itemSquareSide / 2 + i / 2 * (itemSquareSide + offsetBetweenItemsY)));
                }
                else
                {
                    rectTransform.anchoredPosition = new Vector2(offsetBetweenItemAndCenter,
                        -(topOffset + itemSquareSide / 2 + i / 2 * (itemSquareSide + offsetBetweenItemsY)));
                }

                obj.transform.Find("Cost").GetComponent<Text>().text = factoryObjsPrefabScripts[i].cost + "$";


                //Setup button listener
                buttons[i] = obj.GetComponent<Button>();

                int copy = i;   //event arguments fix
                buttons[i].onClick.AddListener(() =>
                {
                    gameUIManager.OnFactoryObjBuyClick(factoryObjsPrefabScripts[copy]);
                });
            }
        }
        else
        {
            Debug.LogError("There is no factory objects to play with!");
        }
    }
}
