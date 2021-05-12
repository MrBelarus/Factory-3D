using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PrefabsContainer : MonoBehaviour
{
    public static PrefabsContainer instance;

    public Dictionary<Materials, SellObject> SellObjectPrefabs
        = new Dictionary<Materials, SellObject>();

    public Dictionary<string, Sprite> SellObjSpritePrefabs 
        = new Dictionary<string, Sprite>();

    public Dictionary<string, GameObject> UISellObjPrefabs 
        = new Dictionary<string, GameObject>();

    public Dictionary<string, FactoryObj> FactoryObjectPrefabs 
        = new Dictionary<string, FactoryObj>();

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            LoadPrefabs();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        DontDestroyOnLoad(gameObject);
    }

    private void LoadPrefabs()
    {
        GameObject[] factories = Resources.LoadAll<GameObject>("Factory");
        foreach (GameObject factory in factories)
        {
            FactoryObj factoryObj = factory.GetComponent<FactoryObj>();
            FactoryObjectPrefabs.Add(factoryObj.prefabName, factoryObj);
        }

        GameObject[] sellObjs = Resources.LoadAll<GameObject>("SellObject");
        foreach (GameObject sellObj in sellObjs)
        {
            SellObject sellObject = sellObj.GetComponent<SellObject>();
            SellObjectPrefabs.Add(sellObject.material, sellObject);
        }

        Sprite[] sellObjSprites = Resources.LoadAll<Sprite>("UISprites");
        foreach (Sprite sellObjSprite in sellObjSprites)
        {
            SellObjSpritePrefabs.Add(sellObjSprite.name, sellObjSprite);
        }

        GameObject[] uiSellObjs = Resources.LoadAll<GameObject>("UISprites");
        foreach (GameObject uiSellObj in uiSellObjs)
        {
            UISellObjPrefabs.Add(uiSellObj.name, uiSellObj);
        }
    }
}
