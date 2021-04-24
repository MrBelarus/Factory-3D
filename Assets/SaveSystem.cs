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

        DontDestroyOnLoad(gameObject);
    }
}

public class SaveData
{
    public int money = 0;

}
