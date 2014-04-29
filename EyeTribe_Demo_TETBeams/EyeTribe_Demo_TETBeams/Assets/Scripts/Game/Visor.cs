/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//visor controlled by player
//uses charge and fires on user input
//recharges once out of charge or when not receiving user input
//checks for boundary and object collisions
public class Visor : MonoBehaviour {
	//constants
	private const string TAG_VISOR = "visor"; //tag for visor game object
	private const string TAG_VISOR_INSIDEVIEW = "visorInsideView"; //tag for visor inside view game object
    private const string TAG_VISOR_RECHARGEVIEW = "visorRechargeView"; //tag for visor recharge view game object
	private const string TAG_TARGET = "target"; //tag for target game objects
    private const int KEY_MOUSE_L = 0; //value for left click input from mouse
    private const int VISOR_W_UNITS = 6; //width of the visor in world units
    private const int VISOR_H_UNITS = 1; //width of the visor in world units
    private const float VISOR_OFFSET_UNITS_INIT = 2.84f; //initial offset for visor recharge view in world units (to align left charging, rather than center)
    private const float VISOR_OFFSET_UNITS_UPDATE = 0.32f; //update offset for visor recharge view in world units (to align left charging, rather than center)

	//properties
	private bool collisionsEnabled; //whether collisions should be checked or ignored
    private bool sfxVisorFullEnabled; //whether the audio sfx is enabled

	//objects
	private GameObject theVisorInsideView; //visor inside view game object in scene
    private GameObject theVisorRechargeView; //visor recharge view game object in scene

	//scripts
	public Recharge theRecharge; //recharge script for managing charge level
    public GazeMove theGazeMove; //gazemove script for updating position

    //sprites
    //sprites values are assigned in unity editor
    public Sprite visorOpen; //open visor sprite
    public Sprite visorClosed; //closed visor sprite

	//init
	void Start () {
		//objects
		theVisorInsideView = GameObject.FindWithTag(TAG_VISOR_INSIDEVIEW); //visor inside view game object in scene
        theVisorRechargeView = GameObject.FindWithTag(TAG_VISOR_RECHARGEVIEW); //visor recharge view game object in scene
        theVisorRechargeView.transform.position = new Vector3(theVisorRechargeView.transform.position.x - VISOR_OFFSET_UNITS_INIT, theVisorRechargeView.transform.position.y, theVisorRechargeView.transform.position.z); //offset initial position for recharge view (to align left charging, rather than center)

        //properties
        //audio
        sfxVisorFullEnabled = true;

		//scripts
        theRecharge = GetComponent<Recharge>(); //get recharge script attached to object
        theGazeMove = GetComponent<GazeMove>(); //get gazemove script attached to object

	} //end function
	
	//update
	void Update () {
        //check input to determine whether visor is charging or decharging
        //on L mouse down
        if (Input.GetMouseButtonDown(KEY_MOUSE_L) == true) {
            //enable collisions
			collisionsEnabled = true;
            //set flag to decharge
            theRecharge.isCharging = false;
            //update visuals
			theVisorInsideView.GetComponent<SpriteRenderer>().sprite = visorOpen;
            theVisorRechargeView.renderer.enabled = false;
            //audio
            AudioManager.Instance.playVisorDecharge();
        }
        //on L mouse release or upon complete discharge, set to recharge
        if (Input.GetMouseButtonUp(KEY_MOUSE_L) == true 
            || theRecharge.charge <= theRecharge.chargeMin) {
			//disable collisions
			collisionsEnabled = false;
            //set flag to recharge
            theRecharge.isCharging = true;
            //update visuals
			theVisorInsideView.GetComponent<SpriteRenderer>().sprite = visorClosed;
            //audio
            AudioManager.Instance.playVisorRecharge();
        }

        //audio
        checkAudio();

        //update recharge view
        updateRechargeView();

	} //end function

    //check audio
    public void checkAudio() {
        //get the current charge ratio
        //current level / max charge
        float chargeRatio = theRecharge.charge / theRecharge.chargeMax;

        //visor full
        //on complete recharge, play sound effect once
        if (chargeRatio >= 1.0f && sfxVisorFullEnabled == true) {
            //toggle flag
            sfxVisorFullEnabled = false;
            //audio
            AudioManager.Instance.playVisorFull();
        }
        //unblock sound after substantial decharge has taken place
        //prevents repeated playing of sound effect
        else if (chargeRatio < 0.9f) {
            //toggle flag
            sfxVisorFullEnabled = true;
        }

        //visor empty
        //note: not necessary, since recharge with sound begins immediately
        /*
        //on complete discharge, play sound effect once
        if (chargeRatio <= 0.0f && sfxVisorEmptyEnabled == true) {
            //toggle flag
            sfxVisorEmptyEnabled = false;
            //audio
            AudioManager.Instance.playVisorEmpty();
        }
        //unblock sound after substantial recharge has taken place
        //prevents repeated playing of sound effect
        else if (chargeRatio > 0.1f) {
            //toggle flag
            sfxVisorEmptyEnabled = true;
        }
        */

    } //end function

