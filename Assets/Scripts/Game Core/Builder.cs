using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Builder : MonoBehaviour
{
    public static Builder instance;

    public static event Action<FactoryObj> OnFactoryObjBuy;
    public static event Action<FactoryObj> OnFactoryObjDeleted;

    private Camera cameraMain;

    public enum Modes
    {
        Undefined,
        Buy,
        Delete,
        Transform,
    }

    private Modes mode;
    public Modes Mode 
    {
        get => mode;
        set
        {
            switch (value)
            {
                case Modes.Undefined:
                    cursorController.CursorStyle = Cursors.Main;
                    break;
                case Modes.Buy:
                    //TODO: buy/transform cursor
                    cursorController.CursorStyle = Cursors.Transform;
                    break;
                case Modes.Delete:
                    if (audioManager)
                    {
                        audioManager.PlaySound(Sounds.DeleteModeOn);
                    }
                    cursorController.CursorStyle = Cursors.Delete;
                    break;

                case Modes.Transform:
                    cursorController.CursorStyle = Cursors.Transform;
                    break;

                default:
                    Debug.LogError("Invalid build mode!");
                    break;
            }

            mode = value;
        }
    }

    [SerializeField]
    private float maxRayDistance = 100f;

    [SerializeField]
    private LayerMask detectLayer;

    [SerializeField]
    private LayerMask ObstaclesLayer;

    [SerializeField]
    private LayerMask SelectiveLayer;

    [SerializeField]
    private LayerMask FactoryObjLayer;

    //Transform mode
    private Vector3 objectToTransformDefaultPos;
    private Vector3 objectToTransformDefaultRotation;


    public GameObject objToReplacePrefab;   //prefab
    private GameObject objectToReplace;     //instance of a prefab
    private FactoryObj factoryObjScript;
    private float lastRotation = 180f;      //if it's setup by defalt -> 
                                            //it will rotate first obj to spawn by N degrees

    private ObjectsHolder gameObjectsHolder;
    private CashManager cashManager;
    private CursorController cursorController;
    private AudioManager audioManager;

    // Start is called before the first frame update
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

        mode = Modes.Undefined;
        cameraMain = Camera.main;
    }

    private void Start()
    {
        gameObjectsHolder = ObjectsHolder.instance;
        cashManager = CashManager.instance;
        cursorController = CursorController.instance;
        audioManager = AudioManager.instance;

        this.enabled = false;
    }

    //check the player's input
    private void Update()
    {
        bool LMBPressed = Input.GetMouseButtonDown(0);

        if (LMBPressed)
        {
            if (EventSystem.current.IsPointerOverGameObject())  //clicked on UI element
            {
                return;
            }

            switch (mode)
            {
                case Modes.Buy:
                    if (CanIReplaceIt() && cashManager.IsEnoughToSpend(factoryObjScript.cost))
                    {
                        OnFactoryObjBuy?.Invoke(factoryObjScript);

                        if (audioManager)
                        {
                            audioManager.PlaySound(Sounds.BuyFactoryObjSound, UnityEngine.Random.Range(0.95f, 1.05f));
                            audioManager.PlaySound(Sounds.ReplaceSound);
                        }

                        cashManager.Spend(factoryObjScript.cost);
                        ReplaceObj();
                        CreateObjToReplace();
                    }
                    break;

                case Modes.Delete:
                    RaycastHit hit;
                    Ray ray = cameraMain.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out hit, maxRayDistance, SelectiveLayer))
                    {
                        //TODO: UI? - do u realy want to remove it

                        FactoryObj factoryObj = hit.transform.root.gameObject.GetComponent<FactoryObj>();
                        CashManager.instance.Earn(factoryObj.cost / 2);

                        OnFactoryObjDeleted?.Invoke(factoryObj);

                        if (audioManager)
                        {
                            audioManager.PlaySound(Sounds.DeteleObjInDeleteModeSound, UnityEngine.Random.Range(0.95f, 1.05f));
                        }

                        Destroy(factoryObj.gameObject);
                    }
                    break;

                case Modes.Transform:
                    if (CanIReplaceIt())
                    {
                        ReplaceObj();
                        SetupObj();

                        if (gameObjectsHolder)
                        {
                            gameObjectsHolder.DirectionArrows = false;
                        }

                        if (audioManager)
                        {
                            audioManager.PlaySound(Sounds.FactoryObjTransformed);
                        }

                        Mode = Modes.Undefined;
                        objectToReplace = null;
                        factoryObjScript = null;
                        this.enabled = false;
                    }
                    break;

                default:
                    Debug.LogError("smth went wrong in builder");
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TurnOffBuilder();
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            if (objectToReplace)
                RotateObjAroundY(90f);
        }
    }

    private void FixedUpdate()
    {
        if (objectToReplace)
        {
            RaycastHit hit;
            Ray ray = cameraMain.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, maxRayDistance, detectLayer))
            {
                //Debug.Log("You selected the " + hit.transform.name);

                float x = hit.point.x;
                float z = hit.point.z;

                SnapPoint(ref x);
                SnapPoint(ref z);

                objectToReplace.transform.position = new Vector3(x, 1, z);
            }
        }
    }

    /// <summary>
    /// Disables work of builder. In addition you can leave arrows on
    /// </summary>
    /// <param name="turnOffArrows"></param>
    public void TurnOffBuilder(bool turnOffArrows = true)
    {
        if (objectToReplace)
        {
            switch (mode)
            {
                case Modes.Buy:
                    Destroy(objectToReplace);
                    factoryObjScript = null;
                    break;

                case Modes.Transform:
                    objectToReplace.transform.position = objectToTransformDefaultPos;
                    objectToReplace.transform.rotation = Quaternion.Euler(objectToTransformDefaultRotation);
                    ReplaceObj();
                    SetupObj();
                    factoryObjScript = null;
                    break;

                default:
                    Debug.LogError("Incorrect mode in builder!");
                    break;
            }
        }

        Mode = Modes.Undefined;

        if (turnOffArrows && gameObjectsHolder && gameObjectsHolder.DirectionArrows)
        {
            gameObjectsHolder.DirectionArrows = false;
        }

        objectToReplace = null;
        this.enabled = false;
    }

    private void SnapPoint(ref float x)
    {
        if (Mathf.Abs(x) % 1 > 0.5f)
        {
            if (x > 0)
            {
                x = Mathf.Ceil(x);
            }
            else
            {
                x = Mathf.Floor(x);
            }
        }
        else
        {
            if (x > 0)
            {
                x = Mathf.Floor(x);
            }
            else
            {
                x = Mathf.Ceil(x);
            }
        }
    }

    private void RotateObjAroundY(float degrees)
    {
        objectToReplace.transform.localEulerAngles += new Vector3(0f, degrees, 0f);
    }

    private void CreateObjToReplace()
    {
        if (objectToReplace)
        {
            lastRotation = objectToReplace.transform.rotation.eulerAngles.y;
        }

        objectToReplace = Instantiate(objToReplacePrefab, Vector3.down, Quaternion.Euler(new Vector3(0, lastRotation, 0)));
    }

    //LMB with buy / transform mode -> enable all colliders
    private void ReplaceObj()
    {
        Collider mainObjCol = objectToReplace.GetComponent<Collider>();

        if (mainObjCol)
        {
            mainObjCol.enabled = true;
        }

        Collider[] colliders = objectToReplace.transform.Find("base").GetComponents<Collider>();
        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }
    }

    private bool CanIReplaceIt()
    {
        Collider selectiveCol = objectToReplace.transform.Find("SelectiveCol").GetComponent<BoxCollider>();

        Vector3 objSize = new Vector3(selectiveCol.bounds.size.x, 1f, selectiveCol.bounds.size.z);
        Vector3 objCenter = selectiveCol.transform.position;

        Collider[] overlaps = Physics.OverlapBox(objCenter,
            objSize / 2.05f, Quaternion.identity, ObstaclesLayer.value);

        for (int i = 0; i < overlaps.Length; i++)
        {
            if (1 << overlaps[i].gameObject.layer == SelectiveLayer.value &&
                overlaps[i] != selectiveCol)    //ignore itself col
            {
                if (audioManager)
                {
                    audioManager.PlaySound(Sounds.CantReplaceHere, UnityEngine.Random.Range(0.95f, 1.05f));
                }
                return false;
            }
        }

        for (int i = 0; i < overlaps.Length; i++)
        {
            if (1 << overlaps[i].gameObject.layer == FactoryObjLayer.value)
            {
                if (overlaps[i].transform.position != objectToReplace.transform.position
                    && overlaps[i].transform.forward == -objectToReplace.transform.forward)
                {
                    if (audioManager)
                    {
                        audioManager.PlaySound(Sounds.CantReplaceHere, UnityEngine.Random.Range(0.95f, 1.05f));
                    }
                    return false;
                }
                else if (overlaps[i].transform.position != objectToReplace.transform.position)
                {
                    //commented logic prevents replace if we want to connect smth to purchaser
                    //if (overlaps[i].transform.root.GetComponent<FactoryObj>().type == FactoryObjTypes.Purchaser
                    //    && objectToReplace.GetComponent<FactoryObj>().type != FactoryObjTypes.Pipeline)
                    //{
                    //    if (audioManager)
                    //    {
                    //        audioManager.PlaySound(Sounds.CantReplaceHere);
                    //    }
                    //    return false;
                    //}
                    //if I want to keep lines above I should also check if objToReplace = Purchaser!
                    return true;
                }
            }
        }

        return true;
    }

    public void BuyFactoryObj(FactoryObj factoryObj)
    {
        objToReplacePrefab = factoryObj.gameObject;
        factoryObjScript = factoryObj;
        CreateObjToReplace();
    }

    public void TransformObj(GameObject obj)
    {
        FactoryObj factoryObj = obj.GetComponent<FactoryObj>();
        objToReplacePrefab = PrefabsContainer.instance.FactoryObjectPrefabs[factoryObj.prefabName].gameObject;
        
        //save pos & rotation before deleting
        objectToTransformDefaultPos = obj.transform.position;
        objectToTransformDefaultRotation = obj.transform.rotation.eulerAngles;

        //to keep correct connections in the future
        Destroy(obj);

        //updates objectToReplace
        CreateObjToReplace();
        factoryObjScript = factoryObj;

        objectToReplace.transform.rotation = Quaternion.Euler(objectToTransformDefaultRotation);
    }

    private void SetupObj()
    {
        switch (factoryObjScript.type)
        {
            case FactoryObjTypes.Factory:
                Factory beforeTransformFactory = (Factory)factoryObjScript;
                Factory factory = objectToReplace.GetComponent<Factory>();
                factory.CopyValues(beforeTransformFactory);
                break;

            case FactoryObjTypes.Purchaser:
                Purchaser beforeTransformPurchaser = (Purchaser)factoryObjScript;
                Purchaser purchaser = objectToReplace.GetComponent<Purchaser>();
                purchaser.CopyValues(beforeTransformPurchaser);
                break;

            default:
                break;
        }
    }
}
