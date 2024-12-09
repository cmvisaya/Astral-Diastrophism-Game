using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCamera : MonoBehaviour
{
    public float rotateSpeed;

    void Update()
    {
        transform.rotation = Quaternion.Euler(0f, transform.rotation.eulerAngles.y + rotateSpeed, transform.rotation.eulerAngles.z + rotateSpeed);
    }
}
