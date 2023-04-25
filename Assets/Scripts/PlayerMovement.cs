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
    //[SerializeField] private float inputGravity = 1;
    //private Vector3 inputStartPos;
    //private Vector3 currentInput;
    //private bool mouseDown;

    [Header("Enemies Punching")]
    private bool isPunching = false;
    [SerializeField] private Transform punchHand;
    [SerializeField] private float punchRadius;
    [SerializeField] private float punchForce;
    private EnemyRagdoll currentEnemy;

    [Header("Enemies Stacking")]
    [SerializeField] private ItemsSetup itemsSetup;
    [SerializeField] private List<EnemyRagdoll> enemiesStacked = new List<EnemyRagdoll>();
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

    void Start()
    {

    }

    void FixedUpdate()
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

        float finalRotLerp = isPunching ? .05f : .2f;
        if (controller.currentInput.magnitude > .2f)
            anim.transform.rotation = Quaternion.Lerp(anim.transform.rotation, targetRot, finalRotLerp);

        // Configure animation 
        anim.SetFloat("MovBlend", controller.currentInput.magnitude/100);
        anim.SetFloat("TypeBlend", Mathf.Lerp(anim.GetFloat("TypeBlend"), targetTypeBlend, typeLerp));


        // Droping
        if (Input.GetKeyDown(KeyCode.D)) DropEnemy();

    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Enemy" && !isPunching)
        {
            currentEnemy = other.GetComponent<EnemyRagdoll>();
            anim.SetTrigger("PunchTrigger");
            isPunching = true;
            Invoke("SetUnPunch", 1.1f);
        }
        else if (other.tag == "DropArea" && !isDroping)
        {
            targetTypeBlend = 1;

            isDroping = true;
            dropEnemiesRoutine = StartCoroutine(DropEnemies());
        }
        else if (other.tag == "UpgradeArea")
        {
            targetTypeBlend = 1;

            other.GetComponent<UpgradeArea>().PurchaseUpgrade();
        }

    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "DropArea" && isDroping)
        {
            targetTypeBlend = 0;

            isDroping = false;
            if(dropEnemiesRoutine != null) StopCoroutine(dropEnemiesRoutine);
        }
        else if (other.tag == "UpgradeArea")
        {
            targetTypeBlend = 0;
            other.GetComponent<UpgradeArea>().ExitArea();
        }
    }

    public void ApplyPunchEffect()
    {
        currentEnemy.SetRagdoll(true);
        currentEnemy.ApplyExplosion((currentEnemy.transform.position - transform.position).normalized * punchForce,
            punchHand.position, punchRadius);

        if (enemiesStacked.Count < maxEnemiesStacked)
        {
            int followIndex = enemiesStacked.Count;
            currentEnemy.followItem = itemsSetup.items[followIndex].transform;

            enemiesStacked.Add(currentEnemy);
        }
    }

    private void SetUnPunch()
    {
        isPunching = false;
    }


    private IEnumerator DropEnemies()
    {
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

        Vector3 dropDirection = dropPit.position - toDrop.transform.position;
        toDrop.SetRagdoll(true);
        toDrop.followItem = dropPit;
        toDrop.DropToPit(dropForce * dropDirection.magnitude);

        enemiesStacked.RemoveAt(enemiesStacked.Count - 1);
    }
}
