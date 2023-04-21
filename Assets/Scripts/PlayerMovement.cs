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

    [SerializeField] private float maxSpeed;
    [SerializeField] private float movementSpeed;

    [SerializeField] private Animator anim;

    private Vector3 inputStartPos;
    private Vector3 currentInput;
    private bool mouseDown;

    private bool isPunching = false;

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
        Vector3 targetInput = mouseDown ? (currentMousePos - inputStartPos) : targetInput = Vector3.zero;
        currentInput = Vector3.ClampMagnitude(Vector3.Lerp(currentInput, targetInput, .2f), maxSpeed);

        // Calculates next position to target
        float finalSpeed = isPunching ? movementSpeed * .3f : movementSpeed;
        Vector3 targetPos =
            new Vector3(transform.position.x + (currentInput.x * finalSpeed * .01f),
            0,
            transform.position.z + (currentInput.y * finalSpeed * .01f));
        transform.position = Vector3.Lerp(transform.position, targetPos, .2f);

        // Adjust rotation to movement direction
        Quaternion targetRot = Quaternion.LookRotation(targetPos - transform.position);
        targetRot.z = 0;
        targetRot.x = 0;

        float finalRotLerp = isPunching ? .05f : .2f;
        if (currentInput.magnitude > .2f)
            anim.transform.rotation = Quaternion.Lerp(anim.transform.rotation, targetRot, finalRotLerp);

        // Configure animation 
        anim.SetFloat("MovBlend", currentInput.magnitude/100);

        if (Input.GetKeyDown(KeyCode.P)) anim.SetTrigger("PunchTrigger");
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"colission with {other.gameObject.name}");
        if(other.tag == "Enemy")
            anim.SetTrigger("PunchTrigger");

        isPunching = true;
        Invoke("SetUnPunch", 1.1f);
    }

    public void ApplyPunchEffect()
    {
        Debug.Log("PUNCH EFFECT");
    }

    private void SetUnPunch()
    {
        isPunching = false;
    }
}
