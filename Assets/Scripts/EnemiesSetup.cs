using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class EnemiesSetup : MonoBehaviour
{
    public static EnemiesSetup instance;
    private void Awake()
    {
        if (instance == null) instance = this;
    }

    [Header("Enemy Spawn")]
    [SerializeField] private int initialCount = 5;
    [SerializeField] private int maxCount = 5;
    [SerializeField] private float randomRadius;
    [SerializeField] private GameObject enemyPrefab;
    [SerializeField] private float spawnInterval;
    public List<EnemyRagdoll> enemies = new List<EnemyRagdoll>();

    [HideInInspector] public IObjectPool<EnemyRagdoll> enemyPool;

    void Start()
    {
        enemyPool = new ObjectPool<EnemyRagdoll>(CreateEnemy, OnTakeFromPool, OnReturnedToPool, OnDestroyPoolObject, true, 10, 100);

        //// Spawn inital enemies
        //for (int i = 0; i < initialCount; i++)
        //{
        //    SpawnEnemy();
        //}

        StartCoroutine(IntervalSpawnEnemy());
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.S)) SpawnEnemy();
    }

    private IEnumerator IntervalSpawnEnemy()
    {
        if (enemies.Count < maxCount) SpawnEnemy();
        yield return new WaitForSeconds(spawnInterval);
        StartCoroutine(IntervalSpawnEnemy());
    }

    public void SpawnEnemy()
    {
        EnemyRagdoll enemy = enemyPool.Get();
        enemies.Add(enemy);

        Vector3 spawnPos = Random.insideUnitSphere * randomRadius + transform.position;
        spawnPos.y = 0;
        enemy.gameObject.transform.position = spawnPos;

        //GameObject enemyClone = 
        //    Instantiate(enemyPrefab, spawnPos, Quaternion.Euler(0, 0, 0), this.transform);
    }

    // Pooling Methods

    EnemyRagdoll CreateEnemy()
    {
        return Instantiate(enemyPrefab, Vector3.zero, Quaternion.Euler(0, 0, 0), this.transform).GetComponent<EnemyRagdoll>();
    }

    void OnTakeFromPool(EnemyRagdoll enemy)
    {
        enemy.ResetRagdoll();
    }

    void OnReturnedToPool(EnemyRagdoll enemy)
    {
        enemy.FreezeRagdoll();
    }

    void OnDestroyPoolObject(EnemyRagdoll enemy)
    {
        Destroy(enemy);
    }
}
