using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent (typeof(Vector2))]
[RequireComponent (typeof(Transform))]
public class MapGenerator : MonoBehaviour {
    public Vector2 mapSize;
    public Transform tilePrefab;


    // This value dictates how much to "shrink" our instantiated tiles
    [Range(0,1)]
    public float tilePadding;

    const string mapHolderName = "Generated Map";

    void Start() { GenerateMap(); }

    // Generates a map of our mapSize from our tilePrefabs
    public void GenerateMap() {
        Transform mapHolder = CreateMapHolder();

        float tileWidth = tilePrefab.transform.localScale.x;
        float tileHeight = tilePrefab.transform.localScale.y;

        // These relative positions of the top and left boundaries from our generation origin
        // Shifted inward by half a tile to compensate for center-based positioning
        float mapTopBoundary = -mapSize.y / 2.0f + 0.5f * tileHeight;
        float mapLeftBoundary = -mapSize.x / 2.0f + 0.5f * tileWidth;

        for (int tileX = 0; tileX < mapSize.x; tileX++) {
            for (int tileY = 0; tileY < mapSize.y; tileY++) {
                Vector3 newTilePosition = new Vector3(
                                              mapLeftBoundary + tileX, 
                                              0, 
                                              mapTopBoundary + tileY
                                          );
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
}