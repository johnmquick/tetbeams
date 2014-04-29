/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
using System; //for Math

//utility script for managing spawn position
//returns a random spawn position on or off screen with a specified buffer
//object should have rigidbody2d and renderer
public class Spawn : MonoBehaviour {
    //constants
    private const float UNITS_TO_PIXELS = 100.0f; //default units to pixels conversion for rendered textures

    //get a random spawn position
    public Vector3 randSpawnWithBufferOffScreen(float theBuffer, bool theOffScreen) {
        //store new random position
        float randX;
        float randY;

        //check that object has renderer
        float halfObjW; //0.5w of object in world space
        float halfObjH; //0.5h of object in world space
        if (gameObject.renderer != null) {
            //offset by half size to ensure object stays on screen
            //in viewport space
            halfObjW = (UNITS_TO_PIXELS * gameObject.renderer.bounds.extents.x) / Screen.width;
            halfObjH = (UNITS_TO_PIXELS * gameObject.renderer.bounds.extents.y) / Screen.height;
            //Debug.Log("[Spawn] Object Half Size: (" + halfObjW + ", " + halfObjH + ")");
        }
        else {
            //no offset based on object size
            halfObjW = 0;
            halfObjH = 0;
            Debug.Log("[Spawn] Object has no renderer - spawn pos will ignore object bounds");
        }

        //convert set edge buffer to world units
        //allows user to set buffer in pixels from inspector, rather than world units
        //also ensures that values are positive
        float edgeBufferX = Math.Abs(theBuffer / Screen.width);
        float edgeBufferY = Math.Abs(theBuffer / Screen.height);
        //Debug.Log("[Spawn] Edge buffer: " + edgeBufferX);

        //generate random position values in viewport space
        //for off-screen spawn
        if (theOffScreen == true) {
            //randomly decide whether x value will be off left side or right side
            float randValue = UnityEngine.Random.value;
            //off left side, random y
            if (randValue >= 0.75f) {
                randX = 0.0f - halfObjW - edgeBufferX;
                randY = UnityEngine.Random.Range(halfObjH + edgeBufferY, 1.0f - halfObjH - edgeBufferY);
            }
            //off right side, random y
            else if (randValue >= 0.50f) {
                randX = 1.0f + halfObjW + edgeBufferX;
                randY = UnityEngine.Random.Range(halfObjH + edgeBufferY, 1.0f - halfObjH - edgeBufferY);
            }
            //off top side, random x
            else if (randValue >= 0.25f) {
                randX = UnityEngine.Random.Range(halfObjW + edgeBufferX, 1.0f - halfObjW - edgeBufferX);
                randY = 1.0f + halfObjH + edgeBufferY;
            }
            //off bottom side, random x
            else {
                randX = UnityEngine.Random.Range(halfObjW + edgeBufferX, 1.0f - halfObjW - edgeBufferX);
                randY = 0.0f - halfObjH - edgeBufferY;
            }

            //Debug.Log("[Spawn] Rand Off Screen Spawn X/Y: (" + randX + ", " + randY + ")");
        }
        //otherwise, spawn on screen
        else {
            randX = UnityEngine.Random.Range(halfObjW + edgeBufferX, 1.0f - halfObjW - edgeBufferX);
            randY = UnityEngine.Random.Range(halfObjH + edgeBufferY, 1.0f - halfObjH - edgeBufferY);
            //Debug.Log("[Spawn] Rand On Screen Spawn X/Y: (" + randX + ", " + randY + ")");
        }       

        //create new random spawn point
        //convert from view space to world space
        Vector3 randSpawnView = new Vector3(randX, randY, 0);
        Vector3 randSpawnWorld = Camera.main.ViewportToWorldPoint(randSpawnView);
        randSpawnWorld.z = gameObject.rigidbody2D.transform.position.z; //maintain z position

        //return
        return randSpawnWorld;
    
    } //end function

} //end class
