using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

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
    }

    private void Start()
    {
        IronManAchievement ironManAchievement = new IronManAchievement();
        ironManAchievement.SubscribeEvent();
    }

    public void TestListener(SellObject obj)
    {
        print(obj.material + " has been produced!");
    }

    private void OnDestroy()
    {
        Factory.OnFactoryObjProduced -= TestListener;
    }
}
