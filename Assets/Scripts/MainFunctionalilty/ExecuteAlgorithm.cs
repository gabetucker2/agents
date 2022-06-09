#pragma warning disable 8321 // disable RunIteration no-call warning

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteAlgorithm : MonoBehaviour {

    private string scriptName;
    private Master master;
    private float iterationTime;
    private int iterationMax;
    private float aStar_weight;
    private bool aStar_dijkstras;
    private float timeStart = 0;
    
    private AStar aStar = null;
    
    private void RunIteration() {

        if (master.iteration == iterationMax) {

            CancelIterations();
            
        } else {

            switch (scriptName) {
                case "AStar":
                    if (master.iteration == 0) { aStar.Main(master, aStar_weight, aStar_dijkstras); }
                    float aStarIterate = aStar.Iterate();
                    if(aStarIterate != -1f) {
                        // detected agent arrived at goal, stop the loop
                        master.lastRunRT = Time.realtimeSinceStartup - timeStart;
                        master.lastRunPathLen = aStarIterate;
                        CancelIterations();
                    }
                    break;
                default:
                    print("ERROR: bad scriptName argument to ExecuteAlgorithm.Main() procedure");
                    CancelIterations();
                    break;

            }
            
            master.iteration++;

        }

    }

    public void Main(string _scriptName, Master _master, float _iterationTime, int _iterationMax, float _aStar_weight, bool _aStar_dijkstras) {

        // update global variables so RunIteration() can reference them without needing a coroutine to pass arguments (since InvokeRepeating passes no arguments)
        ( scriptName,  master,  iterationTime,  iterationMax,  aStar_weight,  aStar_dijkstras) =
        (_scriptName, _master, _iterationTime, _iterationMax, _aStar_weight, _aStar_dijkstras);

        aStar = master.GetComponent<AStar>();

        timeStart = Time.realtimeSinceStartup;
        
        if (iterationTime == 0) {
            // use while loop instead if iterationTime == 0 since InvokeRepeating can't handle when t == 0
            while (!master.finished) {
                RunIteration();
            }
        } else {
            InvokeRepeating("RunIteration", 0f, iterationTime);
        }

    }

    public void CancelIterations() {
        if (master != null) {
            master.finished = true;
            CancelInvoke();
        }
    }

}
