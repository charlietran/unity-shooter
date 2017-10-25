using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
     
    public Wave[] waves;
    public Enemy enemy;

    Wave currentWave;
    int currentWaveNumber;
     
    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;


    void Start()
    {
        NextWave();
    }

    void Update()
    {
        if (enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy() {
        enemiesRemainingToSpawn--;
        nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;
        Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity);
        spawnedEnemy.OnDeath += OnEnemyDeath;
    }

    void OnEnemyDeath ()
    {
        enemiesRemainingAlive--;
        if (enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }

    void NextWave()
    {
        if (currentWaveNumber < waves.Length)
        {
            currentWaveNumber++;
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.EnemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;
        }
    }

    [System.Serializable] 
    public class Wave
    {
        public int EnemyCount;
        public float timeBetweenSpawns;


    }
}
