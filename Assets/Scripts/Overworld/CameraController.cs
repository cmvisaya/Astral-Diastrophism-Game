using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{

    public Transform target;
    public Vector3 offset;
    public bool useOffsetValues;
    public float rotateSpeed;
    public Transform pivot;

    public float maxViewAngle;
    public float minViewAngle;

    public bool invertY;

    private float dv;

    // Start is called before the first frame update
    void Start()
    {
        if(!useOffsetValues)
        {
            offset = target.position - transform.position;
        }

        pivot.transform.position = target.transform.position;
        pivot.transform.parent = null;
    }

    // Update is called once per frame
    void LateUpdate()
    {
        pivot.transform.position = target.transform.position;

        float horizontal = Input.GetAxis("Mouse X") * rotateSpeed;

        pivot.Rotate(0, horizontal, 0);
        //Debug.Log("X = " + pivot.rotation.eulerAngles.x + " | Y = " + pivot.rotation.eulerAngles.y);

        float vertical = Input.GetAxis("Mouse Y") * rotateSpeed;
        dv -= vertical;
        if (dv > maxViewAngle)
        {
            dv = maxViewAngle;
        }
        if (dv < minViewAngle)
        {
            dv = minViewAngle;
        }
        //Debug.Log("dvAngle = " + dv + " | dv = " + Mathf.Asin(dv * Mathf.Deg2Rad));

        if (invertY)
        {
            pivot.Rotate(vertical, 0, 0);
        }
        else
        {
            pivot.Rotate(-vertical, 0, 0);
        }

        //Debug.Log("y = " + pivot.rotation.y + " | z = " + pivot.rotation.z);
        if (pivot.rotation.eulerAngles.x > maxViewAngle && pivot.rotation.eulerAngles.x < 180f)
        {
            pivot.rotation = Quaternion.Euler(maxViewAngle, pivot.rotation.eulerAngles.y, pivot.rotation.eulerAngles.z);
            //Debug.Log("Snapback up: y = " + pivot.rotation.y + " | z = " + pivot.rotation.z);
        }
        if (pivot.rotation.eulerAngles.x > 180f && pivot.rotation.eulerAngles.x < 360f + minViewAngle)
        {
            pivot.rotation = Quaternion.Euler(360f + minViewAngle, pivot.rotation.eulerAngles.y, pivot.rotation.eulerAngles.z);
            //Debug.Log("Snapback down: y = " + pivot.rotation.y + " | z = " + pivot.rotation.z);
        }

        float desiredYAngle = pivot.eulerAngles.y;
        float desiredXAngle = pivot.eulerAngles.x;
        Quaternion rotation = Quaternion.Euler(desiredXAngle, desiredYAngle, 0);
        transform.position = (target.position + new Vector3(0, Mathf.Asin(dv * Mathf.Deg2Rad) * offset.magnitude, 0)) - (rotation * offset);

        transform.LookAt(target);
    }
}
