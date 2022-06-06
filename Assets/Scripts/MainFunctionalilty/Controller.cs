using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class Controller : EditorWindow {

    // VARIABLES (underscored ones for previous state comparison)
    private GameObject mainframe = null;

    // General Section:
    private float iterationTime = 1f;
    private int iterationMax = 100;
    private bool cancelIterations = false;
    private bool verified = false;

    private float minIterationTime = 0f;
    
    // GenerateWorld Section:
    private Vector2Int mapScale, _mapScale = new Vector2Int(1, 1);
    private Vector2 offset, _offset = new Vector2(0, 0);
    private float stretch, _stretch = 1f;
    private float clamp, _clamp = 0.5f;
    private bool runGenerateWorld = false;
    private Vector2Int startPos, _startPos = new Vector2Int(0, 0);
    private Vector2Int goalPos, _goalPos = new Vector2Int(0, 0);

    private Vector2Int minScale = new Vector2Int(1, 1);
    private float minStretch = 0.1f;
    private Vector2 clampLim = new Vector2(0f, 1f);

    // Algorithms Section:
    private bool runAStar = false;

    // INITIALIZE
    [MenuItem("Window/Controller")]
    private static void Init() {
        GetWindow(typeof(Controller)).Show();
    }

    // RENDER
    private void OnGUI() { Main(); }

    private void Main() {

        // SETUP
        EditorGUILayout.LabelField("Mainframe"); // label
        mainframe = (GameObject)EditorGUILayout.ObjectField(mainframe, typeof(GameObject), true); // gameobject dropbox

        if (mainframe != null) {

            // get scripts in mainframe
            Master master = mainframe.GetComponent<Master>();
            GenerateWorld generateWorld = mainframe.GetComponent<GenerateWorld>();
            ExecuteAlgorithm executeAlgorithm = mainframe.GetComponent<ExecuteAlgorithm>();

            // ESSENTIAL FIELDS
            iterationTime = EditorGUILayout.FloatField("Iteration Time", iterationTime); // time between each iteration
            iterationMax = EditorGUILayout.IntField("Iteration Max", iterationMax); // maximum number of iterations
            cancelIterations = GUILayout.Button("Cancel Procedures");
            if (cancelIterations) { executeAlgorithm.CancelIterations(); }
            if (iterationTime < minIterationTime) { iterationTime = minIterationTime; }


            // MAPGENERATOR
            // frontend
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(" - MAP GENERATOR");

            mapScale = EditorGUILayout.Vector2IntField("Scale", mapScale); // offset input text
            offset = EditorGUILayout.Vector2Field("Offset", offset); // offset input text
            stretch = EditorGUILayout.FloatField("Stretch", stretch); // stretch input text
            clamp = EditorGUILayout.FloatField("Clamp", clamp); // clamp input text
            startPos = EditorGUILayout.Vector2IntField("Start", startPos); // start position input text
            goalPos = EditorGUILayout.Vector2IntField("Goal", goalPos); // goal position input text
            runGenerateWorld = GUILayout.Button("Regenerate"); // generate tiles trigger

            if (mapScale.x < minScale.x) { mapScale.x = minScale.x; }
            if (mapScale.y < minScale.y) { mapScale.y = minScale.y; }
            if (stretch < minStretch) { stretch = minStretch; }
            if (clamp < clampLim.x) { clamp = clampLim.x; } else if (clamp > clampLim.y) { clamp = clampLim.y; }
            startPos.x = Mathf.Clamp(startPos.x, 0, mapScale.x - 1);
            startPos.y = Mathf.Clamp(startPos.y, 0, mapScale.y - 1);
            goalPos.x = Mathf.Clamp(goalPos.x, 0, mapScale.x - 1);
            goalPos.y = Mathf.Clamp(goalPos.y, 0, mapScale.y - 1);

            // backend
            master.Main();
            master.startTile = master.GetTileFromCoords(startPos);
            master.goalTile = master.GetTileFromCoords(goalPos);
            master.mapScale = mapScale;

            bool needsToUpdate = 
                (mapScale != _mapScale || offset != _offset || stretch != _stretch || clamp != _clamp ||
                startPos != _startPos || goalPos != _goalPos);

            if (runGenerateWorld || (master.update && needsToUpdate)) { // button click to cancel OR value update when allowed
                
                if (master.update && needsToUpdate) {
                    (_mapScale, _offset, _stretch, _clamp) =
                    ( mapScale,  offset,  stretch,  clamp);
                } else {
                    master.update = true;
                    executeAlgorithm.CancelIterations();
                }
                
                verified = generateWorld.Main(master, mapScale, offset, stretch, clamp);

            }

            bool ready = master.iteration == 0;
            bool playing = Application.isPlaying;

            EditorGUILayout.LabelField(verified ? "Verified" : "/ Unverified");
            EditorGUILayout.LabelField(playing ? "Play Mode" : "/ Editor Mode");
            EditorGUILayout.LabelField(ready ? "Ready" : (!master.finished ? "/ Active" : "/ Finished"));
                
            // ALGORITHMS
            if (verified && playing && ready) {
                
                EditorGUILayout.Space();
                EditorGUILayout.LabelField(" - ALGORITHMS");
                
                runAStar = GUILayout.Button("Run AStar"); // run AStar trigger

                // add or conditions to represent other algorithms
                if (runAStar) {
                    master.update = false;
                    generateWorld.Main(master, mapScale, offset, stretch, clamp);
                }

                if (runAStar) {
                    executeAlgorithm.Main("AStar", master, iterationTime, iterationMax);
                }
                
            }

        }

    }

}
