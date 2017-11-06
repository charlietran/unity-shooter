using System.Collections;
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
    public Cinemachine.CinemachineVirtualCamera playerCamera;
    Vector3 initialCameraPosition;
    Quaternion initialCameraRotation;

    float campingCheckInterval = 2f;
    float campingDistanceThreshold = 1.5f;
    float nextCampingCheckTime;
    Vector3 lastPlayerPosition;
    bool playerIsCamping;

    public event System.Action<int> OnNewWave;

    void Start() {
        PlayerSetup();

        mapGenerator = FindObjectOfType<MapGenerator>();
        NextWave();
    }

    void Update() {
        if (spawnerEnabled) {
            CheckPlayerCamping();

            bool shouldSpawn = enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime;
            if (shouldSpawn) {
                StartCoroutine(SpawnEnemy());
            }
        }
    }

    void PlayerSetup() {
        playerEntity = FindObjectOfType<Player>();
        playerTransform = playerEntity.transform;
        playerEntity.OnDeath += OnPlayerDeath;
        initialCameraPosition = playerCamera.transform.position;
        initialCameraRotation = playerCamera.transform.rotation;

        // Initialize our first camping check time and player position 
        nextCampingCheckTime = Time.time + campingCheckInterval;
        lastPlayerPosition = playerTransform.position;
    }

    void CheckPlayerCamping() {
        if (Time.time > nextCampingCheckTime) {
            nextCampingCheckTime = Time.time + campingCheckInterval;
            // The player is camping if they have not moved our distance theshold since we last checked their position
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
        float flashSpeed = 4.0f; // Arbitrary multiplier to control the file flashing speed

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
        // Disable all spawning events, which are irrelevant / nonvalid once there is no player
        spawnerEnabled = false;
    }

    void OnEnemyDeath () {
        enemiesRemainingAlive--;
        if (enemiesRemainingAlive == 0) {
            NextWave();
        }
    }

    void ResetPlayerPosition() {
        // Move the player back to slightly above the center of the map
        playerTransform.position = mapGenerator.GetTileFromPosition(Vector3.zero).position + Vector3.up * 3;
        playerCamera.transform.position = initialCameraPosition;
        playerCamera.transform.rotation = initialCameraRotation;
    }

    void NextWave() {
        if (currentWaveNumber < waves.Length) {
            currentWaveNumber++;
            currentWave = waves[currentWaveNumber - 1];
            enemiesRemainingToSpawn = currentWave.EnemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;

            if (OnNewWave != null) {
                OnNewWave(currentWaveNumber);
            }

            ResetPlayerPosition();
        }
    }


    [System.Serializable] 
    public class Wave {
        public int EnemyCount;
        public float timeBetweenSpawns;
    }
}