    //update recharge view
    public void updateRechargeView() {
        //enable renderer
        theVisorRechargeView.renderer.enabled = true;

        //get the current charge ratio
        //current level / max charge
        float chargeRatio = theRecharge.charge / theRecharge.chargeMax;

        //get the change in charge level
        //(current charge - prev charge) / max charge
        float chargeChange = (theRecharge.charge - theRecharge.prevCharge) / theRecharge.chargeMax;

        //update previous charge
        theRecharge.updatePrevRecharge();

        //update the scale of the view
        //update the width only
        theVisorRechargeView.transform.localScale = new Vector3(chargeRatio, theVisorRechargeView.transform.localScale.y, theVisorRechargeView.transform.localScale.z);

        //move the object relative to its updated scale so it appears to grow from L to R
        float xAdjust = theVisorRechargeView.transform.position.x + (chargeChange * ((VISOR_W_UNITS - VISOR_OFFSET_UNITS_UPDATE) / 2));
        theVisorRechargeView.transform.position = new Vector3(xAdjust, theVisorRechargeView.transform.position.y, theVisorRechargeView.transform.position.z);

    } //end function

    //check collisions
    /*
    //initial collision
    void OnTriggerEnter2D(Collider2D theCollider) {
		//only check collisions if enabled
		if (collisionsEnabled == true) {
			//check collider tag
			switch (theCollider.gameObject.tag) {
				//target
				case TAG_TARGET:
				//Debug.Log("[Visor] Target Initial Collision Detected: " + theCollider.gameObject.GetComponent<Recharge>().charge);
				//note: handle charging flag in stay function
				break;
				default:
				Debug.Log("[Visor] Error: no object found for collider tag");
				break;
			} //end switch
		} //end if
    } //end function
    */
    //collision over a series of frames
    void OnTriggerStay2D(Collider2D theCollider) {
		//only check collisions if enabled
		if (collisionsEnabled == true) {
			//check collider tag
			switch (theCollider.gameObject.tag) {
				//target
				case TAG_TARGET:
				//Debug.Log("[Visor] Target Charge Increased: " + theCollider.gameObject.GetComponent<Recharge>().charge);
				//toggle charging flag
				theCollider.gameObject.GetComponent<Recharge>().isCharging = true;
                //toggle shaking flag
                theCollider.gameObject.GetComponent<Shake>().isShaking = true;
				break;
				default:
				Debug.Log("[Visor] Error: no object found for collider tag");
				break;
			} //end switch
		} //end if
		//ignore or stop collision effects
		else {
			//check collider tag
			switch (theCollider.gameObject.tag) {
				//target
				case TAG_TARGET:
				//Debug.Log("[Visor] Target Charge Stopped: " + theCollider.gameObject.GetComponent<Recharge>().charge);
				//toggle charging flag
				theCollider.gameObject.GetComponent<Recharge>().isCharging = false;
                //toggle shaking flag
                theCollider.gameObject.GetComponent<Shake>().isShaking = false;
				break;
				default:
				Debug.Log("[Visor] Error: no object found for collider tag");
				break;
			} //end switch
		} //end else
    } //end function

    //check for full charge, set to destroy

    //collision exit
    void OnTriggerExit2D(Collider2D theCollider) {
		//only check collisions if enabled
		if (collisionsEnabled == true) {
			//check collider tag
			switch (theCollider.gameObject.tag) {
				//target
				case TAG_TARGET:
				//Debug.Log("[Visor] Target Exit Collision Detected: " + theCollider.gameObject.GetComponent<Recharge>().charge);
				//toggle charging flag
				theCollider.gameObject.GetComponent<Recharge>().isCharging = false;
                //toggle shaking flag
                theCollider.gameObject.GetComponent<Shake>().isShaking = false;
				break;
				default:
				Debug.Log("[Visor] Error: no object found for collider tag");
				break;
			} //end switch
		} //end if
    }

    /*
     * Collision Notes
     * currently, isKinematic is bugged for rigidbody2d collisions
     * instead, set gravity = 1, check fixed angles, check trigger
     * when trigger is checked, only ontrigger functions will register
	 * add rigidbody (fixed angles, 0 gravity, not kinematic) and collider trigger to collision-checking object; only collider needed for basic collision, add rigidbody kinematic for moving objects
    
	//check collisions
    void OnCollisionStay2D(Collision2D theCollision) {
		//if (theCollision.gameObject.tag == "targets") {
			Debug.Log("TEST COLLISION: COLLISION STAY 2D");
		//}
		
	} //end function

    void OnCollisionEnter2D(Collision2D theCollision) {
        //if (theCollision.gameObject.tag == "targets") {
        Debug.Log("TEST COLLISION: COLLISION ENTER 2D");
        //}

    } //end function
    */

} //end class
