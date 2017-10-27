using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

// The CustomEditor directive specifies which class types this Editor modification applies to
[CustomEditor (typeof(MapGenerator))]

// something
public class MapEditor : Editor {
    public override void OnInspectorGUI() {
        base.OnInspectorGUI();
        MapGenerator map = target as MapGenerator;
        map.GenerateMap();
    }
}
