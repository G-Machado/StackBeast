using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StackedItem : MonoBehaviour
{
    [SerializeField] private Rigidbody rb;
    [SerializeField] private float floatVelocity = 10;
    private Vector3 offsetPos;

    void Start()
    {
        offsetPos = transform.position - PlayerMovement.instance.transform.position;
    }

    void Update()
    {
        Vector3 originDirection = offsetPos + PlayerMovement.instance.transform.position - transform.position;
        rb.velocity = new Vector3(0, floatVelocity, 0);
    }
}
