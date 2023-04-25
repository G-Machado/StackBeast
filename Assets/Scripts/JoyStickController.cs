using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class JoyStickController : MonoBehaviour
{
    [Header("Input Variables")]
    [SerializeField] private float inputGravity = 1;
    [SerializeField] private float sensitivity = 1;
    [SerializeField] private float maxInput = 1;
    [SerializeField] private float lerp = 1;
    [SerializeField] private float dragLerp = 1;

    private Vector3 inputStartPos;
    public Vector3 currentInput;
    private bool mouseDown;

    [Header("UI Variables")]
    [SerializeField] private Image button;
    [SerializeField] private Image area;
    [SerializeField] private RectTransform rect;

    void Start()
    {
        inputStartPos = Vector3.zero;
    }

    void Update()
    {
        // Handles input
        if (Input.GetMouseButton(0) && !mouseDown)
        {
            inputStartPos = Input.mousePosition;
            inputStartPos.z = 0;
            mouseDown = true;

            rect.anchoredPosition = inputStartPos;
            SetVisible(true);
        }
        if (!Input.GetMouseButton(0) && mouseDown)
        {
            inputStartPos = Vector3.zero;
            mouseDown = false;

            SetVisible(false);
            return;
        }
        Vector3 currentMousePos = Input.mousePosition;
        Vector3 currentJoystickPos = new Vector3(rect.anchoredPosition.x, rect.anchoredPosition.y, Input.mousePosition.z);
        Vector3 targetInput = mouseDown ? ((currentMousePos - currentJoystickPos)) : Vector3.zero;

        // Move joystick and redefines start pos
        if(targetInput.magnitude > maxInput * 2)
        {
            rect.anchoredPosition = Vector3.Lerp(rect.anchoredPosition, Input.mousePosition, dragLerp);
        }

        currentInput = Vector3.ClampMagnitude(Vector3.Lerp(currentInput, targetInput, lerp), maxInput);
        button.transform.localPosition = currentInput * sensitivity;
    }

    private void SetVisible(bool mode)
    {
        button.enabled = mode;
        area.enabled = mode;
    }
}
