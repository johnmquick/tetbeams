/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
using System.Reflection; //for MethodInfo
//TET SDK
using TETCSharpClient;

//manages TET setup process
//TODO: Create simplified system that connects and activates automatically, then proceeds to calibration, based on updated release of TET SDK
public class SetupManager : MonoBehaviour {
    //constants
    private const string TAG_GLOBAL = "global"; //tag for global game object
    private const string TAG_SETUP = "setup"; //tag for setup game object
    private const float PIXELS_TO_UNITS = 1.0f / 100.0f; //default pixels to units conversion for rendered textures

    //properties
    private GameObject globalObj; //global object
    private Global globalScript; //global script 
    private GameObject setupObj; //setup object
    private Connect connectScript; //connect script
    private Activate activateScript; //activate script
    private string nextLevel; //the name of the next level to load

    //awake
    //called before start function
    void Awake() {
        //set the camera's size based on the current resolution and pixels to units ratio
        Camera.main.orthographicSize = Screen.height * PIXELS_TO_UNITS / 2; //half of the current window size
    }

    //init
    void Start() { 
        //get game objects from scene
        globalObj = GameObject.FindWithTag(TAG_GLOBAL); //global game object
        globalScript = globalObj.GetComponent<Global>(); //global script
        setupObj = GameObject.FindWithTag(TAG_SETUP); //setup game object
        connectScript = setupObj.GetComponent<Connect>(); //connect script
        activateScript = setupObj.GetComponent<Activate>(); //activate script

        //disable scripts to start
        //note: disable/uncheck scripts in the Unity interface
        connectScript.enabled = false;
        activateScript.enabled = false;

        //state manager
        //transition into scene
        //initialize since this is the first scene
        if (StateManager.Instance != null) {
            //set the transition effects for the state manager
            StateManager.Instance.gameObject.GetComponent<TransitionFade>().isFadingIn = true;
            StateManager.Instance.gameObject.GetComponent<TransitionFade>().isHoldFade = false;
        }

        //audio manager
        //initialize since this is the first scene
        if (AudioManager.Instance != null) {
            //set the transition effects for the audio manager
            //bgm
            AudioManager.Instance.switchBgmAfterDelay(AudioManager.Instance.bgmMenu, 0.0f);
            AudioManager.Instance.bgmIsFadingIn = true;
            AudioManager.Instance.bgmIsHoldFade = false;
        }

    } //end function

    //TODO: TEMP GUI FOR TESTING
    void OnGUI() {
        //set gui skin
        if (GameObject.FindWithTag("global")) {
            GUI.skin = GameObject.FindWithTag("global").GetComponent<Global>().menuGUI;
        }

        //create buttons
        int btnW = 300;
        int btnH = 100;
        float btnX = Screen.width / 2 - btnW / 2;
        float btnY = Screen.height - btnH; //aligned to bottom of screen
        int btnBuffer = btnH + btnH / 4;

        //connect button
        float btnConnectY = btnY - btnH / 2 - btnBuffer * 2;
        Rect btnConnectRect = new Rect(btnX, btnConnectY, btnW, btnH);
        string btnConnectText;
        if (globalScript.tetConnected == true) {
            //set text
            btnConnectText = "Connected!";
            //disable button
            GUI.enabled = false;
            //draw button
            GUI.Button(btnConnectRect, btnConnectText);
        }
        else {
            //set text
            btnConnectText = "1. Connect";
            //enable button
            GUI.enabled = true;
            //draw button
            if (GUI.Button(btnConnectRect, btnConnectText)) {
                //connect
                UnityEngine.Debug.Log("1. Setup Connecting...");
                //enable script
                connectScript.enabled = true;
                //audio
                AudioManager.Instance.playBtnClick(); //sfx
            }
        }
        //activate button
        float btnActivateY = btnY - btnH / 2 - btnBuffer;
        Rect btnActivateRect = new Rect(btnX, btnActivateY, btnW, btnH);
        string btnActivateText;
        if (globalScript.tetActive == true) {
            //set text
            btnActivateText = "Activated!";
            //disable button
            GUI.enabled = false;
            //draw button
            GUI.Button(btnActivateRect, btnActivateText);
        }
        else {
            //set text
            btnActivateText = "2. Activate";
            //enable button only if tet server already connected
            if (globalScript.tetConnected == true) {
                GUI.enabled = true;
            }
            else {
                GUI.enabled = false;
            }
            //draw button
            if (GUI.Button(btnActivateRect, btnActivateText)) {
                //activate
                UnityEngine.Debug.Log("2. Setup Activating...");
                //enable script
                activateScript.enabled = true;
                //audio
                AudioManager.Instance.playBtnClick(); //sfx
            }
        }

        //calibrate button
        float btnCalY = btnY - btnH / 2;
        Rect btnCalRect = new Rect(btnX, btnCalY, btnW, btnH);
        string btnCalText = "3. Calibrate";
        //TODO
        //if (GazeManager.Instance.IsActivated == true) {
        if (globalScript.tetConnected == true && globalScript.tetActive == true) {
            //enable button
            GUI.enabled = true;
            //draw button
            if (GUI.Button(btnCalRect, btnCalText)) {
                //activate
                UnityEngine.Debug.Log("3. Setup Calibrating...");
                //transition to next scene
                TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
                theTransition.toggleFade();
                StateManager.Instance.switchSceneAfterDelay("calibrate", theTransition.duration);
                //audio
                AudioManager.Instance.playBtnClick(); //sfx
                AudioManager.Instance.toggleFade(); //bgm
            }
        }
        else {
            //disable button
            GUI.enabled = false;
            //draw button
            GUI.Button(btnCalRect, btnCalText);
        }
        
        //reenable GUI by default
        GUI.enabled = true;

    } //end function

} //end class