#pragma warning disable 8321 // disable RunIteration no-call warning

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExecuteAlgorithm : MonoBehaviour {

    private string scriptName;
    private Master master;
    private float iterationTime;
    private int iterationMax;
    
    private AStar aStar = null;
    
    private void RunIteration() {

        if (master.iteration == iterationMax) {

            CancelIterations();
            
        } else {

            switch (scriptName) {
                case "AStar":
                    if (master.iteration == 0) { aStar.Main(master); }
                    if(!aStar.Iterate()) {
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

    public void Main(string _scriptName, Master _master, float _iterationTime, int _iterationMax) {

        // update global variables so RunIteration() can reference them without needing a coroutine to pass arguments (since InvokeRepeating passes no arguments)
        ( scriptName,  master,  iterationTime,  iterationMax) =
        (_scriptName, _master, _iterationTime, _iterationMax);

        aStar = master.GetComponent<AStar>();
        
        if (iterationTime == 0) {
            while (!master.finished) {
                RunIteration();
            }
        } else {
            InvokeRepeating("RunIteration", 0f, iterationTime);
        }

    }

    public void CancelIterations() {
        master.finished = true;
        CancelInvoke();
    }

}
