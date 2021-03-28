using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Throw : MonoBehaviour
{
    public float angle;
    Rigidbody rb;
    public Transform dirPoint;

    public float deltaAngle;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody>();
        dirPoint.localPosition = new Vector3(0, Mathf.Sin(angle * Mathf.Deg2Rad), Mathf.Cos(angle * Mathf.Deg2Rad));
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.D))
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f, 100f * Time.deltaTime, 0f) + transform.rotation.eulerAngles);
        }
        else if (Input.GetKey(KeyCode.A))
        {
            transform.rotation = Quaternion.Euler(new Vector3(0f, 100f * -Time.deltaTime, 0f) + transform.rotation.eulerAngles);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            ThrowIt();
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            // рассчет локального направления вектора в плоскости вектора XZ от угла deltaAngle (нормализ. вектор)
            Vector3 localDir = new Vector3(Mathf.Sin(deltaAngle * Mathf.Deg2Rad), 0, Mathf.Cos(deltaAngle * Mathf.Deg2Rad));

            // рассчет глоб. направления вектора в плоскости вектора XZ относительно того, куда смотрит объект (нормализ. вектор)
            Vector3 worldDir = (transform.TransformPoint(localDir) - transform.position).normalized;

            Debug.DrawLine(transform.position, transform.position + worldDir * 5f, Color.red, 5f);
        }
    }

    private void ThrowIt()
    {
        rb.isKinematic = false;
        //rb.velocity = transform.forward * 5f;
        Vector3 direction = dirPoint.position - transform.position;

        rb.AddForce(10 * direction, ForceMode.VelocityChange);
    }
}
