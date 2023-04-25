using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class EnemyRagdoll : MonoBehaviour
{
    private Rigidbody[] parts;
    [SerializeField] private bool ragActive = false;
    [SerializeField] private Animator anim;
    public Transform followItem;
    [SerializeField] private Rigidbody bodyRB;
    [SerializeField] private float followSpeed;
    [SerializeField] private float followLerp;
    [SerializeField] private float lockedLerp;

    [SerializeField] private bool locked = false;
    public bool dropped = false;
    public bool punched = false;

    [Header("FX Variables")]
    public TrailRenderer[] trails;

    void Start()
    {
        parts = GetComponentsInChildren<Rigidbody>();
        SetRagdoll(false);
    }

    private void FixedUpdate()
    {
        FollowItemPos();
    }

    public void SetRagdoll(bool mode)
    {
        ragActive = mode;
        anim.enabled = !mode;
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].isKinematic = !mode;
        }

        SetTrails(mode);
    }

    private void SetTrails(bool mode )
    {
        // Setup all trails mode
        for (int i = 0; i < trails.Length; i++)
        {
            trails[i].enabled = mode;
        }

        // If turning on trails, disable 0 to 2 random trails to give some variation
        if(mode)
        {
            for (int i = 0; i < Random.Range(trails.Length/3, trails.Length); i++)
            {
                int randomIndex = Random.Range(0, trails.Length);
                trails[randomIndex].enabled = false;
            }
        }
    }

    public void ApplyExplosion(Vector3 forceVector, Vector3 origin, float radius)
    {
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].AddExplosionForce(forceVector.magnitude, origin, radius, 3, ForceMode.Impulse);
        }

        bodyRB.useGravity = false;
        punched = true;
        GetComponent<SphereCollider>().enabled = false;
    }

    private void SetDrag(float value)
    {
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].drag = value;
        }
    }

    private void FollowItemPos()
    {
        if (!followItem) return;

        float distanceToItem = Vector3.Distance(followItem.transform.position, bodyRB.transform.position);
        if (!locked && distanceToItem < (!dropped ? .5f : 5.5f))
        {
            locked = true;
            SetDrag(5);
            bodyRB.constraints = RigidbodyConstraints.FreezePosition;

            SetTrails(false);

            if (dropped)
            {
                followItem = null;
                bodyRB.useGravity = true;
                UpgradeManager.instance.AddCurrency(5);
                EnemiesSetup.instance.enemies.Remove(this);
                Destroy(this.gameObject, 10f);
                return;
            }
        }

        if (locked)
        {
            bodyRB.MovePosition(Vector3.Lerp(bodyRB.position, followItem.transform.position, lockedLerp));
        }
        else
        {
            SetDrag(Mathf.Max(5 - distanceToItem, .5f));
            Vector3 targetDirection = followItem.transform.position - bodyRB.transform.position;
            bodyRB.velocity =
                Vector3.Lerp(bodyRB.velocity, targetDirection.normalized * followSpeed, followLerp);
        }
    }

    public void DropToPit(float dropUpSpeed)
    {
        SetDrag(2);
        bodyRB.constraints = RigidbodyConstraints.None;
        bodyRB.velocity = Vector3.up * dropUpSpeed;
        locked = false;
        dropped = true;
    }
}
