/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
using System; //for Math
//TET SDK
using TETCSharpClient;
using TETCSharpClient.Data;

//Manage the data coming in from the eye tracker
//Data gets validated and reported to the game manager
public class GazeDataManager : MonoBehaviour, IGazeListener {
    //properties
    private Validator gazeValidator; //validator of tracking data
    public Point2D eyeCoords; //average coordinates from eye/head position
    public Point2D gazeCoords; //average coordinates from tet gaze data

	//init
	void Start () {
        //validate gaze data
        gazeValidator = new Validator(30);

        //listen for gaze events
        GazeManager.Instance.AddGazeListener(this);
	} //end function

    //IGazeListener onGazeUpdate used to receive tracking data
    //called each time new gaze data is sampled
    public void OnGazeUpdate(GazeData theData) {
        //update the latest gaze data
        gazeValidator.UpdateData(theData);
        
        //get data for eyes
        //for positioning based on eye/head position
        Eye eyeL = gazeValidator.getPrevEyeL();
        Eye eyeR = gazeValidator.getPrevEyeR();
        //get center point for eyes
        if (eyeL != null && eyeR != null) {
            double eyeX = Math.Min(eyeL.PupilCenterCoordinates.X, eyeR.PupilCenterCoordinates.X) + (Math.Abs((eyeR.PupilCenterCoordinates.X - eyeL.PupilCenterCoordinates.X)) / 2);
            double eyeY = Math.Min(eyeL.PupilCenterCoordinates.Y, eyeR.PupilCenterCoordinates.Y) + (Math.Abs((eyeR.PupilCenterCoordinates.Y - eyeL.PupilCenterCoordinates.Y)) / 2);
            eyeCoords = new Point2D(eyeX, eyeY);
        }
        
        //smoothed gaze data
        //for positioning based on gaze point
        Point2D prevSmoothCoords = gazeValidator.getPrevSmoothCoords();
        if (prevSmoothCoords != null) {
            gazeCoords = gazeValidator.getPrevSmoothCoords();
        }

        //print data
        //UnityEngine.Debug.Log("Avg Smooth Coordinates: (" + xAvgSmooth + ", " + yAvgSmooth + ")");
        
    } //end function

} //end class
