using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.UI.Image;

public class EnemyRagdoll : MonoBehaviour
{
    [SerializeField]private Rigidbody[] parts;
    private Vector3[] initialPartsPos;
    private Quaternion[] initialPartsRot;

    [SerializeField] private bool ragActive = false;
    [SerializeField] private Animator anim;
    public Transform followItem;
    [SerializeField] private SphereCollider sphereCollider;
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
        initialPartsPos = new Vector3[parts.Length];
        initialPartsRot = new Quaternion[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            initialPartsPos[i] = parts[i].position;
            initialPartsRot[i] = parts[i].rotation;
        }

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
        sphereCollider.enabled = false;
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

        // Checks for locking distance to target
        float distanceToItem = Vector3.Distance(followItem.transform.position, bodyRB.transform.position);
        if (!locked && distanceToItem < (!dropped ? .5f : 3.5f))
        {
            locked = true;
            SetTrails(false);

            if (dropped)
            {
                DropDead();
                return;
            }
            else
            {
                SetDrag(5);
                bodyRB.constraints = RigidbodyConstraints.FreezePosition;
            }
        }

        // Move enemy to target position depending on state (i.e. locked or unlocked)
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

    private void DropDead()
    {
        followItem = null;

        // Configure rigidbodies
        SetDrag(1);
        bodyRB.useGravity = true;
        bodyRB.mass = 10;
        bodyRB.drag = 4;
        bodyRB.constraints = RigidbodyConstraints.None;

        EnemiesSetup.instance.enemies.Remove(this);

        Invoke("ReleaseFromPool", 5f);
        //Destroy(this.gameObject, 10f);

        UpgradeManager.instance.AddCurrency(5);
    }

    private void ReleaseFromPool()
    {
        EnemiesSetup.instance.enemyPool.Release(this);
    }

    public void DropToPit(float dropUpSpeed)
    {
        SetDrag(2);
        bodyRB.constraints = RigidbodyConstraints.None;
        bodyRB.velocity = Vector3.up * dropUpSpeed;
        locked = false;
        dropped = true;
    }

    public void ResetRagdoll()
    {
        dropped = false;
        locked = false;
        punched = false;
        sphereCollider.enabled = true;

        gameObject.SetActive(true);
    }

    public void FreezeRagdoll()
    {
        gameObject.SetActive(false);

        // Turn off ragdoll properties
        SetRagdoll(false);

        // Resets position of ragdoll parts
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].gameObject.transform.position = initialPartsPos[i];
            parts[i].gameObject.transform.rotation = initialPartsRot[i];
        }
    }
}
