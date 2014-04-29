/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
using System; //for Math

//manages a time-based charging system
public class Recharge : MonoBehaviour {
	//properties
    public float chargeMin; //minimum charge level
	public float chargeMax; //maximum charge level
	public float chargeIncrement = 1; //amount to increment charge level when filling
	public float chargeDecrement = 1; //amount to decrement charge level when draining
    public float charge; //current charge level
    public float prevCharge; //previous charge level
    public bool isCharging; //whether currently recharging or decharging
    public bool isChargeOnly; //whether the object only charges up
	public bool isDestroyedOnCharge; //whether object gets destroyed on full charge
	public bool isDechargeOnly; //whether the object only decharges
	public bool isDestroyedOnDecharge; //whether object gets destroyed on complete decharge

    private bool isRunning; //flag for preventing spawns while application is quitting

    //prefabs
    public Transform destroyPS; //optional particle system prefab called when object is destroyed

	//init
	void Start () {
		/*
		 * Set initial public properties in inspector
		 * For all examples, adjust the min/max and increment/decrement values for timing
		 * 
		 * EXAMPLE: a rechareable battery
		 * uncheck isChargeOnly, isDechargeOnly, isDestroyedOnCharge, isDestroyedOnDecharge
		 * signal (such as through user input) when to begin draining the charge (isCharging = false)
		 * signal (such as through power-ups or full decharge) when to refill the charge (isCharging = true)
		 * 
		 * EXAMPLE: destroy an object once fully drained
		 * check isDechargeOnly and isDestroyedOnDecharge
		 * set the opposite flags for an object that is destroyed once fully charged
		 * 
		 * EXAMPLE: a horizontal progress bar from left to right
		 * check isChargeOnly and set the charge to 0
		 * to resize, set the width of the bar to the current charge ratio 
		 * = the localscale x value * (current charge / max charge)
		 * to left align, offset the x position by the relative change in the charge level times half of the bar width
		 * = current x position + (charge - previous charge / max charge) * (bar width / 2 in world units)
         */

        //properties
        isRunning = true; //default to running at start

	}
	
	//update
	void Update () {
        //update current charge level
        updateCharge();

	} //end function

    //update current charge level
    public void updateCharge() {
        //update charge
        switch (isCharging) {
            //decharging
            case false:
                //make sure object is capable of decharging
                if (isChargeOnly == false) {
                    //reduce charge
                    if (charge > chargeMin) {
                        //Time.deltaTime gives the time in seconds that it took to complete the last frame
                        //use Time.deltaTime to make code framerate independent
                        charge -= chargeDecrement * Time.deltaTime;
                        //Debug.Log("[Recharge] Charge reduced: " + charge);
                    }
                    //at minimum
                    else {
                        charge = chargeMin;
                        //Debug.Log("[Recharge] Reached min charge");
                    }
                    //check charge level
                    checkCharge();
                }
                //update score
                //for visor only
                if (gameObject.tag == "visor") {
                    float chargeChange = Math.Abs(charge - prevCharge);
                    ScoreManager.Instance.chargeUsed += chargeChange;
                    //Debug.Log("[Recharge] updated score, charge used: " + ScoreManager.Instance.chargeUsed);
                }
                break;
            //recharging
            case true:
                //make sure object is capable of recharging
                if (isDechargeOnly == false) {
                    //increase charge
                    if (charge < chargeMax) {
                        charge += chargeIncrement * Time.deltaTime;
                        //Debug.Log("[Recharge] Charge increased: " + charge);
                    }
                    //at maximum
                    else {
                        charge = chargeMax;
                        //Debug.Log("[Recharge] Reached max charge");
                    }
                    //check charge level
                    checkCharge();
                }
                break;
            //error
            default:
                Debug.Log("[Recharge] Error: neither charging nor decharging");
                break;
        } //end switch
    }

    //check charge level
    public void checkCharge() {
        //if object is fully charged and is destroyed once full
        if (isDestroyedOnCharge == true && 
			charge >= chargeMax) {
            //Debug.Log("[Recharge] Full charge destroy");
            //set charge
            charge = chargeMax;
            //destroy parent game object
            Destroy(gameObject);
        }

		//if object is fully decharged and is destroyed once empty
		if (isDestroyedOnDecharge == true &&
			charge <= chargeMin) {
            //Debug.Log("[Recharge] Full decharge destroy");
            //set charge
            charge = chargeMin;
			//destroy parent game object
			Destroy(gameObject);
		}
    }

    //update previous recharge level
    public void updatePrevRecharge() {
        //update previous charge
        prevCharge = charge;
    }

    //call just before destroy
    void OnDestroy() {   
        //only create if application is still running (prevents Unity error with cleanup of objects)
        //only create if object is being destroyed due to charging and not some other method
        if (isRunning == true && 
            (charge == chargeMax && isDestroyedOnCharge == true) || 
            (charge == chargeMin && isDestroyedOnDecharge == true)) {
            //update score
            ScoreManager.Instance.hitObjects++;
            //Debug.Log("[Recharge] updated score, hit objects: " + ScoreManager.Instance.hitObjects);
            //check for valid particle system
            if (destroyPS != null &&
                destroyPS.GetComponent<ParticleSystem>() != null) {
                //create particle system at position of object
                Transform newPS = (Transform)Instantiate(destroyPS); //clone prefab 
                newPS.transform.position = gameObject.transform.position; //set position
                newPS.parent = gameObject.transform.parent; //add to parent container
                Destroy(newPS.gameObject, newPS.GetComponent<ParticleSystem>().duration); //destroy on completion
            } //end inner if
            //audio
            AudioManager.Instance.playHit(); //sfx
        } //end outer if
    } //end function

    //when application is quit
    void OnApplicationQuit() {
        //toggle flag
        isRunning = false;
    } 

} //end class
