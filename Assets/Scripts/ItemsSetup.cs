using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class ItemsSetup : MonoBehaviour
{
    public static ItemsSetup instance;
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    [Header("Spawn Variables")]
    [SerializeField] private GameObject itemPrefab;
    [SerializeField] private Vector3 offset;

    [Header("Stack Varialbes")]
    [SerializeField] private int initialCount = 5;
    [SerializeField] private float yInterval = 5;
    public List<Rigidbody> items = new List<Rigidbody>();
    [SerializeField] private Rigidbody playerRB;

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

            joints[i].GetComponent<Rigidbody>().isKinematic = false;
        }

        UpgradeManager.instance.InitializeUpgrades();
    }

    public void SpawnItem()
    {
        Vector3 spawnPos = new Vector3(PlayerMovement.instance.transform.position.x,
            offset.y + items.Count * yInterval, offset.z);

        GameObject itemClone =
            Instantiate(itemPrefab, spawnPos, Quaternion.Euler(-90, 0, 90), this.transform);

        // Configure the hinge joints present at stack item clone
        HingeJoint[] hJoints = itemClone.GetComponents<HingeJoint>();
        for (int i = 0; i < hJoints.Length; i++)
        {
            hJoints[i].connectedBody =
                items.Count <= 0 ? playerRB : items[items.Count - 1];
        }

        items.Add(itemClone.GetComponent<Rigidbody>());
    }
}
