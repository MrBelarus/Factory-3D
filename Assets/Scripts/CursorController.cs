using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorController : MonoBehaviour
{
    public static CursorController instance;

    [SerializeField] private GameObject deleteCursor;
    [SerializeField] private GameObject transformCursor;
    [SerializeField] private GameObject mainCursor;

    [Header("Parent obj of all cursors")]
    [SerializeField] private Transform customCursorTransform;         //it will be used
    [SerializeField] private Cursors cursorOnEnable;

    private Cursors cursor = Cursors.Undefined;
    public Cursors CursorStyle
    {
        set
        {
            if (cursor == value)
            {
                return;
            }

            mainCursor.SetActive(false);
            transformCursor.SetActive(false);
            deleteCursor.SetActive(false);

            switch (value)
            {
                case Cursors.Main:
                    mainCursor.SetActive(true);
                    break;
                case Cursors.Delete:
                    deleteCursor.SetActive(true);
                    break;
                case Cursors.Transform:
                    transformCursor.SetActive(true);
                    break;
                default:
                    break;
            }

            cursor = value;
        }
        get
        {
            return cursor;
        }
    }

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

        //to make custom
        Cursor.visible = false;
        CursorStyle = cursorOnEnable;
    }

    // Update is called once per frame
    void Update()
    {
        customCursorTransform.position = Input.mousePosition;
    }
}

public enum Cursors
{
    Undefined,
    Main,
    Delete,
    Transform
}