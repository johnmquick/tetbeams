/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
using System; //for Math

//manages summary of data presented to player
public class SummaryManager : MonoBehaviour {
    //constants
    private const float PIXELS_TO_UNITS = 1.0f / 100.0f; //default pixels to units conversion for rendered textures

    //properties
    private string _summaryText; //the text to be presented to the player

    //awake
    //called before start function
    void Awake() {
        //set the camera's size based on the current resolution and pixels to units ratio
        Camera.main.orthographicSize = Screen.height * PIXELS_TO_UNITS / 2; //half of the current window size
    }

    //init
    void Start() {
        //summarize the data
        summarizeData();

        //transition into scene via state manager
        if (StateManager.Instance != null) {
            TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
            theTransition.toggleFade();
        }

        //audio
        if (AudioManager.Instance != null) {
            AudioManager.Instance.toggleFade();
        }

    } //end function

    //summarize the data from the score manager instance so it can be reported to the player
    private void summarizeData() {
        //verify that the score manager instance exists
        if (ScoreManager.Instance != null) {

            //get the data from the score manager instance
            int totalObjects = ScoreManager.Instance.totalObjects;
            int hitObjects = ScoreManager.Instance.hitObjects;
            double pctHit = Math.Round(ScoreManager.Instance.pctHit, 2);
            double chargeUsed = Math.Round(ScoreManager.Instance.chargeUsed / 100, 2);
            TimeSpan duration = TimeSpan.FromSeconds(ScoreManager.Instance.duration);

            //format strings
            string pctHitStr = String.Format("{0:0.00%}", pctHit);
            string durationStr = String.Format("{0:D1}:{1:D2}.{2:D3}", duration.Minutes, duration.Seconds, duration.Milliseconds);

            //prepare the final summary text
            _summaryText =
                "Round Statistics \n"
                + "Total Targets: " + totalObjects + " \n"
                + "Destroyed Targets: " + hitObjects + " \n"
                + "Percentage: " + pctHitStr + " \n"
                + "Full Charges Used: " + chargeUsed + " \n"
                + "Total Time: " + durationStr;
        } //end outer if
        //error
        else {
            Debug.Log("[SummaryManager] Score Manager Instance not found - cannot summarize data");
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
        string btnPlayText = "Play Again";

        //menu button
        float btnMenuY = btnPlayY + btnBuffer;
        Rect btnMenuRect = new Rect(btnX, btnMenuY, btnW, btnH);
        string btnMenuText = "Return to Main Menu";

        //create text
        float summaryW = btnW;
        float summaryH = btnH * 2;
        float summaryX = Screen.width / 2 - summaryW / 2;
        float summaryY = btnPlayY - summaryH;
        Rect summaryRect = new Rect(summaryX, summaryY, summaryW, summaryH);
        GUI.Label(summaryRect, _summaryText);

        //play button pressed
        if (GUI.Button(btnPlayRect, btnPlayText)) {
            //return to game
            Debug.Log("Load Game");
            //transition to next scene
            TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
            theTransition.toggleFade();
            StateManager.Instance.switchSceneAfterDelay("game", theTransition.duration);
            //audio
            AudioManager.Instance.playBtnClick(); //sfx
            AudioManager.Instance.toggleFade();
            AudioManager.Instance.switchBgmAfterDelay(AudioManager.Instance.bgmGame, AudioManager.Instance.bgmFadeDuration);
        }

        //calibrate button pressed
        if (GUI.Button(btnMenuRect, btnMenuText)) {
            //return to main menu
            Debug.Log("Load Main Menu");
            //transition to next scene
            TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
            theTransition.toggleFade();
            StateManager.Instance.switchSceneAfterDelay("mainMenu", theTransition.duration);
            //audio
            AudioManager.Instance.playBtnClick(); //sfx
            AudioManager.Instance.toggleFade();
            AudioManager.Instance.switchBgmAfterDelay(AudioManager.Instance.bgmMenu, AudioManager.Instance.bgmFadeDuration);
        }

    } //end function

} //end class
