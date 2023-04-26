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
        SetupInitiaPose();
        SetRagdoll(false);
    }

    private void FixedUpdate()
    {
        FollowItemPos();
    }


    public void ApplyExplosion(Vector3 forceVector, Vector3 origin, float radius)
    {
        // Apply explosion physics effect to every part of the enemy ragdoll
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].AddExplosionForce(forceVector.magnitude, origin, radius, 3, ForceMode.Impulse);
        }

        bodyRB.useGravity = false;
        punched = true;
        sphereCollider.enabled = false;
    }

    private void FollowItemPos()
    {
        // If there is no item to follow, don't run
        if (!followItem) return;

        // Check for locking distance to target
        float distanceToItem = Vector3.Distance(followItem.transform.position, bodyRB.transform.position);
        if (!locked && distanceToItem < (!dropped ? .5f : 3.5f))
        {
            locked = true;
            SetTrails(false);

            // If is dropped from player, disable enemy
            if (dropped)
            {
                DropDead();
                return;
            }
            else // else lock the enemy at player stack
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


    // Ragdoll methods

    private void SetupInitiaPose()
    {
        // Stores ragdoll parts inital position and rotation
        initialPartsPos = new Vector3[parts.Length];
        initialPartsRot = new Quaternion[parts.Length];
        for (int i = 0; i < parts.Length; i++)
        {
            initialPartsPos[i] = parts[i].position;
            initialPartsRot[i] = parts[i].rotation;
        }
    }

    public void SetRagdoll(bool mode)
    {
        ragActive = mode;

        // Configures the ragdoll according to mode
        anim.enabled = !mode;
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].isKinematic = !mode;
        }

        // Configure trail effect 
        SetTrails(mode);
    }

    private void SetDrag(float value)
    {
        // Configure the drag of every part of the radgoll
        for (int i = 0; i < parts.Length; i++)
        {
            parts[i].drag = value;
        }
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


    // Drop mechanics

    private void DropDead()
    {
        followItem = null;

        // Configure rigidbodies
        SetDrag(1);
        bodyRB.useGravity = true;
        bodyRB.mass = 10;
        bodyRB.drag = 4;
        bodyRB.constraints = RigidbodyConstraints.None;

        // Update enemies list from setup
        EnemiesSetup.instance.enemies.Remove(this);

        Invoke("ReleaseFromPool", 5f);

        // Add currency to enemy collected
        UpgradeManager.instance.AddCurrency(5);
    }

    public void DropToPit(float dropUpSpeed)
    {
        // Configure rigid body to drop
        SetDrag(2);
        bodyRB.constraints = RigidbodyConstraints.None;
        bodyRB.velocity = Vector3.up * dropUpSpeed;

        locked = false;
        dropped = true;
    }


    // Pooling methods

    private void ReleaseFromPool()
    {
        EnemiesSetup.instance.enemyPool.Release(this);
    }

    public void ResetRagdoll()
    {
        // Reset stats
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
