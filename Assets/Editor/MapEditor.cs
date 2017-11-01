using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// The CustomEditor directive specifies which class types this Editor modification applies to
[CustomEditor (typeof(MapGenerator))]

// something
public class MapEditor : Editor {
    public override void OnInspectorGUI() {
        MapGenerator map = target as MapGenerator;
        // Only re-generate the map when an inspector value changes
        if (DrawDefaultInspector()) {
            map.GenerateMap();
        }

        // Add a button to re-generate the map
        if (GUILayout.Button("Generate Map")) {
            map.GenerateMap();
        }
    }
}
