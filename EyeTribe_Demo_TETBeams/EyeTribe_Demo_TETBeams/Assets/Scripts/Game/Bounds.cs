/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//validates a position for an object with a renderer given constraints
public class Bounds : MonoBehaviour {
    //constants
    private const float UNITS_TO_PIXELS = 100.0f; //default pixels to units conversion for rendered textures
    private const float BOUNDS_BUFFER = 0.0f; //object boundary buffer in viewport space

    //check boundaries for a given renderer
    //do not allow object to move outside window bounds
    //assumes incoming vector is in viewport space
    public Vector3 checkBoundsForRenderer(Renderer theRenderer, Vector3 theCheckPos) {
        //UnityEngine.Debug.Log("Original CheckBounds Vector: (" + theCheckPos.x + ", " + theCheckPos.y + ")");

        //set up new vector components
        float newX;
        float newY;
        float newZ;

        //offset center so no part of object leaves screen
        //calculate relative to viewport space
        float halfObjW = (UNITS_TO_PIXELS * theRenderer.bounds.extents.x) / Screen.width; //0.5w of object in world space
        float halfObjH = (UNITS_TO_PIXELS * theRenderer.bounds.extents.y) / Screen.height; //0.5h of object in world space
        //UnityEngine.Debug.Log("Visor Half W/H in View Space: (" + halfObjW + ", " + halfObjH + ")");
        //UnityEngine.Debug.Log("Visor Half W/H in View Space: (" + theVisor.renderer.bounds.extents.x + ", " + theVisor.renderer.bounds.extents.y + ")");

        //create check positions
        //based on (0, 0) origin at bottom-left and (1, 1) full view size at upper-right
        float xMin = halfObjW + BOUNDS_BUFFER; //0 + 0.5w
        float xMax = 1.0f - halfObjW - BOUNDS_BUFFER; //1 - 0.5w
        float yMin = halfObjH + BOUNDS_BUFFER; //0 + 0.5h
        float yMax = 1.0f - halfObjH - BOUNDS_BUFFER; //1 - 0.5h

        //check bounds
        //x min
        if (theCheckPos.x < xMin) {
            newX = xMin;
        }
        //x max
        else if (theCheckPos.x > xMax) {
            newX = xMax;
        }
        //check cleared
        else {
            newX = theCheckPos.x;
        }

        //y min
        if (theCheckPos.y < yMin) {
            newY = yMin;
        }
        //y max
        else if (theCheckPos.y > yMax) {
            newY = yMax;
        }
        //check cleared
        else {
            newY = theCheckPos.y;
        }

        //retain z (if any)
        newZ = theCheckPos.z;

        //return new vector
        Vector3 newPos = new Vector3(newX, newY, newZ);
        //UnityEngine.Debug.Log("Visor Bounds Vector: (" + newPos.x + ", " + newPos.y + ", " + newPos.z + ")");
        return newPos;
    } //end function

} //end class
