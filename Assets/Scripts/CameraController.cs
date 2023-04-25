using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private JoyStickController inputController;
    [SerializeField] private float movementLerp = .2f;
    [SerializeField] private float movRange = .5f;
    private Vector3 offset;
    [SerializeField] private Vector3 maxStackPos;
    private Vector3 maxOffset;
    private Vector3 trackedInput;

    void Start()
    {
        offset = transform.position - PlayerMovement.instance.transform.position;
        maxOffset =  maxStackPos - PlayerMovement.instance.transform.position;
        trackedInput = inputController.currentInput;
    }

    void FixedUpdate()
    {
        trackedInput = Vector3.Lerp(trackedInput, inputController.currentInput, .2f);

        Vector3 finalOffset = Vector3.Lerp(offset, maxOffset, PlayerMovement.instance.enemiesStacked.Count / (.01f + 20));
        Vector3 targetPos = PlayerMovement.instance.transform.position +
            new Vector3(trackedInput.x, 0, trackedInput.y).normalized * movRange
            + finalOffset;

        transform.position = Vector3.Lerp(transform.position, targetPos, movementLerp);
    }
}
