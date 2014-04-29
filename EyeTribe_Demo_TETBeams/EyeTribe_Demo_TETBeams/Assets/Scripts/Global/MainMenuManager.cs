/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//manages the application states from the main menu
public class MainMenuManager : MonoBehaviour {
    //constants
    private const float PIXELS_TO_UNITS = 1.0f / 100.0f; //default pixels to units conversion for rendered textures

    //awake
    //called before start function
    void Awake() {
        //set the camera's size based on the current resolution and pixels to units ratio
        Camera.main.orthographicSize = Screen.height * PIXELS_TO_UNITS / 2; //half of the current window size
    }

	//init
	void Start () {
        //show mouse cursor
        Screen.showCursor = true;

        //transition into scene via state manager
        if (StateManager.Instance != null) {
            TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
            theTransition.toggleFade();
        }

        //audio
        if (AudioManager.Instance != null) {
            AudioManager.Instance.bgmSource.time = 0; //reset playback to beginning
            AudioManager.Instance.toggleFade();
        }
	
	} //end function

    //GUI
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
        int btnBuffer = btnH + btnH / 2;

        //play button
        float btnPlayY = btnY - btnH / 2 - btnBuffer;
        Rect btnPlayRect = new Rect(btnX, btnPlayY, btnW, btnH);
        string btnPlayText = "Play";

        //calibrate button
        float btnCalY = btnPlayY + btnBuffer;
        Rect btnCalRect = new Rect(btnX, btnCalY, btnW, btnH);
        string btnCalText = "Recalibrate";
        
        //quit button
        float btnQuitW = 100;
        float btnQuitH = 50;
        float btnQuitBuffer = 10;
        float btnQuitX = Screen.width - btnQuitW - btnQuitBuffer; //aligned to bottom-right of screen
        float btnQuitY = Screen.height - btnQuitH - btnQuitBuffer;
        Rect btnQuitRect = new Rect(btnQuitX, btnQuitY, btnQuitW, btnQuitH);
        string btnQuitText = "Quit";
        
        //play button pressed
        if (GUI.Button(btnPlayRect, btnPlayText)) {
            //proceed to game scene
            Debug.Log("Load Game");
            //transition to next scene
            TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
            theTransition.toggleFade();
            StateManager.Instance.switchSceneAfterDelay("game", theTransition.duration);
            //audio
            AudioManager.Instance.playBtnClick(); //sfx
            AudioManager.Instance.toggleFade(); //bgm
            AudioManager.Instance.switchBgmAfterDelay(AudioManager.Instance.bgmGame, AudioManager.Instance.bgmFadeDuration);
        }

        //calibrate button pressed
        if (GUI.Button(btnCalRect, btnCalText)) {
            //proceed to calibration scene
            Debug.Log("Load Calibration");
            //transition to next scene
            TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
            theTransition.toggleFade();
            StateManager.Instance.switchSceneAfterDelay("calibrate", theTransition.duration);
            //audio
            AudioManager.Instance.playBtnClick(); //sfx
            AudioManager.Instance.toggleFade(); //bgm
        }
        
        //quit button pressed
        if (GUI.Button(btnQuitRect, btnQuitText)) {
            //quit application
            Debug.Log("Quit Application");
            Application.Quit();
        }
        
    } //end function

} //end class
