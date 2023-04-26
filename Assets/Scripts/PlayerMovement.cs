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
    [SerializeField] private float punchMovementSpeed;

    [SerializeField] private Animator anim;

    [Header("Input Variables")]
    [SerializeField] private JoyStickController controller;

    [Header("Enemies Punching")]
    private bool isPunching = false;
    [SerializeField] private Transform punchHand;

    [Header("Enemies Stacking")]
    [SerializeField] private ItemsSetup itemsSetup;
    public List<EnemyRagdoll> enemiesStacked = new List<EnemyRagdoll>();
    private int maxEnemiesStacked
    {
        get
        {
            return UpgradeManager.instance == null ? 3 :
                        UpgradeManager.instance.currentStackStats.maxStack;
        }
    }

    [Header("Enemies Droping")]
    private bool isDroping = false;
    [SerializeField] private float dropForce;
    [SerializeField] private float dropInterval;
    [SerializeField] private Transform dropPit;
    private Coroutine dropEnemiesRoutine;
    private float targetTypeBlend;
    [Range(0,1)]
    [SerializeField] private float typeLerp;

    void FixedUpdate()
    {
        MoveAndRotate();
        UpdateAnim();
    }

    // -- Movement logics

    private void MoveAndRotate()
    {
        // Calculates next position to target
        float finalSpeed = isPunching ? punchMovementSpeed : movementSpeed;
        Vector3 targetPos =
            new Vector3(transform.position.x + (controller.currentInput.x * finalSpeed * .01f),
            0,
            transform.position.z + (controller.currentInput.y * finalSpeed * .01f));
        transform.position = Vector3.Lerp(transform.position, targetPos, .2f);

        // Adjust rotation to movement direction
        Quaternion targetRot = Quaternion.LookRotation(targetPos - transform.position);
        targetRot.z = 0;
        targetRot.x = 0;

        float finalRotLerp = isPunching ? .05f : .3f;
        if (controller.currentInput.magnitude > .2f)
            anim.transform.rotation = Quaternion.Lerp(anim.transform.rotation, targetRot, finalRotLerp);
    }

    private void UpdateAnim()
    {
        // Configure animation blends
        anim.SetFloat("MovBlend", controller.currentInput.magnitude / 100);
        anim.SetFloat("TypeBlend", Mathf.Lerp(anim.GetFloat("TypeBlend"), targetTypeBlend, typeLerp));
    }


    // -- Collision interactions

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && !isPunching)
        {
            StartPunch();
        }
        else if (other.tag == "EnemyFallen" && !isPunching)
        {
            // Try collect falled enemy ( if has space at stack )
            EnemyRagdoll enemy = other.transform.parent.GetComponentInParent<EnemyRagdoll>();
            if(!enemiesStacked.Contains(enemy) && !enemy.dropped) TryCollectEnemy(enemy);
        }
        else if (other.tag == "DropArea" && !isDroping)
        {
            // Start dropping enemies at pit
            dropPit = other.transform.GetChild(0);
            targetTypeBlend = 1;

            isDroping = true;
            dropEnemiesRoutine = StartCoroutine(DropEnemies());
        }
        else if (other.tag == "UpgradeArea")
        {
            // Trigger the attempt to purchase
            targetTypeBlend = 1;
            other.GetComponent<UpgradeArea>().TryPurchaseUpgrade();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "DropArea" && isDroping)
        {
            // Stop dropping the enemies
            targetTypeBlend = 0;
            isDroping = false;
            if(dropEnemiesRoutine != null) StopCoroutine(dropEnemiesRoutine);
        }
        else if (other.tag == "UpgradeArea")
        {
            // Triggers activation when leaving upgrade area
            targetTypeBlend = 0;
            other.GetComponent<UpgradeArea>().ExitArea();
        }
    }


    // -- Punch mechanics

    private void StartPunch()
    {
        // Start punch animation
        anim.SetTrigger("PunchTrigger");
        isPunching = true;

        // Call end of punch state
        Invoke("SetUnPunch", 1.1f);
    }

    public void ApplyPunchEffect()
    {
        // Calculate punch values
        float punchRadius = UpgradeManager.instance.currentPunchStats.punchRadius;
        float punchForce = UpgradeManager.instance.currentPunchStats.punchForce;

        // Get enemies in punch explosion area and apply effect
        Collider[] enemies = Physics.OverlapSphere(punchHand.position, punchRadius * .6f, 1 << 8);
        for (int i = 0; i < enemies.Length; i++)
        {
            EnemyRagdoll currentEnemy = enemies[i].GetComponent<EnemyRagdoll>();
            currentEnemy.SetRagdoll(true);
            currentEnemy.ApplyExplosion((currentEnemy.transform.position - transform.position).normalized * punchForce,
                punchHand.position, punchRadius);

            SpawnPunchExplosionFX();

            // Try collect punched enemy ( if has space at stack )
            TryCollectEnemy(currentEnemy);
        }
    }

    private bool TryCollectEnemy(EnemyRagdoll currentEnemy)
    {
        if (enemiesStacked.Count < maxEnemiesStacked)
        {
            int followIndex = enemiesStacked.Count;
            currentEnemy.followItem = itemsSetup.items[followIndex].transform;

            enemiesStacked.Add(currentEnemy);
            UpgradeManager.instance.UpdateStackUI();

            return true;
        }

        return false;
    }

    private void SetUnPunch()
    {
        isPunching = false;
    }

    private void SpawnPunchExplosionFX()
    {
        GameObject explosionClone =
            Instantiate(UpgradeManager.instance.currentPunchStats.explosionEffect, punchHand.position, Quaternion.identity);
        Destroy(explosionClone, 2f);
    }

    // -- Droping mechanics

    private IEnumerator DropEnemies()
    {
        // While there are enemies at player's stack, keep droping them at 'dropInterval'
        if (enemiesStacked.Count > 0 && isDroping)
        {
            DropEnemy();
            yield return new WaitForSeconds(dropInterval);
            StartCoroutine(DropEnemies());
        }
    }

    private void DropEnemy()
    {
        if (enemiesStacked.Count <= 0) return;

        EnemyRagdoll toDrop = enemiesStacked[enemiesStacked.Count-1];

        // Configure and triggers enemy ragdoll to drop at pit
        toDrop.SetRagdoll(true);
        toDrop.followItem = dropPit;
        toDrop.DropToPit(dropForce * (dropPit.position - toDrop.transform.position).magnitude);

        enemiesStacked.RemoveAt(enemiesStacked.Count - 1);
        UpgradeManager.instance.UpdateStackUI();
    }
}
