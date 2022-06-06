using UnityEngine;

public class GenerateWorld : MonoBehaviour {

    public bool Main(Master master, Vector2Int mapScale, Vector2 offset, float stretch, float clamp) {

        // INITIALIZE VARIABLES
        master.finished = false;
        bool verified = true;
        verified = !(master.startTile.Coordinates == master.goalTile.Coordinates);
        GameObject map = master.map;
        Material mat = map.GetComponent<MeshRenderer>().sharedMaterial;
        ExecuteAlgorithm executeAlgorithm = master.GetComponent<ExecuteAlgorithm>();
        
        // REFRESH STUFF TO BASE/CANCEL ROUTINES
        if (master.iteration > 0) {
            executeAlgorithm.CancelIterations();
            master.iteration = 0;
        }

        // GENERATE AND APPLY TEXTURE
        Texture2D texture = new Texture2D(mapScale.x, mapScale.y, TextureFormat.ARGB32, false);
        texture.filterMode = FilterMode.Point;
        mat.mainTexture = texture;

        master.tiles.Clear();

        // 0 -> mapScale.x/mapScale.y
        for (int x = 0; x < mapScale.x; x++) {
            for (int y = 0; y < mapScale.y; y++) {

                // 0 -> 1
                float i = ((float)x / (float)mapScale.x) * (float)mapScale.x / (float)mapScale.y;
                float j = (float)y / (float)mapScale.y;
                
                bool empty = ((Mathf.PerlinNoise((offset.x + i) * stretch, (offset.y + j) * stretch)) < clamp);
                
                string tileType;
                Color col;
                if (master.startTile.Coordinates.x == x && master.startTile.Coordinates.y == y) {
                    tileType = "Start";
                    col = master.agentCol;
                    if (!empty) {
                        verified = false;
                    }
                } else if (master.goalTile.Coordinates.x == x && master.goalTile.Coordinates.y == y) {
                    tileType = "Goal";
                    col = master.goalCol;
                    if (!empty) {
                        verified = false;
                    }
                } else if (empty) {
                    tileType = "Empty";
                    col = master.emptyCol;
                } else {
                    tileType = "Full";
                    col = master.fullCol;
                }

                texture.SetPixel(mapScale.x - 1 - x, mapScale.y - 1 - y, col);

                master.tiles.Add(new Tile(new Vector2Int(x, y), tileType));

            }
        }

        texture.Apply();

        // UPDATE MAP mapScale
        map.transform.localScale = new Vector3((float)mapScale.x, 1, (float)mapScale.y);

        return verified;

    }
    
}
