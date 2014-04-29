/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//utility script for managing shake animations
//chaotically alters an object's position to create a shake effect
public class Shake : MonoBehaviour {

    //properties
    public float intensity; //how intense the shake effect is
    public bool isShaking; //whether the object is currently shaking
	
	//update
	void Update () {
        //check whether currently shaking
        if (isShaking == true) {
            //update position with random deviation and set intensity
            Vector3 shakePos = Random.insideUnitSphere * intensity; //get random deviation
            Vector3 newPos = gameObject.transform.position; //store current object position
            //update new position based on deviation
            newPos.x += shakePos.x;
            newPos.y += shakePos.y;
            //update game object position
            gameObject.transform.position = newPos;
        }
	
	} //end function

} //end class
