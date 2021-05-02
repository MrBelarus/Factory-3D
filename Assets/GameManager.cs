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
        }

        SaveSystem.instance.SetupGame();
        Application.targetFrameRate = 60;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            AudioManager.instance.PlaySound(Sounds.ReplaceSound);
            OnKeyPressed?.Invoke(KeyCode.F, Time.unscaledTime);
        }

        if (Input.GetKeyDown(KeyCode.M))
        {
            AudioManager.instance.PlaySound(Sounds.CantReplaceHere);
            CashManager.instance.Earn(1000);
        }
    }
}
