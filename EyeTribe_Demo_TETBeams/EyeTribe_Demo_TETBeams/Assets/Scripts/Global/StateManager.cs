/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
//TET SDK
using TETCSharpClient;

//manages switching between scenes
//uses singleton instance to manage all scenes
public class StateManager : MonoBehaviour {
    //singleton properties
    private static StateManager _Instance; //the singleton instance of the class

    //properties
    private string _nextScene; //the next scene to switch to; set by the current scene

    //create instance via getter
    //access StateManager.Instance from other classes
    public static StateManager Instance {
        get {
            //check for existing instance
            //if no instance
            if (_Instance == null) {
                //create game object
                GameObject StateManagerObj = new GameObject();
                StateManagerObj.name = "StateManager";

                //create instance
                _Instance = StateManagerObj.AddComponent<StateManager>();

                //add scripts
                StateManagerObj.AddComponent<TransitionFade>(); //add new transition script
            }

            //return the instance
            return _Instance;
        } //end get
    } //end accessor

    //awake
    void Awake() {
        //prevent this script from being destroyed when application switches scenes
        DontDestroyOnLoad(this);

    } //end function

    //switch scene after delay
    public void switchSceneAfterDelay(string theScene, float theDelay) {
        //set the next scene
        _nextScene = theScene;

        //invoke the switch after the given delay
        //used to allow transition to occur before switch
        Invoke("switchScene", theDelay);
    }

    //switch scene
    private void switchScene() {
        //audio
        //stop any outstanding sound effects
        AudioManager.Instance.stopAllSfx();

        //load next scene
        Application.LoadLevel(_nextScene);
    }

    //when application is exited
    void OnApplicationQuit() {
        //remove gaze listeners
        GazeManager.Instance.ClearListeners();
        //clear calibration
        GazeManager.Instance.CalibrationClear();
        //deactivate tet server
        GazeManager.Instance.Deactivate();
    } //end function
    
} //end class
