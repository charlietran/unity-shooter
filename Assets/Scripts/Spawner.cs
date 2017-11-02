﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {
    public Wave[] waves;
    public Enemy enemy;

    Wave currentWave;
    int currentWaveNumber;

    bool spawnerEnabled = true;
     
    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    MapGenerator mapGenerator;

    LivingEntity playerEntity;
    Transform playerTransform;

    float campingCheckInterval = 2f;
    float campingDistanceThreshold = 1.5f;
    float nextCampingCheckTime;
    Vector3 lastPlayerPosition;
    bool playerIsCamping;

    void Start() {
        playerEntity = FindObjectOfType<Player>();
        playerTransform = playerEntity.transform;
        playerEntity.OnDeath += OnPlayerDeath;

        nextCampingCheckTime = Time.time + campingCheckInterval;
        lastPlayerPosition = playerTransform.position;

        mapGenerator = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    void Update() {
        if (spawnerEnabled) {
            CheckForPlayerCamping();

            bool shouldSpawn = enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime;
            if (shouldSpawn) {
                StartCoroutine(SpawnEnemy());
            }
        }
    }

    void CheckForPlayerCamping() {
        if (Time.time > nextCampingCheckTime) {
            nextCampingCheckTime = Time.time + campingCheckInterval;
            playerIsCamping = (Vector3.Distance(playerTransform.position, lastPlayerPosition) < campingDistanceThreshold);
            lastPlayerPosition = playerTransform.position;
        }
    }

    IEnumerator SpawnEnemy() {
        enemiesRemainingToSpawn--;
        nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

        // If the player is camping, the next spawn will happen on the player's tile
        // Otherwise, pick a random tile with the map generator
        Transform spawnTile;
        if (playerIsCamping) {
            spawnTile = mapGenerator.GetTileFromPosition(playerTransform.position);
        } else {
            spawnTile = mapGenerator.GetRandomOpenTile();
        }

        // Flash the tile to alert the player before spawning the enemy
        yield return StartCoroutine(FlashTile(spawnTile));

        // Instantiate the enemy on our chosen tile, shifted upward so that its bottom is on the tile
        Enemy spawnedEnemy = Instantiate(enemy, spawnTile.position + Vector3.up, Quaternion.identity);
        spawnedEnemy.OnDeath += OnEnemyDeath;
    }

    IEnumerator FlashTile(Transform tile) {
        float flashTimer = 0;
        float flashDuration = 1.0f;
        float flashSpeed = 4.0f;

        Material tileMaterial = tile.GetComponent<Renderer>().material;
        Color initialTileColor = tileMaterial.color;
        Color flashColor = Color.red;

        while (flashTimer < flashDuration) {
            flashTimer += Time.deltaTime;
            tileMaterial.color = Color.Lerp(
                                     initialTileColor, 
                                     flashColor, 
                                     Mathf.PingPong(flashTimer * flashSpeed, 1)
                                 );
            yield return null;
        }
    }
    
    void OnPlayerDeath () {
        spawnerEnabled = false;
    }

    void OnEnemyDeath () {
        enemiesRemainingAlive--;
        if (enemiesRemainingAlive == 0) {
            NextWave();
        }
    }

    void NextWave() {
        if (currentWaveNumber < waves.Length) {
            currentWaveNumber++;
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.EnemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;
        }
    }

    [System.Serializable] 
    public class Wave {
        public int EnemyCount;
        public float timeBetweenSpawns;
    }
}
