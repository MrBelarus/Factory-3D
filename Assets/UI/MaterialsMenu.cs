using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaterialsMenu : MonoBehaviour
{
    [Header("Item To Buy Vars")]
    [SerializeField] private RectTransform ContentRect;
    [SerializeField] private int topOffset = 75;
    [SerializeField] private int offsetBetweenItemsY = 100;
    [SerializeField] private int offsetBetweenItemAndCenter = 180;
    [SerializeField] private int itemSquareSide = 250;

    public List<Button> buttons;

    public void UpdateButtons(List<Materials> materials)
    {
        if (buttons.Count > 0)
        {
            foreach (Button button in buttons)
            {
                Destroy(button.gameObject);
            }
            buttons.Clear();
        }

        //calculating content size
        ContentRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x,
            topOffset + (itemSquareSide + offsetBetweenItemsY) * ((materials.Count + 1) / 2));

        for (int i = 0; i < materials.Count; i++)
        {
            GameObject obj = Instantiate(PrefabsContainer.instance.UISellObjPrefabs[materials[i].ToString()], 
                ContentRect);

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

            buttons.Add(obj.GetComponent<Button>());
        }
    }
}
