using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Vector2))]
[RequireComponent (typeof(Transform))]
public class MapGenerator : MonoBehaviour {
    // Allow multiple maps to be defined, and one to be manually selected
    public Map[] maps;
    public int mapIndex;
    Map currentMap;
    Transform mapHolder;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;

    public float tileSize; 

    float mapTopBoundary;
    float mapLeftBoundary;

    // This will be a shuffled queue of all possible tile coordinates
    Queue<Coord> shuffledPossibleObstacleCoords;

    // This value dictates how much to "shrink" our instantiated tiles
    [Range(0,1)]
    public float tilePadding;

    // Used for identitifying our generated map so we can easily destroy/recreate it from our custom editor tool
    const string mapHolderName = "Generated Map";

    // 2D array that will hold the transform of each instantiated map tile
    Transform[,] tileMap;

    // This list and queue will hold all non-obstacle coords into which an enemy may spawn
    List<Coord> openSpawningCoords;
    Queue<Coord> shuffledOpenTileCoords;

    void Start() { GenerateMap(); }

    // Generates a map of our currentMap.mapSize from our tilePrefabs
    public void GenerateMap() {
        currentMap = maps[mapIndex];

        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        // These relative positions of the top and left boundaries from our generation origin
        // Shifted inward by half a tile to compensate for center-based positioning
        mapTopBoundary = -currentMap.mapSize.y / 2.0f + 0.5f;
        mapLeftBoundary = -currentMap.mapSize.x / 2.0f + 0.5f;

        // Set the size of our collider and nav mesh based on tileSize
        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, .05f, currentMap.mapSize.y * tileSize);
        navmeshFloor.localScale = new Vector3(currentMap.mapSize.x, currentMap.mapSize.y) * tileSize;

        CreateMapHolder();
        InstantiateTiles(mapHolder);

        InitCoords();
        InstantiateAllObstacles();

