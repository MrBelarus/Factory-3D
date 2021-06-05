using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    public static event Action<KeyCode, float> OnKeyPressed;

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

        SaveSystem.instance.SetupGame();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            OnKeyPressed?.Invoke(KeyCode.F, Time.unscaledTime);
        }

        //for tests
        if (Input.GetKeyDown(KeyCode.M))
        {
            AudioManager.instance.PlaySound(Sounds.BuyFactoryObjSound, UnityEngine.Random.Range(0.85f, 1.15f));
            CashManager.instance.Earn(1000);
        }
    }
}
