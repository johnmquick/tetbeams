/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
using System; //for Math

//utility script for managing fade in/out animations
//fades an object in/out over time and destroys if needed
//object should have a sprite renderer
public class Fade : MonoBehaviour {
    //constants
    private const float ALPHA_MIN = 0.0f; //minimum alpha value
    private const float ALPHA_MAX = 1.0f; //maximum alpha value
    private const float DURATION_DEFAULT = 1.0f; //default duration of 1 second

    //objects
    private SpriteRenderer theRenderer; //the game object's renderer

    //properties
    public bool isFadeIn; //whether the object should fade in
    public bool isFadeOut; //whether the object should fade out
    public float duration; //how long, in seconds, the object should take to complete each fade
    public bool isFadeInFirst; //whether the object should start by fading in
    public bool isHoldFade; //whether to hold the fading until flag is told to begin
    public bool isDestroyedOnComplete; //whether to destroy the object once fading is complete

    private float alpha; //the game object renderer's alpha color value
    private bool isFadingIn; //whether the object is currently fading in
    private float startTime; //when the object started fading

    //init
    public void Start() {
        //ensure object has a sprite renderer
        if (gameObject.GetComponent<SpriteRenderer>() != null) {
            //objects
            theRenderer = gameObject.GetComponent<SpriteRenderer>(); //the game object's renderer
            alpha = gameObject.GetComponent<SpriteRenderer>().color.a; //the game object renderer's color value
        }
        //error
        else {
            Debug.Log("[Fade] Error: Missing sprite renderer - object cannot be faded");
            //disable script
            this.enabled = false;
        }
        
        //properties
        //ensure a valid duration value is used
        if (duration <= 0.0f) {
            duration = DURATION_DEFAULT;
            Debug.Log("[Fade] Invalid duration provided - set to default of 1 second instead");
        }

        //check whether to start by fading in or out
        isFadingIn = false; //fade out first
        if (isFadeInFirst == true && isFadeIn == true) {
            isFadingIn = true; //fade in first
        }

        //set the start time
        startTime = Time.time;

    } //end function

    //update
    public void Update() {
        //only update alpha if not on hold
        if (isHoldFade == false) {

            //fade in
            if (isFadeIn == true &&
                isFadingIn == true) {
                fadeIn();
            }

            //fade out
            if (isFadeOut == true &&
                isFadingIn == false) {
                fadeOut();
            }

            //update the renderer's alpha value
            Color theColor = theRenderer.color;
            theColor.a = alpha;
            theRenderer.color = theColor;

            //check whether to switch between fading in/out
            checkFadeToggle();
        
        } //end outer if
        //update the start time
        else {
            startTime = Time.time;
        }
        
        //Debug.Log("[Fade] Current alpha level: " + theAlpha);

    } //end function

    //fade in
    public void fadeIn() {
        //calculate the time completed thus far
        float cumulativeTime = Time.time - startTime; //cumulative time completed
        float pctTime = Mathf.Clamp(cumulativeTime / duration, 0.0f, 1.0f); //percentage time completed
        //Debug.Log("[Fade] cumulativeTime: " + cumulativeTime);
        //Debug.Log("[Fade] pctTime: " + pctTime);

        //alpha is less than max
        if (alpha < ALPHA_MAX) {
            //increase alpha
            alpha = pctTime;
        }
        //alpha has reached max
        else {
            //set to max alpha
            alpha = ALPHA_MAX;
        }
    
    } //end function

    //fade out
    public void fadeOut() {
        //calculate the time completed thus far
        float cumulativeTime = Time.time - startTime; //cumulative time completed
        float pctTime = 1.0f - Mathf.Clamp(cumulativeTime / duration, 0.0f, 1.0f); //percentage time completed
        //Debug.Log("[Fade] cumulativeTime: " + cumulativeTime);
        //Debug.Log("[Fade] pctTime: " + pctTime);

        //alpha is greater than min
        if (alpha > ALPHA_MIN) {
            //decrease alpha
            alpha = pctTime;
        }
        //alpha has reached min
        else {
            //set to min alpha
            alpha = ALPHA_MIN;
        }

    } //end function

    //check whether to switch between fading in/out
    public void checkFadeToggle() {
        //currently at max alpha and set to fade out
        if (alpha == ALPHA_MAX && 
            isFadeOut == true) {
            Debug.Log("[Fade] Fade in complete - begin fade out");
            //reset start time
            startTime = Time.time;
            //toggle flag to continue fading
            isFadingIn = false;
        }
        //currently at min alpha and set to fade in
        else if (alpha == ALPHA_MIN &&
            isFadeIn == true) {
            Debug.Log("[Fade] Fade out complete - begin fade in");
            //reset start time
            startTime = Time.time;
            //toggle flag to continue fading
            isFadingIn = true;
        }
        //all fading is complete
        else if ((alpha == ALPHA_MAX && isFadeOut == false) || 
                (alpha == ALPHA_MIN && isFadeIn == false)) {
            //check whether to destroy object 
            if (isDestroyedOnComplete == true) {
                Debug.Log("[Fade] All fading complete - destroy object");
                //destroy parent game object
                Destroy(gameObject);
            }
            //otherwise, disable script
            else {
                Debug.Log("[Fade] All fading complete - script disabled");
                //disable script
                this.enabled = false;
            }
        }
            
    } //end function

} //end class
