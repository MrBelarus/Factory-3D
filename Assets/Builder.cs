using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Builder : MonoBehaviour
{
    public static Builder instance;

    private Camera cameraMain;

    public enum Modes
    {
        Undefined,
        Buy,
        Delete,
        Transform,
    }

    public Modes Mode { get; set; }

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


    public GameObject objToReplacePrefab; //prefab
    private GameObject objectToReplace; //instance of a prefab
    private float lastRotation = 0f;

    private ObjectsHolder gameObjectsHolder;

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
        }

        Mode = Modes.Undefined;
        cameraMain = Camera.main;
    }

    private void Start()
    {
        gameObjectsHolder = ObjectsHolder.instance;
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

            switch (Mode)
            {
                case Modes.Buy:
                    if (CanIReplaceIt())
                    {
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
                        //TODO: Game manager? - add sell cost to player pocket

                        Destroy(hit.transform.root.gameObject);
                    }
                    break;

                case Modes.Transform:
                    if (CanIReplaceIt())
                    {
                        ReplaceObj();

                        if (gameObjectsHolder)
                        {
                            gameObjectsHolder.DirectionArrows = false;
                        }

                        objectToReplace = null;
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
            if (objectToReplace)
            {
                switch (Mode)
                {
                    case Modes.Buy:
                        Destroy(objectToReplace);
                        break;

                    case Modes.Transform:
                        objectToReplace.transform.position = objectToTransformDefaultPos;
                        objectToReplace.transform.rotation = Quaternion.Euler(objectToTransformDefaultRotation);
                        ReplaceObj();
                        break;

                    default:
                        Debug.LogError("Incorrect mode in builder!");
                        break;
                }
            }

            Mode = Modes.Undefined;

            if (gameObjectsHolder && gameObjectsHolder.DirectionArrows)
            {
                gameObjectsHolder.DirectionArrows = false;
            }

            objectToReplace = null;
            this.enabled = false;
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
                    return false;
                }
                else if (overlaps[i].transform.position != objectToReplace.transform.position)
                {
                    if (overlaps[i].transform.root.GetComponent<FactoryObj>().type == FactoryObjTypes.Purchaser
                        && objectToReplace.GetComponent<FactoryObj>().type != FactoryObjTypes.Pipeline)
                    {
                        return false;
                    }
                    return true;
                }
            }
        }

        return true;
    }

    public void BuyFactoryObj(GameObject prefab)
    {
        objToReplacePrefab = prefab;
        CreateObjToReplace();
    }

    public void TransformObj(GameObject obj)
    {
        objToReplacePrefab = Resources.Load<GameObject>("Factory/" + obj.GetComponent<FactoryObj>().prefabName);
        
        //save pos & rotation before deleting
        objectToTransformDefaultPos = obj.transform.position;
        objectToTransformDefaultRotation = obj.transform.rotation.eulerAngles;

        //to keep correct connections in the future
        Destroy(obj);

        //updates objectToReplace
        CreateObjToReplace();

        objectToReplace.transform.rotation = Quaternion.Euler(objectToTransformDefaultRotation);
    }
}
