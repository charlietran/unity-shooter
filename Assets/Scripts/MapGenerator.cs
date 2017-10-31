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

    // This will be a shuffled queue of all possible tile coordinates
    Queue<Coord> shuffledPossibleObstacleCoords;

    // The percentage of tiles that will be obstacles
    [Range(0,1)]
    public float obstaclePercentage = 0.5f;

    // Arbitrary starting seed for our obstacle randomization
    public int obstacleSeed = 10;

    // This value dictates how much to "shrink" our instantiated tiles
    [Range(0,1)]
    public float tilePadding;

    // Used for identitifying our generated map so we can easily destroy/recreate it from our custom editor tool
    const string mapHolderName = "Generated Map";

    // This will get set to the center tile of our map. Used for determining map accessibility
    Coord mapCenter;

    void Start() { GenerateMap(); }

    // Generates a map of our mapSize from our tilePrefabs
    public void GenerateMap() {
        InitPossibleObstacleCoords();
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
        // 2D Array that represents our whole map, and which of its tiles are obstacles
        bool[,] obstacleMap = new bool[(int)mapSize.x, (int)mapSize.y];

        int obstacleMax = (int)(mapSize.x * mapSize.y * obstaclePercentage);

        // Counter of how mnay obstacles we've placed on the map
        int obstacleCount = 0;

        for (int i = 0; i < obstacleMax; i++) {
            // Get a random coordinate and try it as an onstacle
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            obstacleCount++;

            // Check if the map remains accessible after adding this obstacle
            bool accessible = MapIsFullyAccessible(obstacleMap, obstacleCount);

            if (accessible) {
                Vector3 obstaclePosition = PositionFromCoord(randomCoord.x, randomCoord.y);
                Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity);
                newObstacle.parent = mapHolder;
            } else {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                obstacleCount--;
            }
        }
    }

    // Use a flood fill algorithm to make sure no part of the map is blocked off
    bool MapIsFullyAccessible(bool[,] obstacleMap, int obstacleCount) {
        int obstacleMapWidth = obstacleMap.GetLength(0);
        int obstacleMapHeight = obstacleMap.GetLength(1);
        Queue<Coord> accessibilityValidationQueue = new Queue<Coord>();

        // Keep an array of coordinates that we've already checked
        bool[,] validatedTiles = new bool[obstacleMapWidth, obstacleMapHeight];
        validatedTiles[mapCenter.x, mapCenter.y] = true;

        // Start our validation queue with the map center
        accessibilityValidationQueue.Enqueue(mapCenter);
        int accessibleTileCount = 1;

        // Our desired number of accessible tiles is the total number of tiles minus the obstacle count
        int targetAccessibleTileCount = (int)(mapSize.x * mapSize.y - obstacleCount);

        // This while loop is a "flood fill" check for all tile availability. Starting from the center tile,
        // it checks to make sure none of its orthogonal neighbors is accessible, i.e. not an obstacle or map boundary. 
        // Then it recursively enqueues each orthogonal coordinate it finds, until the whole map is checked
        while (accessibilityValidationQueue.Count > 0) {
            Coord tile = accessibilityValidationQueue.Dequeue();

            // Check each of the eight neighboring tiles to make sure it's not already filled with an obstacle
            for(int xOffset = -1; xOffset <= 1; xOffset++) {
                for (int yOffset = -1; yOffset <= 1; yOffset++) {
                    // Set the coordinates of the neighbor tile that we're checking
                    int neighborX = tile.x + xOffset;
                    int neighborY = tile.y + yOffset;

                    bool isValidNeighbor = ValidForAccessibilityCheck(obstacleMapWidth, obstacleMapHeight, xOffset, yOffset, neighborX, neighborY);

                    if (isValidNeighbor) {
                        // If we haven't validated this neighbor tile already, and if it's also not an obstacle,
                        // then mark that it's been validated, increase our accessible tile count,
                        // then queue this tile coordinate 
                        if(!validatedTiles[neighborX, neighborY] && !obstacleMap[neighborX, neighborY]) {
                            validatedTiles[neighborX, neighborY] = true;
                            accessibleTileCount++;
                            accessibilityValidationQueue.Enqueue(new Coord(neighborX, neighborY));
                        }
                    }
                }
            }
        }

        // Our accessibility flood-fill check may reveal that it's no longer possible to add obstacles
        // while ensuring accessibility. In which case, the returned accessibleTileCount will be lower than
        // our targetAccessibleTileCount, and we'll return false for the map being fully accessible

        return targetAccessibleTileCount == accessibleTileCount;
    }

    bool ValidForAccessibilityCheck(int obstacleMapWidth, int obstacleMapHeight, int xOffset, int yOffset, int neighborX, int neighborY) {
        // We only want to check orthgonally, which is done by ensuring at least one of the offset axes is zero
        bool orthogonalCheck = (xOffset == 0 || yOffset == 0);

        // Make sure the coordinates of the neighbor tile we're checking are within the bounds of the map
        bool withinBoundaries = (neighborX >= 0 && neighborY >= 0 && neighborX < obstacleMapWidth && neighborY < obstacleMapHeight);

        return orthogonalCheck && withinBoundaries;
    }


    Vector3 PositionFromCoord(int x, int y) {
        return new Vector3( mapLeftBoundary + x, 0, mapTopBoundary + y );
    }

    private void InitPossibleObstacleCoords() {
        // This list will hold all possible tile coordinates where obstacles may be instantiated
        List<Coord> possibleObstacleCoords = new List<Coord>();

        mapCenter = new Coord((int)mapSize.x / 2, (int)mapSize.y / 2);

        // Fill the list with Coords corresponding to the dimensions of our map, excluding the center
        // (The center tile will always be available as that's the player spawning point)
        for (int tileX = 0; tileX < mapSize.x; tileX++) {
            for (int tileY = 0; tileY < mapSize.y; tileY++) {
                // 
                if (tileX != mapCenter.x && tileY != mapCenter.y) {
                    possibleObstacleCoords.Add(new Coord(tileX, tileY));
                }
            }
        }

        // From the list, create a randomized queue of all possible obstacle coords
        shuffledPossibleObstacleCoords = new Queue<Coord>(
                                            Utility.ShuffleArray(
                                                possibleObstacleCoords.ToArray(), obstacleSeed
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
        Coord randomCoord = shuffledPossibleObstacleCoords.Dequeue();
        return randomCoord;
    }

    public struct Coord {
        public int x;
        public int y;

        public Coord(int _x, int _y) {
            x = _x;
            y = _y;
        }

        public static bool operator == (Coord c1, Coord c2) {
            return c1.x == c2.x && c1.y == c2.y;
        }

        public static bool operator != (Coord c1, Coord c2) {
            return !(c1 == c2);
        }
    }
}