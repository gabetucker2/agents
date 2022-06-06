using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AStar : MonoBehaviour {

    // state 0 => untouched, state 1 => open, state 2 => closed with expand color, 3 => closed with closed color

    private Master master;
    private float sqrt2;
    private List<Vector2Int> searchMatrix = new List<Vector2Int>() {
        new Vector2Int(-1, -1),
        new Vector2Int(-1,  0),
        new Vector2Int(-1,  1),
        new Vector2Int( 0,  1),
        new Vector2Int( 0, -1),
        new Vector2Int( 1, -1),
        new Vector2Int( 1,  0),
        new Vector2Int( 1,  1),
    };
    private List<Tile> open;
    private List<Tile> closed;

    private float GetG(Tile tile) {
        return GetPathLen(tile);
    }
    
    private float GetH(Tile tile) { // calculate number of moves, indiscriminate from environment, to get to end
        Vector2Int magnitude = new Vector2Int(Mathf.Abs(tile.Coordinates.x - master.goalTile.Coordinates.x), Mathf.Abs(tile.Coordinates.y - master.goalTile.Coordinates.y));
        int diagonals = Mathf.Max(magnitude.x, magnitude.y);
        int parallels = magnitude.x < magnitude.y ? magnitude.y - diagonals : magnitude.x - diagonals;
        return (float)diagonals*sqrt2 + (float)parallels;
    }

    private void UpdateCosts(Tile tile) {
        Data_AStar data_AStar = tile.data_AStar;
        data_AStar.G = GetG(tile);
        data_AStar.H = GetH(tile);
    }

    private void AddToOpen(Tile tile) {
        tile.state++;
        open.Add(tile);
        if (tile.tileType == "Empty") {
            master.UpdateTileColor(tile, master.openCol);
        }
    }
    
    private void OpenToClosed(Tile tile) {
        tile.state++; // 2
        open.Remove(tile);
        closed.Add(tile);
        if (tile.tileType == "Empty") {
            master.UpdateTileColor(tile, master.expandCol);
        } else {
            tile.state++; // 3 if agent/goal
        }
    }

    private void FinalizePath(Tile tile) {
        
        Tile predecessor = master.GetPredecessorFromID(tile.data_AStar.predecessorID);

        if (predecessor != null) {
            if (tile.tileType != "Goal") {
                master.UpdateTileColor(tile, master.agentCol);
            }
            FinalizePath(predecessor);
        }

    }

    private float GetPathLen(Tile tile, Tile hypotheticalPredecessor = null, float len = 0) {
        
        // if hypothetical predecessor exists, act like that's the current tile's predecessor; otherwise, use real predecessor.
        Tile predecessor = hypotheticalPredecessor == null ? master.GetPredecessorFromID(tile.data_AStar.predecessorID) : hypotheticalPredecessor;
    
        if (predecessor == null) { // root has null predecessor, terminate
            return len;
        } else {

            // different x and y implies diagonality
            if (tile.Coordinates.x != predecessor.Coordinates.x && tile.Coordinates.y != predecessor.Coordinates.y) {
                len += sqrt2;
            } else {
                len++;
            }

            return GetPathLen(predecessor, null, len);

        }

    }

    public void Main(Master _master) {

        master = _master;
        
        open = new List<Tile>();
        closed = new List<Tile>();
        
        Tile startTile = master.tiles[master.From2DTo1D(master.startTile.Coordinates)];
        startTile.Coordinates = master.startTile.Coordinates;
        startTile.tileType = "Start";

        AddToOpen(startTile);
        UpdateCosts(startTile);

        sqrt2 = Mathf.Sqrt(2);

    }

    public bool Iterate() {

        bool ExploreNeighbors(Tile current) {

            OpenToClosed(current);

            if (current.tileType == "Goal") {

                // arrived at goal!
                FinalizePath(current);
                return false;

            }

            foreach (Vector2Int expandOffset in searchMatrix) {
                
                Tile neighbor = master.GetTileFromCoords(new Vector2Int(current.Coordinates.x + expandOffset.x, current.Coordinates.y + expandOffset.y));

                // if neighbor exists, is not closed, and is traversable
                if (neighbor != null && neighbor.state < 2 && neighbor.tileType != "Full") {

                    // if new path to neighbor is shorter than the neighbor's previous path or neighbor isn't added to open
                    if (GetPathLen(neighbor, current) < GetPathLen(neighbor) || neighbor.state != 1) {
                        
                        // update paths/costs of surrounding
                        neighbor.data_AStar.predecessorID = master.GetIDFromTile(current);
                        UpdateCosts(neighbor);
                        if (neighbor.state != 1) {
                            AddToOpen(neighbor);
                        }

                    }

                }

            }

            return true;

        }

        Tile GetMinFCost() {

            Tile minFTile = null;
            float minF = float.MaxValue;
            float minLen = System.Int32.MaxValue;

            bool expandedLastIteration = false;

            foreach (Tile tile in closed) {

                // if closed tile was just expanded to, then recolor it and tell us we expanded last iteration
                if (tile.state == 2) {
                    tile.state++;
                    master.UpdateTileColor(tile, master.closedCol);
                    expandedLastIteration = master.iteration > 0;
                    break;
                }

            }

            foreach (Tile tile in open) {
                
                float f = tile.data_AStar.f;
                float pathLen = GetPathLen(tile);
                if (f < minF || (f == minF && pathLen < minLen)) {
                    
                    minF = f;
                    minFTile = tile;

                    if (f == minF && pathLen < minLen) {
                        minLen = pathLen;
                    }

                }

            }

            if (!expandedLastIteration && master.iteration > 1) { // throw error if no new tile explored
                print("ERROR: Didn't expand last iteration");
            }
            
            return minFTile;

        }
        
        return ExploreNeighbors(GetMinFCost());

    }
    
}
