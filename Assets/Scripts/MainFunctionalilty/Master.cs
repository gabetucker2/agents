using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class Data_AStar {

    // FUNCTIONS
    private float Operation(float _g, float _h, float _w, bool _dijkstras) {
        float dij = _dijkstras ? 0 : 1;
        return g + w*dij*h;
    }

    // DON'T SET
    public float f;
    public float g;
    public float h;
    public float w;
    public bool dijkstras;

    public void UpdateF() { f = Operation(g, h, w, dijkstras); }
    
    // SET
    public int predecessorID; // int rather than Tile to avoid infinite recursion

    // CONSTRUCTOR
    public Data_AStar(float _g = -0.5f, float _h = -0.5f, float _w = 1f, bool _dijkstras = false, int _predecessorID = -1) {

        ( g,  h,  w,  dijkstras,  predecessorID) =
        (_g, _h, _w, _dijkstras, _predecessorID);

    }

}

[System.Serializable]
public class Tile {

    // DON'T SET
    public string scoordinates;
    private Vector2Int coordinates;
    public int state;


    // SET
    [HideInInspector] public Vector2Int Coordinates {
        get { return coordinates; }
        set {
            coordinates = value;
            scoordinates = coordinates.x + ", " + coordinates.y;
        }
    }
    public string tileType;
    public Data_AStar data_AStar;

    // CONSTRUCTOR
    public Tile(Vector2Int _Coordinates, string _tileType, int _state = 0, Data_AStar _data_AStar = null) {

        ( Coordinates,  tileType,  state,  data_AStar) =
        (_Coordinates, _tileType, _state, _data_AStar);

        // manually set immutable default parameter by replacing temporary null
        if (data_AStar == null) {
            data_AStar = new Data_AStar();
        }

    }

}

public class Master : MonoBehaviour {

    // VARIABLES
    [Header(" - SET")]
    public GameObject map;
    private Material mat;
    public Color emptyCol, fullCol, agentCol, goalCol, expandCol, openCol, closedCol;
    
    [Space(10)]
    [Header(" - DON'T SET")]
    public float lastRunRT = 0f;
    public float lastRunPathLen = 0f;
    public int iteration = 0;
    public bool update = true;
    public bool finished = false;
    public Tile startTile, goalTile;
    public Vector2Int mapScale;
    //public bool showTilesList = false;
    [HideInInspector] public List<Tile> tiles;

    public void Main() {
        mat = map.GetComponent<MeshRenderer>().sharedMaterial;
    } 

    // METHODS
    public int From2DTo1D(Vector2Int coords) { // iterates through y first then x second in terms of List order
        return IsWithinMapBounds(coords) ? (mapScale.y * coords.x) + coords.y : -1;
    }

    public Tile GetTileFromCoords(Vector2Int coords) {
        int i = From2DTo1D(coords);
        return i == -1 ? null : tiles[i];
    }

    public void UpdateTileColor(Tile tile, Color col) {
        Texture2D texture = (Texture2D)mat.mainTexture;
        texture.SetPixel(mapScale.x - 1 - tile.Coordinates.x, mapScale.y - 1 - tile.Coordinates.y, col);
        texture.Apply();
    }

    public Tile GetPredecessorFromID(int id) {
        return id == -1 ? null : tiles[id];
    }
    
    public bool IsWithinMapBounds(Vector2Int vec) {
        return !(vec.x < 0 || vec.x == mapScale.x || vec.y < 0 || vec.y == mapScale.y);
    }

    public int GetIDFromTile(Tile tile) {
        return tiles.IndexOf(tile);
    }

}

/*
[CustomEditor(typeof(Master))]
public class MasterEditor : Editor
{
    public override void OnInspectorGUI()
    {

        Master master = (Master)target;
        master.showTilesList = GUILayout.Toggle(master.showTilesList, "Show Tiles List");

        if(master.showTilesList) {
                                                                            // IMPLEMENT
            ScriptableObject target = this;
            SerializedObject so = new SerializedObject(target);
            SerializedProperty stringsProperty = so.FindProperty("tiles");
    
            EditorGUILayout.PropertyField(stringsProperty, true);
            so.ApplyModifiedProperties();

        }
        
    }

}
*/
