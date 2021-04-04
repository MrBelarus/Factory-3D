using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
    private LayerMask FactoryObjLayer;

    //{
    //    get
    //    {
    //        return Mode;
    //    }
    //    set
    //    {
    //        switch (value)
    //        {
    //            case Modes.Replace:
    //                replaceMode = true;
    //                deleteMode = false;
    //                break;
    //            case Modes.Delete:
    //                deleteMode = true;
    //                replaceMode = false;
    //                break;
    //            case Modes.Transform:

    //            default:
    //                break;
    //        }
    //    }
    //}

    //Transform mode
    private Vector3 objectToTransformDefaultPos;
    private Vector3 objectToTransformDefaultRotation;


    public GameObject objToReplacePrefab; //prefab
    private GameObject objectToReplace; //instance of a prebab
    private float lastRotation = 0f;

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

        ////Interface to UI Manager
        //CreateObjToReplace();   //temp
    }

    //check the player's input
    private void Update()
    {
        bool LMBPressed = Input.GetMouseButtonDown(0);

        if (LMBPressed && Mode == Modes.Buy)    //replace obj
        {
            Collider[] overlaps = Physics.OverlapBox(objectToReplace.transform.position,
                Vector3.one / 2.05f, Quaternion.identity, ObstaclesLayer.value);
            print(overlaps.Length);

            if (overlaps.Length > 0)
            {
                for (int i = 0; i < overlaps.Length; i++)
                {
                    if (1 << overlaps[i].gameObject.layer == FactoryObjLayer.value)
                    {
                        if (overlaps[i].transform.position != objectToReplace.transform.position
                            && overlaps[i].transform.forward == -objectToReplace.transform.forward)
                        {
                            //TODO: can't replace UI title
                            print("Ты дурак?!");
                        }
                        else if (overlaps[i].transform.position != objectToReplace.transform.position)
                        {
                            ReplaceObj();
                            CreateObjToReplace();
                            //TODO: -money
                        }
                    }
                }
            }
            else
            {
                ReplaceObj();
                CreateObjToReplace();
                //TODO: -money
            }
        }
        else if (LMBPressed && Mode == Modes.Delete)
        {
            RaycastHit hit;
            Ray ray = cameraMain.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, maxRayDistance, FactoryObjLayer))
            {
                //TODO: UI? - do u realy want to remove it

                //TODO: Game manager? - add sell cost to player pocket

                Destroy(hit.collider.transform.root.gameObject);
            }
        }
        else if (LMBPressed && Mode == Modes.Transform)
        {
            Collider[] overlaps = Physics.OverlapBox(objectToReplace.transform.position,
                Vector3.one / 2.05f, Quaternion.identity, ObstaclesLayer.value);
            print(overlaps.Length);

            if (overlaps.Length > 0)
            {
                for (int i = 0; i < overlaps.Length; i++)
                {
                    if (1 << overlaps[i].gameObject.layer == FactoryObjLayer.value)
                    {
                        bool overlapsFactory = overlaps[i].name == "base";

                        if (!overlapsFactory
                            && overlaps[i].transform.forward == -objectToReplace.transform.forward)
                        {
                            //TODO: can't replace UI title
                            print("Ты дурак?!");
                        }
                        else if (!overlapsFactory)    //base - rigidbody+collider (not trigger)
                        {
                            ReplaceObj();
                            objectToReplace = null;
                            this.enabled = false;
                        }
                    }
                }
            }
            else
            {
                ReplaceObj();
                objectToReplace = null;
                this.enabled = false;
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
