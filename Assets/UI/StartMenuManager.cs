using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartMenuManager : MonoBehaviour
{
    private SaveSystem saveSystem;

    [Header("Load menu vars")]
    [SerializeField] private GameObject LoadView;
    [SerializeField] private RectTransform ContentRect;
    [SerializeField] private RectTransform LoadButtonPrefab;
    [SerializeField] private int topOffset = 75;
    [SerializeField] private int offsetBetweenItemsY = 100;
    [SerializeField] private Button loadSaveButton;
    [SerializeField] private float disabledButtonAlphaColor;

    private GameObject activeMenu = null;

    bool saveButtonsWereLoaded = false;

    private void Start()
    {
        saveSystem = SaveSystem.instance;

        if (saveSystem.GetSaveNames().Length == 0)
        {
            loadSaveButton.enabled = false;
            loadSaveButton.GetComponent<CanvasGroup>().alpha = disabledButtonAlphaColor;
        }
    }

    public void LoadGame(string saveName)
    {
        saveSystem.LoadGame(saveName);
        SceneManager.LoadScene("Game_Play_Scene");
    }

    #region Events
    public void OnExitGameClick()
    {
        Application.Quit();
    }

    public void OnLoadGameClick()
    {
        if (saveButtonsWereLoaded)
        {
            activeMenu = LoadView;
            LoadView.SetActive(true);
            return;
        }

        string[] saves = saveSystem.GetSaveNames();

        ContentRect.sizeDelta = new Vector2(ContentRect.sizeDelta.x,
            topOffset / 2 + (LoadButtonPrefab.sizeDelta.y + offsetBetweenItemsY) * saves.Length);

        for (int i = 0; i < saves.Length; i++)
        {
            GameObject button = Instantiate(LoadButtonPrefab.gameObject, ContentRect);

            RectTransform buttonRectTransform = button.GetComponent<RectTransform>();
            buttonRectTransform.anchoredPosition = new Vector2(0, -topOffset + -i * (buttonRectTransform.sizeDelta.y + offsetBetweenItemsY));

            Text saveText = buttonRectTransform.GetChild(0).GetComponent<Text>();
            saveText.text = saves[i];

            Button buttonScript = button.GetComponent<Button>();
            int copy = i;   //event arguments fix
            buttonScript.onClick.AddListener(() => { LoadGame(saves[copy]); });
        }

        saveButtonsWereLoaded = true;

        activeMenu = LoadView;
        LoadView.SetActive(true);
    }

    public void OnCloseMenuButtonClick(GameObject menu)
    {
        menu.SetActive(false);

        if (menu == activeMenu)
        {
            activeMenu = null;
        }
    }
    #endregion
}
