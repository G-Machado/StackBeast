using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public static PlayerMovement instance;
    private void Awake()
    {
        instance = this;
    }

    [SerializeField] private float movementRange;
    [SerializeField] private float movementSpeed;

    [SerializeField] private Animator anim;

    [SerializeField] private Vector3 inputStartPos;
    private Vector3 currentInput;
    private bool mouseDown;

    void Start()
    {
        inputStartPos = Vector3.zero;
    }

    void Update()
    {
        // Handles input
        if (Input.GetMouseButtonDown(0))
        {
            inputStartPos = Input.mousePosition;
            mouseDown = true;
        }
        if (Input.GetMouseButtonUp(0))
        {
            inputStartPos = Vector3.zero;
            mouseDown = false;
            return;
        }
        Vector3 currentMousePos = Input.mousePosition;
        Vector3 targetInput = (currentMousePos - inputStartPos);
        if (!mouseDown) targetInput = Vector3.zero;
        currentInput = Vector3.Lerp(currentInput, targetInput, .2f);

        // Calculates next position to target
        Vector3 targetPos =
            new Vector3(transform.position.x + (currentInput.x * movementSpeed * .01f),
            0,
            transform.position.z + (currentInput.y * movementSpeed * .01f));
        transform.position = Vector3.Lerp(transform.position, targetPos, .2f);

        // Adjust rotation to movement direction
        Quaternion targetRot = Quaternion.LookRotation(targetPos - transform.position);
        targetRot.z = 0;
        targetRot.x = 0;
        if(currentInput.magnitude > .2f)
            anim.transform.rotation = Quaternion.Lerp(anim.transform.rotation, targetRot, .3f);

        // Configure animation 
        anim.SetFloat("MovBlend", currentInput.magnitude/100);
    }
}
