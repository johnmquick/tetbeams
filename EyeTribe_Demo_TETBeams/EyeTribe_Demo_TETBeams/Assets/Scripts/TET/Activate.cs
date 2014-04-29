/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
//TET SDK
using TETCSharpClient;

//manages tet server activation
public class Activate : MonoBehaviour {
    //constants
    private const string TAG_GLOBAL = "global"; //tag for global game object

    //properties
    private GameObject globalObj; //global object
    private Global globalScript; //global script 

    //init
    void Start () {
        //get game objects from scene
        globalObj = GameObject.FindWithTag(TAG_GLOBAL); //global game object
        globalScript = globalObj.GetComponent<Global>(); //global script
	
	}

    //update
    void Update() {
        //check for prepared tracker state and for need to activate tet server
        if (GazeManager.Instance.Trackerstate == GazeManager.TrackerState.TRACKER_CONNECTED && globalScript.tetActive == false) {
            //activate the tet server
            activateTETServer();
        }
        else {
            //UnityEngine.Debug.Log("TET Server Already Active Or Not In Connected State");
        }
    }

    //activate tet server
    public void activateTETServer() {
        UnityEngine.Debug.Log("Activating TET Server");
        //Activate(API version, client mode)
        //client mode can be push (continuous) or pull (on request)
        if (GazeManager.Instance.Activate(GazeManager.ApiVersion.VERSION_1_0, GazeManager.ClientMode.Push)) {
            //toggle flag
            globalScript.tetActive = true;
            //disable script once tet server is active
            this.enabled = false;
        }
    } 
}
