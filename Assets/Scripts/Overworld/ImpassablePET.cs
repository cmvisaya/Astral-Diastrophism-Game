using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImpassablePET : MonoBehaviour
{
    public float newx;
    public float newy;
    public float newz;

    public void ResetPlayerPos()
    {
        FindObjectOfType<PlayerController>().controller.enabled = false;
        GameObject.Find("Player").transform.position = new Vector3(newx, newy, newz);
        FindObjectOfType<PlayerController>().controller.enabled = true;
    }
}
