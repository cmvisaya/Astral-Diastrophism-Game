using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public float targetingRange;
    public float targetingSpeed;
    public float returnSpeed;
    public float gravityScale;

    public CharacterController controller;
    private Vector3 initialPos;
    private Vector3 moveDirection;
    private Vector3 playerPos;

    public Animator anim;

    //public Transform pivot;
    //public float rotateSpeed;

    public GameObject model;

    void Awake()
    {
        initialPos = transform.position;
    }

    void Update()
    {
        float yStore = moveDirection.y;

        playerPos = GameObject.Find("Player").transform.position;
        moveDirection = playerPos - transform.position;
        //Debug.Log(moveDirection.magnitude);

        if (moveDirection.magnitude <= targetingRange / 2 && FindObjectOfType<GameManager>().interactable && Vector3.Distance(initialPos, transform.position) <= targetingRange * 1.5) { 
            moveDirection = moveDirection.normalized * targetingSpeed;
        }
        else {
            moveDirection = initialPos - transform.position;
            moveDirection = moveDirection.normalized * returnSpeed; 
        }

        moveDirection.y = yStore;

        if (controller.isGrounded)
        {
            moveDirection.y = -1f;
        }

        moveDirection.y = moveDirection.y + (Physics.gravity.y * gravityScale * Time.deltaTime);

        controller.Move(moveDirection * Time.deltaTime);
    }

    /*private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player" && FindObjectOfType<GameManager>().interactable)
        {
            targetLocked = true;
        }
        Debug.Log("Please work?");
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player" && FindObjectOfType<GameManager>().interactable)
        {
            targetLocked = false;
        }
    }*/
}
