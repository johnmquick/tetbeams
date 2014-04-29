/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
//TET SDK
using TETCSharpClient;
using TETCSharpClient.Data;

//for managing object movement based on gaze data
//converts gaze coordinates into a world position based on specified limits
public class GazeMove : MonoBehaviour {

    //for moving object based on gaze coordinates from tracker
    //based on resolution of window
    //receives object, renderer, bounds script, and data script
    //returns vector3 position in world coordinates
    public Vector3 positionObjectWithRendererBoundsData(GameObject theObject, Renderer theRenderer, Bounds theBounds, GazeDataManager theData) {
        //verify the gaze data
        if (theData.gazeCoords != null) {
            //convert gaze coords to screen coords
            Point2D screenCoords = DataUtilities.gazePointToWindowPoint(theData.gazeCoords);

            //convert window coords to viewport coords
            Point2D viewCoords = DataUtilities.windowPointToViewPoint(screenCoords);
            Vector3 viewVector = new Vector3((float)viewCoords.X, (float)(viewCoords.Y), 0);

            //check bounds
            //use the object with the outermost bounds and a renderer to make the check
            Vector3 boundsVector = theBounds.checkBoundsForRenderer(theRenderer, viewVector);

            //convert viewport vector to world position vector
            Vector3 worldPos = Camera.main.ViewportToWorldPoint(boundsVector);
            worldPos.z = theObject.transform.position.z; //maintain z position for object

            //return new world position
            return worldPos;

        }
        //error
        else {
            //Debug.Log("[GazeMove] Null gaze data, " + theObject.name + " cannot be positioned");
            return Vector3.zero;
        }
    } //end function

} //end class

            /*
            //for moving object based on eye/head position relative to tracker
            //check for valid coordinates from the data manager
            if (theData.eyeCoords != null) {
                //convert gaze coords to screen coords
                Point2D screenCoords = Utilities.gazePointToScreenPixels(theData.eyeCoords);
                //update position
                Vector3 visorPos = Utilities.positionObjectWithScreenPoint(theCam, theVisor, screenCoords);
                theVisor.transform.position = visorPos;
                UnityEngine.Debug.Log("Updated Pos: (" + visorPos.x + ", " + visorPos.y + ")");
            }
            */
