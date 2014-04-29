/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//stores global variables for use across application
//uses singleton instance
public class Global : MonoBehaviour {
    //global variables
    public bool tetConnected; //whether the tet server has been connected
    public bool tetActive; //whether the tet server has been activated
    public GUISkin menuGUI; //the gui skin for game menus

    //awake
    void Awake() {
        //prevent this script from being destroyed when application switches scenes
        DontDestroyOnLoad(this);
    }

	//init
	void Start () {
        tetConnected = false; //on startup, tet server assumed disconnected
        tetActive = false; //on startup, tet server should not be active
	
	}
	
}
