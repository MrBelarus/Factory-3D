
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HelpIndicator : MonoBehaviour
{
    public Transform arrowContainer;    //keep clean
    public GameObject arrowPrefab;

    public List<Transform> targetTransforms;
    private List<RectTransform> arrowTransforms;

    public Camera mainCamera;
    private Vector3 screenCenter;

    public CanvasScaler canvasScaler;

    private Vector2 refScreenResolution;
    private Vector2 screenResFactor;

    public float borderOffset = 25f;

    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 1;

        canvasScaler = GetComponent<CanvasScaler>();
        refScreenResolution = canvasScaler.referenceResolution;

        screenResFactor = new Vector2(Screen.width / refScreenResolution.x,
            Screen.height / refScreenResolution.y);

        borderOffset *= screenResFactor.x;

        arrowTransforms = new List<RectTransform>();
        mainCamera = Camera.main;
        screenCenter = new Vector3(Screen.width / 2, Screen.height / 2);

        Add();
    }

    private void LateUpdate()
    {
        DrawPointers();
    }

    private void DrawPointers()
    {
        for (int i = 0; i < arrowTransforms.Count; i++)
        {
            Vector3 screenPointOfTarget = mainCamera.WorldToScreenPoint(targetTransforms[i].position);

            bool isInCameraView = screenPointOfTarget.x > 0 && screenPointOfTarget.x < Screen.width
                && screenPointOfTarget.y > 0 && screenPointOfTarget.y < Screen.height;

            if (isInCameraView)
            {
                arrowTransforms[i].gameObject.SetActive(false);
            }
            else
            {
                if (screenPointOfTarget.z < 0)
                {
                    screenPointOfTarget *= -1;
                }

                Vector3 targetPos = screenPointOfTarget;

                float angle = GetAngleByVector((screenPointOfTarget - screenCenter).normalized);
                arrowTransforms[i].localEulerAngles = new Vector3(0, 0, angle);

                if (screenPointOfTarget.x <= borderOffset) 
                    targetPos.x = borderOffset;
                else if (screenPointOfTarget.x >= Screen.width - borderOffset) 
                    targetPos.x = Screen.width - borderOffset;

                if (screenPointOfTarget.y <= borderOffset) 
                    targetPos.y = borderOffset;
                else if (screenPointOfTarget.y >= Screen.height - borderOffset) 
                    targetPos.y = Screen.height - borderOffset;


                targetPos /= screenResFactor;
                arrowTransforms[i].anchoredPosition = targetPos;
                arrowTransforms[i].gameObject.SetActive(true);
            }
        }
    }

    private void Add()
    {
        for (int i = 0; i < targetTransforms.Count; i++)
        {
            GameObject arrow = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity);
            arrow.transform.SetParent(arrowContainer.transform, false);

            arrowTransforms.Add(arrow.GetComponent<RectTransform>());
        }
    }

    private float GetAngleByVector(Vector3 dir)
    {
        float angle = Vector3.Angle(Vector3.up, dir);

        if (dir.x > 0)
        {
            angle = 360 - angle;
        }

        return angle;
    }
}