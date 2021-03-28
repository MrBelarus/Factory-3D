using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputDetection : MonoBehaviour
{
    private Camera cameraMain;

    [SerializeField]
    private float maxRayDistance = 100f;
    [SerializeField]
    private LayerMask detectLayer;

    public GameObject objectToReplace;

    // Start is called before the first frame update
    private void Awake()
    {
        cameraMain = Camera.main;
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            RaycastHit hit;
            Ray ray = cameraMain.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, maxRayDistance, detectLayer))
            {
                Debug.Log("You selected the " + hit.transform.name);

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
}