        // Create a shuffled queue of all non-obstacle tiles that an enemy can spawn into
        shuffledOpenTileCoords = new Queue<Coord>(Utility.ShuffleArray(openSpawningCoords.ToArray(), currentMap.obstacleSeed));
    }

    // Loop through our map's X/Y size and instantiate a tilePrefab at each point
    private void InstantiateTiles(Transform mapHolder) {
        for (int tileX = 0; tileX < currentMap.mapSize.x; tileX++) {
            for (int tileY = 0; tileY < currentMap.mapSize.y; tileY++) {
                Vector3 newTilePosition = CoordToPosition(tileX, tileY);
                Transform newTile = Instantiate(
                                        tilePrefab,
                                        newTilePosition,
                                        Quaternion.Euler(Vector3.right * 90)
                                    );
                // Reduce the size of the new tile by our specified padding amount
                newTile.localScale = Vector3.one * tileSize * (1 - tilePadding);
                newTile.parent = mapHolder;
                tileMap[tileX, tileY] = newTile;
            }
        }
    }

    private void InstantiateAllObstacles() {
        // 2D Array that represents our whole map, and which of its tiles are obstacles
        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        // Our max number of obstacles, based on total number of tiles and obstaclePercentage
        int obstacleMax = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercentage);
        int obstacleCount = 0;

        // RNG used for randomizing obstacle height
        System.Random rng = new System.Random(currentMap.obstacleSeed);

        for (int i = 0; i < obstacleMax; i++) {
            // Get a random coordinate and try it as an onstacle
            Coord randomCoord = GetRandomCoord();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            obstacleCount++;

            // Check if the map remains accessible after adding this obstacle
            bool accessible = MapIsFullyAccessible(obstacleMap, obstacleCount);

            if (accessible) {
                InstantiateObstacle(randomCoord, rng);
            } else {
                // If the obstacle cannot be placed, mark the obstacle map as such and undo our obstacle count increment
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                obstacleCount--;
            }
        }
    }

    private void InstantiateObstacle(Coord randomCoord, System.Random rng) {
        // Randomize obstacle height based on our min / max height inputs 
        float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)rng.NextDouble());

        // Obstacle's position will be our random coord, with the center shifted upward to account for obstacle height
        Vector3 obstaclePosition = CoordToPosition(randomCoord) + Vector3.up * obstacleHeight / 2;

        // Instantiate the obstacle and nest it under our map holder
        Transform newObstacle = Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
        newObstacle.parent = mapHolder;

        // Mark this coord as ineligible for enemy spawning
        openSpawningCoords.Remove(randomCoord);

        // Size our obstacle based on our given tile size and tile padding
        float tileLength = tileSize * (1 - tilePadding);
        newObstacle.localScale = new Vector3(tileLength, obstacleHeight, tileLength);

        // Give the obstacle a color based on its position, interpolated from our defined bg/fg colors
        // This is to create a gradient effect on the obstacles
        Renderer obstacleRenderer = newObstacle.GetComponent<Renderer>();
        Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
        float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
        obstacleMaterial.color = Color.Lerp(currentMap.fgColor, currentMap.bgColor, colorPercent);
        obstacleRenderer.sharedMaterial = obstacleMaterial;
    }

    // Use a flood fill algorithm to make sure no part of the map is blocked off
    bool MapIsFullyAccessible(bool[,] obstacleMap, int obstacleCount) {
        int obstacleMapWidth = obstacleMap.GetLength(0);
        int obstacleMapHeight = obstacleMap.GetLength(1);
        Queue<Coord> accessibilityValidationQueue = new Queue<Coord>();

        // Keep an array of coordinates that we've already checked
        bool[,] validatedTiles = new bool[obstacleMapWidth, obstacleMapHeight];
        validatedTiles[currentMap.mapCenter.x, currentMap.mapCenter.y] = true;

        // Start our validation queue with the map center
        accessibilityValidationQueue.Enqueue(currentMap.mapCenter);
        int accessibleTileCount = 1;

        // Our desired number of accessible tiles is the total number of tiles minus the obstacle count
        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - obstacleCount);

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


    // CoordToPosition translates a simple x/y Coord into a Vector3 usable for positioning game objects
    Vector3 CoordToPosition(Coord coord) {
        return new Vector3(mapLeftBoundary + coord.x, 0, mapTopBoundary + coord.y) * tileSize;
    }

    Vector3 CoordToPosition(int x, int y) {
        return CoordToPosition(new Coord(x, y));
    }

    // Given a position, get the transform of the tile that it's on, i.e. find out what tile a player is currently on
    public Transform GetTileFromPosition(Vector3 position) {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1) / 2f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1) / 2f);
        int mapWidth = tileMap.GetLength(0);
        int mapHeight = tileMap.GetLength(1);

        // Clamp our possible return values to within the possible indexes of the tile map
        x = Mathf.Clamp(x, 0, mapWidth - 1);
        y = Mathf.Clamp(y, 0, mapHeight - 1);
        return tileMap[x, y];
    }

    private void InitCoords() {
        // This list will hold all possible tile coordinates where obstacles may be instantiated
        List<Coord> possibleObstacleCoords = new List<Coord>();

        // Fill the list with Coords corresponding to the dimensions of our map, excluding the center
        // (The center tile will always be available as that's the player spawning point)
        for (int tileX = 0; tileX < currentMap.mapSize.x; tileX++) {
            for (int tileY = 0; tileY < currentMap.mapSize.y; tileY++) {
                if (tileX != currentMap.mapCenter.x && tileY != currentMap.mapCenter.y) {
                    possibleObstacleCoords.Add(new Coord(tileX, tileY));
                }
            }
        }

        // Initially, the list of all possible enemy spawning coords is the same as all possible obstacle coords
        openSpawningCoords = possibleObstacleCoords;

        // From the list, create a randomized queue of all possible obstacle coords
        shuffledPossibleObstacleCoords = new Queue<Coord>(
                                            Utility.ShuffleArray(
                                                possibleObstacleCoords.ToArray(), currentMap.obstacleSeed
                                            )
                                         );
    }

    public Transform GetRandomOpenTile() {
        Coord randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);
        return tileMap[randomCoord.x, randomCoord.y];
    }

    // Create an empty game object to hold our map, and nest it under our Map game object
    void CreateMapHolder() {
        DestroyExistingMapHolder();
        mapHolder = new GameObject(mapHolderName).transform;
        mapHolder.parent = transform;
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

    // This struct represents an x/y coordinate on our 2D tile map
    [System.Serializable]
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

    [System.Serializable]
    public class Map {
        public Coord mapSize;
        public Color fgColor;
        public Color bgColor;

        // The percentage of tiles that will be obstacles
        [Range(0,1)]
        public float obstaclePercentage;

        public float minObstacleHeight;
        public float maxObstacleHeight;

        // Arbitrary starting seed for our obstacle randomization
        public int obstacleSeed = 10;

        public Coord mapCenter {
            get {
                return new Coord(mapSize.x / 2, mapSize.y / 2);
            }
        }

    }
}