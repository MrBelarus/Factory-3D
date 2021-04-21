﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameUIManager : MonoBehaviour
{
    public static GameUIManager instance;

    [Header("Buttons:")]
    [SerializeField] private Button DestroyButton;
    //[SerializeField] private Button BuildMode;
    [SerializeField] private Button MenuButton;


    private GameObject activeMenu;
    [Header("Menus:")]
    [SerializeField] private GameObject SellObjMenu;
    [SerializeField] private GameObject SellerMenu;
    [SerializeField] private GameObject FactoryMenu;
    [SerializeField] private GameObject PipelineMenu;
    [SerializeField] private GameObject PurchaserMenu;

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
        print("Material");
        //TODO: sellObj menu setup
        sellObjectMenuHandler.SetUpMenu(sellObj);

        activeMenu.SetActive(true);

        //freeze time to prevent any issues
        Time.timeScale = 0f;
    }

    public void CloseObjMenu()  //Button event
    {
        activeMenu.SetActive(false);
        Time.timeScale = 1f;
    }

    #region Events

    //Game UI Button that is not in Obj Menu
    public void OnDeleteModeClick()
    {
        if (builder.enabled)
        {
            builder.enabled = false;
            //TODO: icon change
            return;
        }

        builder.Mode = Builder.Modes.Delete;
        //TODO: icon change
        builder.enabled = true;
    }

    public void OnTransformClick()
    {
        if (builder.enabled)
        {
            builder.enabled = false;
            //TODO: icon change
            return;
        }

        builder.TransformObj(selectedObj);
        builder.Mode = Builder.Modes.Transform;
        //TODO: icon change
        builder.enabled = true;

        if (gameObjectsHolder)
        {
            gameObjectsHolder.DirectionArrows = true;
        }

        selectedObj = null;
        CloseObjMenu();
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
        CloseObjMenu();
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
