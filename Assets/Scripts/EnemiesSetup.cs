using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Start()
    {
        // Spawn inital enemies
        for (int i = 0; i < initialCount; i++)
        {
            SpawnEnemy();
        }

        StartCoroutine(IntervalSpawnEnemy());
    }

    private IEnumerator IntervalSpawnEnemy()
    {
        if (enemies.Count < maxCount) SpawnEnemy();
        yield return new WaitForSeconds(spawnInterval);
        StartCoroutine(IntervalSpawnEnemy());
    }

    public void SpawnEnemy()
    {
        Vector3 spawnPos = Random.insideUnitSphere * randomRadius + transform.position;
        spawnPos.y = 0;

        GameObject enemyClone =
            Instantiate(enemyPrefab, spawnPos, Quaternion.Euler(0, 0, 0), this.transform);

        enemies.Add(enemyClone.GetComponent<EnemyRagdoll>());
    }
}
