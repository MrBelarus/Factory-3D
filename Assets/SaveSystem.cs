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

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        data = new SaveData();

        //LoadGame("24-4-2021 & 22h 19m 0s.ftr");

        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        //SaveGame();

        foreach(string saveName in GetSaveNames())
        {
            print(saveName);
        }
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

            //SetupGame();

            print(data.money + " " + data.saveObjects.Count);
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
        List<FactoryObj> factoryObjs = sceneObjs.factoryObjs;

        //clear all old save objs
        data.saveObjects = new List<SaveObject>();

        for (int i = 0; i < factoryObjs.Count; i++)
        {
            FactoryObj factoryObj = factoryObjs[i];
            Vector3 pos = factoryObj.transform.position;
            Quaternion rot = factoryObj.transform.rotation;

            string itemToBuy = null;
            string itemToProduce = null;
            bool AutoBuy = false;

            if (factoryObj.type == FactoryObjTypes.Purchaser)
            {
                Purchaser purchaser = (Purchaser)factoryObj;
                if (purchaser.itemToPurchase)
                {
                    itemToBuy = purchaser.itemToPurchase.material.ToString();
                }
                AutoBuy = purchaser.AutoBuy;
            }
            else if (factoryObj.type == FactoryObjTypes.Factory)
            {
                Factory factory = (Factory)factoryObj;
                if (factory.whatNeedToProduce)
                {
                    itemToProduce = factory.whatNeedToProduce.name;
                }
            }

            data.saveObjects.Add(new SaveObject()
            {
                itemToBuyPrefabName = itemToBuy,
                itemToProducePrefabName = itemToProduce,
                prefabName = factoryObj.prefabName,
                position = new float[3] { pos.x, pos.y, pos.z },
                rotation = new float[4] { rot.x, rot.y, rot.z, rot.w },
                autoBuy = AutoBuy
            });
        }

        data.money = CashManager.instance.Money;
    }

    /// <summary>
    /// Loads all necessary info from data [data was init with save file]
    /// </summary>
    public void SetupGame()
    {
        CashManager.instance.Money = data.money;

        if (data.saveObjects == null)
        {
            return;
        }

        GameObject[] prefabs = Resources.LoadAll<GameObject>("Factory");

        int savedObjectsCount = data.saveObjects.Count;
        float[] rot;
        float[] pos;
        SaveObject[] saveObjects = data.saveObjects.ToArray();

        for (int i = 0; i < savedObjectsCount; i++)
        {
            rot = saveObjects[i].rotation;
            pos = saveObjects[i].position;

            GameObject prefab = null;
            for (int j = 0; j < prefabs.Length; j++)
            {
                if (prefabs[j].name == saveObjects[i].prefabName)
                {
                    prefab = prefabs[j];
                }
            }

            GameObject obj = Instantiate(prefab);
            obj.transform.rotation = new Quaternion(rot[0], rot[1], rot[2], rot[3]);
            obj.transform.position = new Vector3(pos[0], pos[1], pos[2]);

            if (prefab.name == "Purchaser" && saveObjects[i].itemToBuyPrefabName != null)
            {
                Purchaser purchaser = obj.GetComponent<Purchaser>();
                purchaser.itemToPurchase = Resources.Load<SellObject>(saveObjects[i].itemToBuyPrefabName);
                purchaser.AutoBuy = saveObjects[i].autoBuy;
            }

            Factory factory = obj.GetComponent<Factory>();
            if (factory)
            {
                factory.whatNeedToProduce = Resources.Load<SellObject>(saveObjects[i].itemToProducePrefabName);
            }

            EnableFactoryObjColliders(obj);
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
}


[System.Serializable]
public class SaveData
{
    public int money = 1000;

    public List<SaveObject> saveObjects;
    public List<int> unlockedAchievementsIds;
}

[System.Serializable]
public class SaveObject
{
    public float[] rotation;
    public float[] position;

    public string prefabName = null;

    public bool autoBuy = false;
    public string itemToBuyPrefabName = null;
    public string itemToProducePrefabName = null;
}
