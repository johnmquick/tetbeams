/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//manages transitions when switching between scenes
//this implementation uses a fade in/out effect via OnGUI
//note: fading in/out the masking texture is the opposite of fading out/in the scene; functions are written in terms of the scene (fade in = black texture to transparent; fade out = transparent texture to black)
public class TransitionFade : MonoBehaviour {
    //constants
    private const float ALPHA_MIN = 0.0f; //minimum alpha value
    private const float ALPHA_MAX = 1.0f; //maximum alpha value
    private const float DURATION_DEFAULT = 1.0f; //default duration of 1 second

    //properties
    private float _alpha; //current alpha level
    private float _startTime; //the time the current fade in/out segment started
    public float duration; //the duration, in seconds, that the fade in/out segment should last
    public bool isFadingIn; //whether currently fading in or out
    public bool isHoldFade; //whether to hold the fading until flag is told to begin

    //objects
    private Texture2D _theTexture; //texture used to cover screen when making fade in/out effect, placed in scene
    
	//init
	void Start () {
        //properties
        //ensure a valid duration value is used
        if (duration <= 0.0f) {
            duration = DURATION_DEFAULT;
            //Debug.Log("[TransitionManager] Invalid duration provided - set to default of 1 second instead");
        }
        _alpha = ALPHA_MAX; //assume max alpha
        _startTime = Time.time; //establish start time

        //objects
        _theTexture = new Texture2D(1, 1); //create 1x1 pixel
        _theTexture.SetPixel(0, 0, Color.black); //set color to black
        _theTexture.Apply(); //apply changes

	} //end function
	
    //update
    void Update() {
        //only update alpha if not on hold
        if (isHoldFade == false) {
            //fade in
            if (isFadingIn == true) {
                fadeIn();
            }

            //fade out
            if (isFadingIn == false) {
                fadeOut();
            }
        } //end outer if
    
    } //end function

    //fade out the scene by fading in the texture
    private void fadeOut() {
        //calculate the time completed thus far
        float cumulativeTime = Time.time - _startTime; //cumulative time completed
        float pctTime = Mathf.Clamp(cumulativeTime / duration, 0.0f, 1.0f); //percentage time completed

        //alpha is less than max
        if (_alpha < ALPHA_MAX) {
            //increase alpha
            _alpha = pctTime;
        }
        //alpha has reached max
        else {
            //set to max alpha
            _alpha = ALPHA_MAX;
            //hold
            isHoldFade = true;
        }

    } //end function

    //fade in the scene by fading out the texture
    private void fadeIn() {
        //calculate the time completed thus far
        float cumulativeTime = Time.time - _startTime; //cumulative time completed
        float pctTime = 1.0f - Mathf.Clamp(cumulativeTime / duration, 0.0f, 1.0f); //percentage time completed

        //alpha is greater than min
        if (_alpha > ALPHA_MIN) {
            //decrease alpha
            _alpha = pctTime;
        }
        //alpha has reached min
        else {
            //set to min alpha
            _alpha = ALPHA_MIN;
            //hold
            isHoldFade = true;
        }

    } //end function

    //toggle fade
    //call before triggering transition
    public void toggleFade() {
        //switch fade effect to prepare for next segment
        isFadingIn = !isFadingIn;
        //turn off hold
        isHoldFade = false;
        //reset time
        _startTime = Time.time;

    } //end function

    //use GUI to create fade effect via texture and modified alpha value
    void OnGUI() {
        //draw the texture
        Rect textureRect = new Rect(0, 0, Screen.width, Screen.height); //cover entire screen
        Color theColor = GUI.color; //adjust the color according to the current alpha
        theColor.a = _alpha;
        GUI.color = theColor;
        GUI.depth = -100; //ensure texture is in front of everything else
        GUI.DrawTexture(textureRect, _theTexture); //draw the texture
    } //end function
    
} //end class
