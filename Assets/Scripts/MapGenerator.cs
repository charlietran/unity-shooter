using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Vector2))]
[RequireComponent (typeof(Transform))]
public class MapGenerator : MonoBehaviour {
    public Transform tilePrefab;
    public Transform obstaclePrefab;

    // A 2D vector that describes the dimensions of our map
    public Vector2 mapSize;

    float mapTopBoundary;
    float mapLeftBoundary;

    public int obstacleSeed = 10;

    // This list will hold all possible tile coordinates that obstacles and go into
    List<Coord> allTileCoords;

    // This will be a shuffled queue of all possible tile coordinates
    Queue<Coord> shuffledTileCoords;

    // How many obstacles to create
    public int obstacleCount = 10;

    // This value dictates how much to "shrink" our instantiated tiles
    [Range(0,1)]
    public float tilePadding;

    // Used for identitifying our generated map so we can easily destroy/recreate it from our custom editor tool
    const string mapHolderName = "Generated Map";

    void Start() { GenerateMap(); }

    // Generates a map of our mapSize from our tilePrefabs
    public void GenerateMap() {
        InitAllTileCoords();
        Transform mapHolder = CreateMapHolder();

        float tileWidth = tilePrefab.transform.localScale.x;
        float tileHeight = tilePrefab.transform.localScale.y;

        // These relative positions of the top and left boundaries from our generation origin
        // Shifted inward by half a tile to compensate for center-based positioning
        mapTopBoundary = -mapSize.y / 2.0f + 0.5f * tileHeight;
        mapLeftBoundary = -mapSize.x / 2.0f + 0.5f * tileWidth;

        InstantiateTiles(mapHolder);
        InstantiateObstacles(mapHolder);
    }

    // Loop through our map's X/Y size and instantiate a tilePrefab at each point
    private void InstantiateTiles(Transform mapHolder) {
        for (int tileX = 0; tileX < mapSize.x; tileX++) {
            for (int tileY = 0; tileY < mapSize.y; tileY++) {
                Vector3 newTilePosition = PositionFromCoord(tileX, tileY);
                Transform newTile = Instantiate(
                                        tilePrefab,
                                        newTilePosition,
                                        Quaternion.Euler(Vector3.right * 90)
                                    );
                // Reduce the size of the new tile by our specified padding amount
                newTile.localScale = Vector3.one * (1 - tilePadding);
                newTile.parent = mapHolder;
            }
        }
    }

    private void InstantiateObstacles(Transform mapHolder) {
        for (int i = 0; i < obstacleCount; i++) {
            Coord randomCoord = GetRandomCoord();
            Vector3 obstaclePosition = PositionFromCoord(randomCoord.x, randomCoord.y);
            Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity);
            newObstacle.parent = mapHolder;
        }
    }


    Vector3 PositionFromCoord(int x, int y) {
        return new Vector3( mapLeftBoundary + x, 0, mapTopBoundary + y );
    }

    private void InitAllTileCoords() {
        // Initialize with an empty Coord List
        allTileCoords = new List<Coord>();

        // Fill the list with Coords corresopnding to the dimensions of our map
        for (int tileX = 0; tileX < mapSize.x; tileX++) {
            for (int tileY = 0; tileY < mapSize.y; tileY++) {
                allTileCoords.Add(new Coord(tileX, tileY));
            }
        }

        // From the list, create a randomized Coord queue of all possible coords
        shuffledTileCoords = new Queue<Coord>(
                                Utility.ShuffleArray(
                                    allTileCoords.ToArray(),
                                    obstacleSeed
                                )
                             );
    }

    // Create an empty game object to hold our map, and nest it under our Map game object
    Transform CreateMapHolder() {
        DestroyExistingMapHolder();
        Transform mapHolder = new GameObject(mapHolderName).transform;
        mapHolder.parent = transform;
        return mapHolder;
    }
    void DestroyExistingMapHolder() {
        Transform existingMapHolder = transform.Find(mapHolderName);
        if (existingMapHolder) {
            DestroyImmediate(existingMapHolder.gameObject);
        }
    }

    public Coord GetRandomCoord() {
        Coord randomCoord = shuffledTileCoords.Dequeue();
        return randomCoord;
    }

    public struct Coord {
        public int x;
        public int y;

        public Coord(int _x, int _y) {
            x = _x;
            y = _y;
        }
    }
}