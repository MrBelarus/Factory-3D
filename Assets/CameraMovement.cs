using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    private enum Rotates
    {
        Left,
        Right
    }

    private const int MIDDLE_MOUSE_ID = 2;
    private Transform cameraTransform;

    [SerializeField]
    private Transform gameCenter;

    private const float MAX_FOV = 25f;
    private const float MIN_FOV = 10f;
    private const float CAMERA_ZX_ROTATION = 90f;

    private float rotateRadius = 10f;

    [SerializeField]
    private float localRadiusBorder = 20f;
    public Vector3 borderCircleCenter;

    [SerializeField]
    private float zoomSpeed = 10f;

    private Vector3 cameraFowardDirection;
    private Vector3 cameraRightDirection;

    private Camera cameraScript;
    private Rigidbody rb;

    private float deltaX, deltaY;

    [SerializeField]
    private float edgeBorderScreenX = 10f, edgeBorderScreenY = 10f;
    [SerializeField]
    private float keyboardMoveSpeed = 0.1f;
    [SerializeField]
    private float shiftCoef = 2f;
    private float moveCoef = 1f;

    private bool lockBorderMovement = false;

    //private float screenWidth, screenHeight;
    //private float mousePosMotionCoef = 1; //where to go camera (up/right = 1, down/left = -1)
    //private Vector3 mousePos;

    private void Awake()
    {
        cameraScript = Camera.main;
        cameraTransform = cameraScript.transform;

        rb = GetComponent<Rigidbody>();

        //screenWidth = Screen.width;
        //screenHeight = Screen.height;

        cameraFowardDirection = cameraTransform.forward;
        cameraFowardDirection = new Vector3(cameraFowardDirection.x, 0, cameraFowardDirection.z);
        cameraRightDirection = new Vector3(cameraFowardDirection.z, 0, -cameraFowardDirection.x);

        rotateRadius = (transform.position - new Vector3(
            gameCenter.position.x, transform.position.y, gameCenter.position.z)).magnitude;

        borderCircleCenter = transform.position;
    }

    private void Update()
    {
        deltaX = Input.GetAxis("Mouse X");
        deltaY = Input.GetAxis("Mouse Y");
        //mousePos = Input.mousePosition;

        //rotate camera
        if (Input.GetKeyDown(KeyCode.E))
        {
            RotateCamera(Rotates.Right);
        }
        else if (Input.GetKeyDown(KeyCode.Q))
        {
            RotateCamera(Rotates.Left);
        }

        //ZoomIn or ZoomOut
        if (Input.GetKey(KeyCode.LeftControl))
        {
            if (Input.GetKeyDown(KeyCode.LeftControl))
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }

            cameraScript.fieldOfView += deltaY * Time.deltaTime * zoomSpeed;
            cameraScript.fieldOfView = Mathf.Clamp(cameraScript.fieldOfView, MIN_FOV, MAX_FOV);

            CheckWorldBorders();
        }

        //Change position by middle mouse
        else if (Input.GetMouseButton(MIDDLE_MOUSE_ID))
        {
            cameraTransform.position -= deltaY * cameraFowardDirection + deltaX * cameraRightDirection;
            lockBorderMovement = true;

            CheckWorldBorders();
        }

        if (Input.GetKeyUp(KeyCode.LeftControl))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else if (Input.GetMouseButtonUp(MIDDLE_MOUSE_ID))
        {
            lockBorderMovement = false;
        }
    }

    private void FixedUpdate()
    {
        if (lockBorderMovement)
        {
            return;
        }


        if (Input.GetKey(KeyCode.LeftShift))
        {
            moveCoef = shiftCoef;
        }
        else
        {
            moveCoef = 1f;
        }


        if (Input.GetKey(KeyCode.A))
        {
            rb.MovePosition(cameraTransform.position += Time.deltaTime * keyboardMoveSpeed
                * -cameraRightDirection * moveCoef);
        }
        else if (Input.GetKey(KeyCode.D))
        {
            rb.MovePosition(cameraTransform.position += Time.deltaTime * keyboardMoveSpeed
                * cameraRightDirection * moveCoef);
        }

        if (Input.GetKey(KeyCode.W))
        {
            rb.MovePosition(cameraTransform.position += Time.deltaTime * keyboardMoveSpeed
                * cameraFowardDirection * moveCoef);
        }
        else if (Input.GetKey(KeyCode.S))
        {
            rb.MovePosition(cameraTransform.position += Time.deltaTime * keyboardMoveSpeed
                * -cameraFowardDirection * moveCoef);
        }

        CheckWorldBorders();
    }

    //if (mousePos.x < edgeBorderScreenX
    //    || mousePos.x > screenWidth - edgeBorderScreenX)
    //{
    //    if (mousePos.x > screenWidth / 2)
    //    {
    //        mousePosMotionCoef = mousePos.x > screenWidth ? 1 : 1 - (screenWidth - mousePos.x) / edgeBorderScreenX;
    //    }
    //    else
    //    {
    //        mousePosMotionCoef = mousePos.x < 0f ? -1 : -(1 - Mathf.Abs(mousePos.x) / edgeBorderScreenX);
    //    }

    //    rb.MovePosition(cameraTransform.position += Time.deltaTime * borderCameraSpeed 
    //        * cameraRightDirection * mousePosMotionCoef);
    //}

    //if (mousePos.y < edgeBorderScreenY
    //|| mousePos.y > screenHeight - edgeBorderScreenY)
    //{
    //    if (mousePos.y > screenHeight / 2)
    //    {
    //        mousePosMotionCoef = mousePos.y > screenHeight ? 1 : 1 - (screenHeight - mousePos.y) / edgeBorderScreenY;
    //    }
    //    else
    //    {
    //        mousePosMotionCoef = mousePos.y < 0f ? -1 : -(1 - Mathf.Abs(mousePos.y) / edgeBorderScreenY);
    //    }

    //    rb.MovePosition(cameraTransform.position += Time.deltaTime * borderCameraSpeed 
    //        * cameraFowardDirection * mousePosMotionCoef);
    //}

    //CheckWorldBorders();

    private void CheckWorldBorders()
    {
        float distanceFromGameCenter = (transform.position - borderCircleCenter).magnitude;

        if (distanceFromGameCenter > localRadiusBorder)
        {
            Vector3 backwardDirection = (borderCircleCenter - transform.position).normalized;

            transform.position += backwardDirection * (distanceFromGameCenter - localRadiusBorder);
        }
    }

    private void RotateCamera(Rotates rotationType)
    {
        Vector3 fowardZX = new Vector3(transform.forward.x, 0, transform.forward.z).normalized;
        Vector3 rightZX = new Vector3(fowardZX.z, 0, -fowardZX.x);

        transform.position += fowardZX * rotateRadius;
        borderCircleCenter += fowardZX * rotateRadius;

        if (rotationType == Rotates.Right)
        {
            transform.position += rightZX * rotateRadius;
            borderCircleCenter += rightZX * rotateRadius;

            transform.rotation = Quaternion.Euler(new Vector3(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y - CAMERA_ZX_ROTATION,
                transform.rotation.eulerAngles.z));
        }
        else
        {
            transform.position -= rightZX * rotateRadius;
            borderCircleCenter -= rightZX * rotateRadius;

            transform.rotation = Quaternion.Euler(new Vector3(
                transform.rotation.eulerAngles.x,
                transform.rotation.eulerAngles.y + CAMERA_ZX_ROTATION,
                transform.rotation.eulerAngles.z));
        }

        cameraFowardDirection = cameraTransform.forward;

        cameraFowardDirection = new Vector3(cameraFowardDirection.x, 0, cameraFowardDirection.z);
        cameraRightDirection = new Vector3(cameraFowardDirection.z, 0, -cameraFowardDirection.x);
    }
}
