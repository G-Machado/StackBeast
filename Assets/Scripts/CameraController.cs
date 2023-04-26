using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private JoyStickController inputController;
    [SerializeField] private float movementLerp;
    [SerializeField] private float movRange;
    [SerializeField] private Vector3 maxStackPos;
    private Vector3 offset;
    private Vector3 maxOffset;

    // Variable created for smooth effect outside joystick
    private Vector3 smoothInput;

    void Start()
    {
        // Initialize min offset and max offset calculations
        offset = transform.position - PlayerController.instance.transform.position;
        maxOffset =  maxStackPos - PlayerController.instance.transform.position;

        // Initialize smooth input value
        smoothInput = inputController.currentInput;
    }

    void FixedUpdate()
    {
        // Updates the smoothInput to the camera doesn't changes to fast from position 
        smoothInput = Vector3.Lerp(smoothInput, inputController.currentInput, .2f);

        // Calculates the current offset relative to amount of enemies stacked
        Vector3 finalOffset = Vector3.Lerp(offset, maxOffset, PlayerController.instance.enemiesStacked.Count / (.01f + 20));

        // Calculates the final position using input and offset
        Vector3 targetPos = PlayerController.instance.transform.position +
            new Vector3(smoothInput.x, 0, smoothInput.y).normalized * movRange
            + finalOffset;

        transform.position = Vector3.Lerp(transform.position, targetPos, movementLerp);
    }
}
