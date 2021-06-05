using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class SaveSystem : MonoBehaviour
{
    public static SaveSystem instance;

    private SaveData data;
    public SaveData Data { get => data; }
    private string currentSavePath = null;

    private bool newGame = false;
    public bool IsNewGame { get => newGame; }

    public LayerMask factoryLayer;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        data = new SaveData();

        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.G))
        {
            SaveGame();
        }
    }

    public void LoadGame(string fileName = "16024203819.ftr")
    {
        //means there is no saves -> new game
        if (fileName == "")
        {
            newGame = true;

            System.DateTime currentTime = System.DateTime.Now;

            string filePath = Application.persistentDataPath + "/" + currentTime.Day + "-" + currentTime.Month +
                "-" + currentTime.Year + " & " + currentTime.Hour + "h " +
                currentTime.Minute + "m " + currentTime.Second + "s.ftr";
            FileStream file = File.Create(filePath);

            currentSavePath = filePath;

            data = new SaveData();
            BinaryFormatter bf = new BinaryFormatter();

            bf.Serialize(file, data);


            file.Close();
        }
        else
        {
            newGame = false;

            //load existing file save
            currentSavePath = Application.persistentDataPath + "/" + fileName;

            FileStream file = File.Open(currentSavePath, FileMode.Open);

            BinaryFormatter bf = new BinaryFormatter();

            data = (SaveData)bf.Deserialize(file);
            file.Close();
        }
    }

    public void SaveGame()
    {
        if (currentSavePath == null)
        {
            Debug.LogError("Missing current Save path!");
        }

        RefreshDataBeforeSave();

        FileStream file = File.Open(currentSavePath, FileMode.Open);
        BinaryFormatter bf = new BinaryFormatter();

        bf.Serialize(file, data);
        file.Close();

        //rename file
        System.DateTime currentTime = System.DateTime.Now;

        string newFileName = Application.persistentDataPath + "/" + currentTime.Day + "-" + currentTime.Month +
                "-" + currentTime.Year + " & " + currentTime.Hour + "h " +
                currentTime.Minute + "m " + currentTime.Second + "s.ftr";
        File.Move(file.Name, newFileName);
        currentSavePath = newFileName;
    }

    /// <summary>
    /// returns all files with .ftr extention in the Application persistent data path sorted descending
    /// </summary>
    /// <returns></returns>
    public string[] GetSaveNames()
    {
        DirectoryInfo saveDir = new DirectoryInfo(Application.persistentDataPath);
        FileInfo[] saveFiles = saveDir.GetFiles("*.ftr"); //Getting Text files

        string[] fileNames = new string[saveFiles.Length];
        for (int i = 0; i < saveFiles.Length; i++)
        {
            fileNames[i] = saveFiles[saveFiles.Length - 1 - i].Name;
        }

        return fileNames;
    }

    /// <summary>
    /// refreshes the game data for the save file
    /// </summary>
    private void RefreshDataBeforeSave()
    {
        //save all factory objs
        ObjectsHolder sceneObjs = ObjectsHolder.instance;


        //clear all old save objs
        data.factoryObjsData = new FactoryObjectData[sceneObjs.factoryObjs.Count];
        data.sellObjsData = new SellObjectData[sceneObjs.sellObjs.Count];

        data.achievements = AchievementController.instance.achievements;
        data.money = CashManager.instance.Money;
        data.gameVersion = Application.version;

        RefreshFactoryObjsData(sceneObjs.factoryObjs);
        RefreshSellObjsData(sceneObjs.sellObjs);
    }

    /// <summary>
    /// Loads all necessary info from data [data was init with save file]
    /// </summary>
    public void SetupGame()
    {
        CashManager.instance.Money = data.money;

        if (data.factoryObjsData != null)
        {
            SetupFactoryObjs();
        }

        if (data.sellObjsData != null)
        {
            SetupSellObjs();
        }
    }

    public void DeleteSaveFile(string saveName = null)
    {
        if (saveName == null)
        {
            File.Delete(currentSavePath);
        }
        else
        {
            File.Delete(Application.persistentDataPath + "/" + saveName);
        }
    }

    private void EnableFactoryObjColliders(GameObject objToReplace)
    {
        Collider mainObjCol = objToReplace.GetComponent<Collider>();

        if (mainObjCol)
        {
            if (mainObjCol.enabled)
            {
                //it's not a factory obj, because just they have disables colliders on spawn!
                return;
            }

            mainObjCol.enabled = true;
        }

        Collider[] colliders = objToReplace.transform.Find("base").GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }

    private void RefreshFactoryObjsData(List<FactoryObj> factoryObjs)
    {
        for (int i = 0; i < factoryObjs.Count; i++)
        {
            FactoryObj factoryObj = factoryObjs[i];
            Vector3 pos = factoryObj.transform.position;
            Quaternion rot = factoryObj.transform.rotation;

            Materials itemToBuy = Materials.Undefined;
            Materials itemToProduce = Materials.Undefined;
            bool AutoBuy = false;
            Materials[] queue = null;
            float buyTimer = 0f;

            if (factoryObj.type == FactoryObjTypes.Purchaser)
            {
                Purchaser purchaser = (Purchaser)factoryObj;
                if (purchaser.itemToPurchase)
                {
                    itemToBuy = purchaser.itemToPurchase.material;
                }
                AutoBuy = purchaser.AutoBuy;
                queue = purchaser.MaterialsQueue;
                buyTimer = purchaser.BuyTimer;
            }
            else if (factoryObj.type == FactoryObjTypes.Factory)
            {
                Factory factory = (Factory)factoryObj;
                if (factory.WhatNeedToProduce)
                {
                    itemToProduce = factory.WhatNeedToProduce.material;
                }

                if (factory.IsBusy)
                {
                    queue = factory.WhatNeedToProduce.canBeCreatedWith.ToArray();
                }
            }

            data.factoryObjsData[i] = new FactoryObjectData()
            {
                itemToBuy = itemToBuy,
                itemToProduce = itemToProduce,
                prefabName = factoryObj.prefabName,
                position = new float[3] { pos.x, pos.y, pos.z },
                rotation = new float[4] { rot.x, rot.y, rot.z, rot.w },
                autoBuy = AutoBuy,
                queue = queue,
                buyTimer = buyTimer,
            };
        }
    }

    private void RefreshSellObjsData(List<SellObject> sellObjects)
    {
        for (int i = 0; i < sellObjects.Count; i++)
        {
            SellObject sellObj = sellObjects[i];
            Vector3 pos = sellObj.transform.position;
            Quaternion rot = sellObj.transform.rotation;


            float[] dir = null;
            int deliverIndex = -1;
            bool isMoving = false;
            if (sellObj.deliver)
            {
                Vector3 direction = sellObj.direction * (1 - sellObj.deliveredTimeProportion);
                dir = new float[] { direction.x, direction.y, direction.z };

                isMoving = true;

                deliverIndex = ObjectsHolder.instance.GetIndexOf(sellObj.deliver);
            }
            else
            {
                Collider[] overlaps = Physics.OverlapBox(sellObj.transform.position, 
                    Vector3.one / 2.05f, Quaternion.identity, factoryLayer);

                if (overlaps.Length > 0)
                {
                    //берем фабрику что под SellObj чтобы привязать потом его к фабрике после загрузки
                    deliverIndex = 
                        ObjectsHolder.instance.GetIndexOf(overlaps[0].transform.root.GetComponent<FactoryObj>());

                    isMoving = false;
                }
            }

            data.sellObjsData[i] = new SellObjectData()
            {
                position = new float[3] { pos.x, pos.y, pos.z },
                rotation = new float[4] { rot.x, rot.y, rot.z, rot.w },
                cost = sellObj.cost,
                isMoving = isMoving,
                directionMove = dir,
                factoryIndex = deliverIndex,
                deliverTime = sellObj.deliverTimeTotal,
                passedWayToDeliver = sellObj.deliveredTimeProportion,
                prefabName = sellObj.material.ToString(),
                material = sellObj.material,
            };
        }
    }

    private void SetupFactoryObjs()
    {
        int savedObjectsCount = data.factoryObjsData.Length;
        float[] rot;
        float[] pos;
        FactoryObjectData[] factories = data.factoryObjsData;

        for (int i = 0; i < savedObjectsCount; i++)
        {
            rot = factories[i].rotation;
            pos = factories[i].position;

            GameObject prefab = 
                PrefabsContainer.instance.FactoryObjectPrefabs[factories[i].prefabName].gameObject;

            GameObject obj = Instantiate(prefab);
            obj.transform.rotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
            obj.transform.position = new Vector3(pos[0], pos[1], pos[2]);

            FactoryObjectData factoryObjData = factories[i];

            if (prefab.name == "Purchaser" && factoryObjData.itemToBuy != Materials.Undefined)
            {
                Purchaser purchaser = obj.GetComponent<Purchaser>();
                purchaser.itemToPurchase =
                    PrefabsContainer.instance.SellObjectPrefabs[factoryObjData.itemToBuy];
                purchaser.AutoBuy = factoryObjData.autoBuy;
                purchaser.BuyTimer = factoryObjData.buyTimer;

                Materials[] queue = factoryObjData.queue;
                for (int k = 0; k < queue.Length; k++)
                {
                    purchaser.PurchaseQueue.Enqueue(
                        PrefabsContainer.instance.SellObjectPrefabs[queue[k]]);
                }
            }

            Factory factory = obj.GetComponent<Factory>();
            if (factory)
            {
                if (factoryObjData.itemToProduce != Materials.Undefined)
                {
                    factory.WhatNeedToProduce =
                        PrefabsContainer.instance.SellObjectPrefabs[factoryObjData.itemToProduce];
                }

                Materials[] queue = factoryObjData.queue;
                if (queue != null)
                {
                    for (int k = 0; k < queue.Length; k++)
                    {
                        factory.Container.Add(
                            PrefabsContainer.instance.SellObjectPrefabs[queue[k]]);
                    }
                    factory.CheckIsReadyToProcess(true);
                }
            }

            EnableFactoryObjColliders(obj);
        }
    }

    private void SetupSellObjs()
    {
        int savedObjectsCount = data.sellObjsData.Length;
        SellObjectData[] sellObjects = data.sellObjsData;

        float[] rot;
        float[] pos;

        for (int i = 0; i < savedObjectsCount; i++)
        {
            rot = sellObjects[i].rotation;
            pos = sellObjects[i].position;

            GameObject prefab = PrefabsContainer.instance.
                SellObjectPrefabs[sellObjects[i].material].gameObject;

            GameObject obj = Instantiate(prefab);
            obj.transform.rotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
            obj.transform.position = new Vector3(pos[0], pos[1], pos[2]);

            SellObjectData data = sellObjects[i];
            SellObject sellObject = obj.GetComponent<SellObject>();

            if (data.isMoving)
            {
                float[] dir = data.directionMove;
                Vector3 direction = new Vector3(dir[0], dir[1], dir[2]);

                FactoryObj deliver = ObjectsHolder.instance.factoryObjs[data.factoryIndex];
                sellObject.MoveTo(direction, deliver, data.deliverTime * (1 - data.passedWayToDeliver));
            }
            else
            {
                FactoryObj parent = ObjectsHolder.instance.factoryObjs[data.factoryIndex];

                switch (parent.type)
                {
                    case FactoryObjTypes.Pipeline:
                        Pipeline pipeline = (Pipeline)parent;
                        pipeline.itemToMove = sellObject;
                        break;

                    case FactoryObjTypes.Factory:
                        Factory factory = (Factory)parent;
                        factory.ProcessResultObj = sellObject.gameObject;
                        break;

                    case FactoryObjTypes.Purchaser:
                        Purchaser purchaser = (Purchaser)parent;
                        purchaser.PurchasedItem = sellObject.gameObject;
                        break;
                }
            }
        }
    }
}

[System.Serializable]
public class SaveData
{
    public int money = 1000;
    public string gameVersion = "";

    public Achievement[] achievements = null;

    public FactoryObjectData[] factoryObjsData;
    public SellObjectData[] sellObjsData;
}

[System.Serializable]
public class SaveObject
{
    public float[] rotation;
    public float[] position;

    public string prefabName = null;
}

[System.Serializable]
public class FactoryObjectData : SaveObject
{
    public bool autoBuy = false;
    public Materials itemToBuy = Materials.Undefined;
    public Materials itemToProduce = Materials.Undefined;
    public Materials[] queue = null;
    public float buyTimer = 0;
}

[System.Serializable]
public class SellObjectData : SaveObject
{
    public int cost = 0;

    public float[] directionMove;
    public int factoryIndex;
    public Materials material;

    public bool isMoving = false;           //if true, 2 vars below need to be init, 
                                            //else change FactoryObj itemHolder by factoryObjType

    public float deliverTime = 0f;          //time to pass way (was init by FactoryObj)
    public float passedWayToDeliver = 0f;   //[0..1]
}
