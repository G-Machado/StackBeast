using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemsSetup : MonoBehaviour
{
    [SerializeField] private int initialCount = 5;
    [SerializeField] private float yOffset = 5;
    [SerializeField] private float yInterval = 5;
    [SerializeField] private GameObject itemPrefab;
    public List<Rigidbody> items = new List<Rigidbody>();

    void Start()
    {
        // Spawn inital fruits
        for (int i = 0; i < initialCount; i++)
        {
            SpawnItem();
        }

        // Configure hinge joints
        HingeJoint[] joints = GetComponentsInChildren<HingeJoint>();
        for (int i = 0; i < joints.Length; i++)
        {
            // Configure spring damp increasing with height
            JointSpring spring = new JointSpring();
            spring.damper = 5 + .5f * i;
            spring.spring = 700 + i * 20;
            //joints[i].spring = spring;

            // Configure rb's drag increasing with heigth
            //joints[i].GetComponent<Rigidbody>().drag = 2 + i * .5f;

            joints[i].GetComponent<Rigidbody>().isKinematic = false;
        }

    }

    private void SpawnItem()
    {
        Vector3 spawnPos = new Vector3(PlayerMovement.instance.transform.position.x,
            yOffset + items.Count * yInterval, 0);

        GameObject itemClone =
            Instantiate(itemPrefab, spawnPos, Quaternion.Euler(-90, 0, 90), this.transform);

        HingeJoint[] hJoints = itemClone.GetComponents<HingeJoint>();
        for (int i = 0; i < hJoints.Length; i++)
        {
            hJoints[i].connectedBody =
                items.Count <= 0 ? PlayerMovement.instance.GetComponent<Rigidbody>() : items[items.Count - 1];
        }

        items.Add(itemClone.GetComponent<Rigidbody>());
    }
}
