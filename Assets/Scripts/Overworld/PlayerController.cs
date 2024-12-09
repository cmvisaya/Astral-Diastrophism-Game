using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    public float moveSpeed;
    public float sprintSpeed;
    public float slopeSpeed;
    public float jumpForce;
    public float gravityScale;

    public CharacterController controller;
    private Vector3 moveDirection;

    public Animator anim;

    public Transform pivot;
    public float rotateSpeed;

    public GameObject playerModel;

    private float cumulativeStepTime = 0f;
    private float stepDelay;
    [SerializeField] private float walkSD;
    [SerializeField] private float sprintSD;
    private bool stepPlayed = false;
    public int stepSoundIndex = 2;
    private float timeSinceLastStep = 0f;

    private bool grounded = false;
    private float speed = 0f;

    private bool willSlideOnSlopes = true;
    private Vector3 hitPointNormal;
    private bool isSliding
    {
        get
        {
            if(grounded && Physics.Raycast(transform.position, Vector3.down, out RaycastHit slopeHit, 2f))
            {
                hitPointNormal = slopeHit.normal;
                return Vector3.Angle(hitPointNormal, Vector3.up) > controller.slopeLimit;
            }
            else
            {
                return false;
            }
        }
    }

    public bool hasControl = true;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        if(GameObject.Find("Pivot"))
        {
            pivot = GameObject.Find("Pivot").transform;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (hasControl)
        {
            float yStore = moveDirection.y;
            moveDirection = (transform.forward * Input.GetAxis("Vertical")) + (transform.right * Input.GetAxis("Horizontal"));
            if (Input.GetButton("Sprint") || Input.GetAxis("Sprint") > 0)
            {
                moveDirection = moveDirection.normalized * sprintSpeed;
                stepDelay = sprintSD;
            }
            else
            {
                moveDirection = moveDirection.normalized * moveSpeed;
                stepDelay = walkSD;
            }
            moveDirection.y = yStore;

            if (controller.isGrounded)
            {
                moveDirection.y = -1f;
                if (Input.GetButtonDown("Jump"))
                {
                    moveDirection.y = jumpForce;
                }
            }
            else if (grounded && Input.GetButtonDown("Jump")) //Account for "sliding" jump
            {
                moveDirection.y = jumpForce;
            }

            moveDirection.y = moveDirection.y + (Physics.gravity.y * gravityScale * Time.deltaTime);

            //More sliding stuff?
            if (willSlideOnSlopes && isSliding)
            {
                moveDirection += new Vector3(hitPointNormal.x, -hitPointNormal.y, hitPointNormal.z) * slopeSpeed;
            }

            controller.Move(moveDirection * Time.deltaTime);

            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
            {
                transform.rotation = Quaternion.Euler(0f, pivot.rotation.eulerAngles.y, 0f);
                Quaternion newRotation = Quaternion.LookRotation(new Vector3(moveDirection.x, 0f, moveDirection.z));
                playerModel.transform.rotation = Quaternion.Slerp(playerModel.transform.rotation, newRotation, rotateSpeed * Time.deltaTime);
            }

            //Playsound
            speed = Mathf.Sqrt(Mathf.Pow(moveDirection.x, 2) + Mathf.Pow(moveDirection.z, 2));
            timeSinceLastStep += Time.deltaTime;
            if (speed > 0 && grounded && !isSliding)
            {
                cumulativeStepTime += Time.deltaTime;
                if (stepPlayed && timeSinceLastStep >= stepDelay)
                {
                    if (FindObjectOfType<AudioManager>() && stepSoundIndex < FindObjectOfType<AudioManager>().soundEffects.Length) {
                        FindObjectOfType<AudioManager>().PlaySoundEffect(stepSoundIndex, 2f);
                        timeSinceLastStep = 0f;
                        stepPlayed = false;
                    }
                }
                else if (stepDelay <= cumulativeStepTime)
                {
                    cumulativeStepTime = 0f;
                    stepPlayed = true;
                }
            }
            else
            {
                cumulativeStepTime = 0f;
                stepPlayed = true;
            }

            //Distance to ground
            RaycastHit hit = new RaycastHit();
            var distanceToGround = 0f; ;
            if (Physics.Raycast(transform.position, -Vector3.up, out hit))
            {
                distanceToGround = hit.distance;
            }

            grounded = controller.isGrounded || distanceToGround < 1.5f;
            //Debug.Log("Sliding: " + isSliding);
            //Debug.Log("Grounded: " + grounded);

            //Debug.Log("distanceToGround: " + distanceToGround);
        }
        else { speed = 0; }
        anim.SetBool("isGrounded", grounded);
        anim.SetBool("isSliding", isSliding);
        anim.SetFloat("Speed", speed);
    }
}
