using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;

    //[Header("Buttons:")]
    //[SerializeField] private Button DestroyButton;
    //[SerializeField] private Button MenuButton;


    private GameObject activeMenu;
    [Header("Menus:")]
    [SerializeField] private GameObject SellObjMenu;
    [SerializeField] private GameObject SellerMenu;
    [SerializeField] private GameObject FactoryMenu;
    [SerializeField] private GameObject PipelineMenu;
    [SerializeField] private GameObject PurchaserMenu;
    [SerializeField] private GameObject FactoryObjsShopMenu;
    [SerializeField] private GameObject MainMenu;

    [Header("Vars")]
    [SerializeField] private Text money;

    private FactoryMenuHandler factoryMenuHandler;
    private PurchaserMenuHandler purchaserMenuHandler;
    private SellObjectMenuHandler sellObjectMenuHandler;
    //private PipelineMenuHandler pipelineMenuHandler;

    [Header("Detect layers:")]
    [SerializeField] private LayerMask FactoryObjLayer; //to identify what we have selected
    [SerializeField] private LayerMask SellObjLayer;    //to identify what we have selected
    [SerializeField] private LayerMask SelectiveLayer;  //what we can select [transfer&factoryObj]

    private GameObject selectedObj; //by LMB without builder enabled
    private Builder builder;
    private ObjectsHolder gameObjectsHolder;

    [Header("Raycast settings")]
    public float rayDistance = 500f;
    private Camera mainCamera;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        AddListenersButton();

        SaveSystem.instance.SetupGame();
    }

    private void Start()
    {
        factoryMenuHandler = FactoryMenu.GetComponent<FactoryMenuHandler>();
        purchaserMenuHandler = PurchaserMenu.GetComponent<PurchaserMenuHandler>();
        sellObjectMenuHandler = SellObjMenu.GetComponent<SellObjectMenuHandler>();

        builder = Builder.instance;
        gameObjectsHolder = ObjectsHolder.instance;
        mainCamera = Camera.main;
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && !builder.enabled)
        {
            if (activeMenu && activeMenu.activeSelf
                || EventSystem.current.IsPointerOverGameObject()) //clicked on UI element
                return;

            RaycastHit hit;
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, rayDistance, SelectiveLayer))
            {
                selectedObj = hit.collider.transform.root.gameObject;
                int objLayerValue = 1 << selectedObj.layer;

                if (objLayerValue == SellObjLayer.value)
                {
                    print("clicked on sell obj");
                    SetUpAndOpenObjMenu(selectedObj,
                        selectedObj.GetComponent<SellObject>());
                }
                else if (objLayerValue == FactoryObjLayer.value)
                {
                    print("clicked on factory obj");
                    SetUpAndOpenObjMenu(selectedObj,
                        selectedObj.GetComponent<FactoryObj>());
                }
            }
        }
    }

    private void SetUpAndOpenObjMenu(GameObject obj, FactoryObj factoryObj)
    {
        //TODO: factory menus setup

        switch (factoryObj.type)
        {
            case FactoryObjTypes.Pipeline:
                activeMenu = PipelineMenu;

                break;

            case FactoryObjTypes.Factory:
                factoryMenuHandler.SetUpMenu((Factory)factoryObj);
                activeMenu = FactoryMenu;

                break;

            case FactoryObjTypes.Purchaser:
                purchaserMenuHandler.SetUpMenu((Purchaser)factoryObj);
                activeMenu = PurchaserMenu;

                break;

            case FactoryObjTypes.SellPort:
                activeMenu = SellerMenu;

                break;

            default:
                Debug.LogError("?");
                break;
        }

        activeMenu.SetActive(true);

        //freeze time to prevent any issues
        Time.timeScale = 0f;
    }

    private void SetUpAndOpenObjMenu(GameObject obj, SellObject sellObj)
    {
        activeMenu = SellObjMenu;
        sellObjectMenuHandler.SetUpMenu(sellObj);

        activeMenu.SetActive(true);

        //freeze time to prevent any issues
        Time.timeScale = 0f;
    }

    public void UpdateMoneyText(int amount)
    {
        money.text = amount.ToString();
    }

    #region Events

    //Game UI Button that is not in Obj Menu
    public void OnDeleteModeClick()
    {
        //if builder was active -> we disable this mode
        if (builder.enabled)
        {
            if (builder.Mode == Builder.Modes.Delete)       //second click on the same obj
            {
                builder.TurnOffBuilder();
                //TODO: Icon change
                return;
            }
            builder.TurnOffBuilder();                       //for example if we was in delete mode 
                                                            //and switched to transform
        }

        builder.Mode = Builder.Modes.Delete;
        //TODO: icon change
        builder.enabled = true;

        if (gameObjectsHolder && !gameObjectsHolder.DirectionArrows)
        {
            gameObjectsHolder.DirectionArrows = true;
        }
    }

    public void OnCloseObjMenuButtonClick(GameObject menu)
    {
        menu.SetActive(false);

        if (menu == activeMenu)
        {
            activeMenu = null;
        }

        Time.timeScale = 1f;
    }

    public void OnTransformClick()
    {
        //if builder was active -> we disable this mode
        if (builder.enabled)
        {
            if (builder.Mode == Builder.Modes.Transform)    //second click on the same obj
            {
                builder.TurnOffBuilder();
                //TODO: Icon change
                return;
            }
            builder.TurnOffBuilder();                       //for example if we was in delete mode 
                                                            //and switched to transform
        }

        builder.TransformObj(selectedObj);
        builder.Mode = Builder.Modes.Transform;
        //TODO: icon change
        builder.enabled = true;

        if (gameObjectsHolder && !gameObjectsHolder.DirectionArrows)
        {
            gameObjectsHolder.DirectionArrows = true;
        }

        selectedObj = null;
        OnCloseObjMenuButtonClick(activeMenu);
    }

    public void OnDeleteClick()
    {
        if (selectedObj)
        {
            Destroy(selectedObj);
        }
        else
        {
            Debug.LogError("obj delete exception - no selected obj");
        }

        selectedObj = null;
        OnCloseObjMenuButtonClick(activeMenu);
    }

    public void OnFactoryShopClick()
    {
        if (activeMenu == FactoryObjsShopMenu)
        {
            OnCloseObjMenuButtonClick(activeMenu);
            return;
        }

        //-> open factory Obj menu
        FactoryObjsShopMenu.SetActive(true);
        activeMenu = FactoryObjsShopMenu;

        //freeze time to prevent any issues
        Time.timeScale = 0f;
    }

    public void OnFactoryObjBuyClick(FactoryObj factoryObj)
    {
        //if builder was active -> we disable this mode
        if (builder.enabled)
        {
            builder.TurnOffBuilder(false);                  //for example if we was in delete mode 
                                                            //and switched to transform
        }

        builder.BuyFactoryObj(factoryObj);
        builder.Mode = Builder.Modes.Buy;
        //TODO: icon change
        builder.enabled = true;

        if (gameObjectsHolder && !gameObjectsHolder.DirectionArrows)
        {
            print("+");
            gameObjectsHolder.DirectionArrows = true;
        }

        OnCloseObjMenuButtonClick(activeMenu);
    }

    public void OnMainMenuButtonClick()
    {
        if (activeMenu == MainMenu)
        {
            OnCloseObjMenuButtonClick(activeMenu);
            return;
        }

        //to prevent save issue when builder mode is on and temp obj saves too
        if (builder.enabled)
        {
            builder.TurnOffBuilder();
        }

        //-> open main menu
        MainMenu.SetActive(true);
        activeMenu = MainMenu;

        //freeze time to prevent any issues
        Time.timeScale = 0f;
    }

    public void OnExitWithSaveButtonClick()
    {
        SaveSystem.instance.SaveGame();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game_Start_Menu");
    }

    public void OnExitWithoutSaveButtonClick()
    {
        if (SaveSystem.instance.IsNewGame)
        {
            SaveSystem.instance.DeleteSaveFile();
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene("Game_Start_Menu");
    }

    private void AddListenersButton()
    {
        PipelineMenu.transform.Find("Transform").GetComponent<Button>().onClick.AddListener(OnTransformClick);
        FactoryMenu.transform.Find("Transform").GetComponent<Button>().onClick.AddListener(OnTransformClick);
        PurchaserMenu.transform.Find("Transform").GetComponent<Button>().onClick.AddListener(OnTransformClick);
        SellerMenu.transform.Find("Transform").GetComponent<Button>().onClick.AddListener(OnTransformClick);

        PipelineMenu.transform.Find("Delete").GetComponent<Button>().onClick.AddListener(OnDeleteClick);
        FactoryMenu.transform.Find("Delete").GetComponent<Button>().onClick.AddListener(OnDeleteClick);
        PurchaserMenu.transform.Find("Delete").GetComponent<Button>().onClick.AddListener(OnDeleteClick);
        SellerMenu.transform.Find("Delete").GetComponent<Button>().onClick.AddListener(OnDeleteClick);
        SellObjMenu.transform.Find("Delete").GetComponent<Button>().onClick.AddListener(OnDeleteClick);
    }

    #endregion
}
