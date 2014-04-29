/*
 * 
 * All content created and copyright © 2014 by John M. Quick, except where noted below.

 * Note: Functions which return a Point2D type were lawfully adapted 
 * from documentation, source code, and sample projects provided by
 * the following source.
 * 
 * Source:
 * Copyright (c) 2014, The Eye Tribe
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 * 
*/

using UnityEngine;
using System.Collections;
//TET SDK
using TETCSharpClient;
using TETCSharpClient.Data;

//utilities for converting coordinates between tracker, window, screen 
public class DataUtilities : GazeUtils {

    //convert gaze coordinates (raw or smoothed) to window coordinates
    //Unity origin = bottom-left; max = (windowW, windowH)
    public static Point2D gazePointToWindowPoint(Point2D theGazePoint) {
        //UnityEngine.Debug.Log("Original Gaze Point: (" + theGazePoint.X + ", " + theGazePoint.Y + ")");
        //invert y-axis
        double newX = theGazePoint.X;
        double newY = (Screen.height - theGazePoint.Y);
        //return screen coordinates
        Point2D screenPoint = new Point2D(newX, newY);
        //UnityEngine.Debug.Log("Gaze to Window Point: (" + screenPoint.X + ", " + screenPoint.Y + ")");
        return screenPoint;
    }

    //convert Unity window coordinates to Unity viewport coordinates
    //Unity origin = bottom-left corner; max = (1, 1)
    public static Point2D windowPointToViewPoint(Point2D theWindowPoint) {
        //convert window X and Y coordinates to Unity viewport
        double newX = theWindowPoint.X / (double)Screen.width;
        double newY = theWindowPoint.Y / (double)Screen.height;
        //UnityEngine.Debug.Log("Screen Resolution: " + Screen.width + " x " + Screen.height);
        //UnityEngine.Debug.Log("GazeManager Resolution: " + GazeManager.Instance.ScreenResolutionWidth + " x " + GazeManager.Instance.ScreenResolutionHeight);
        //return viewport coordinates
        Point2D viewPoint = new Point2D(newX, newY);
        //UnityEngine.Debug.Log("Window to View Point: (" + viewPoint.X + ", " + viewPoint.Y + ")");
        return viewPoint;
    }

    //convert gaze coordinates (raw or smoothed) to screen pixels
    //Unity origin = bottom-left; max = (fullResW, fullResH)
    public static Point2D gazePointToScreenPixels(Point2D theGazePoint) {
        return getRelativeToScreenSpace(theGazePoint, Screen.width, Screen.height);
    }

    //position a game object in world given screen pixels
    public static Vector3 positionObjectWithScreenPoint(Camera theCamera, GameObject theObject, Point2D thePoint) {
        //convert origin to bottom-left to match Unity
        Vector3 unityPos;
        unityPos = new Vector3((float)thePoint.X, (float)(Screen.height - thePoint.Y), 0);

        //align object to center of point
        unityPos.x = unityPos.x - (theObject.transform.localScale.x / 2);
        unityPos.y = unityPos.y - (theObject.transform.localScale.y / 2);

        //convert screen coords to world coords
        Vector3 worldPos;
        worldPos = theCamera.ScreenToWorldPoint(unityPos);

        //store z axis data
        worldPos.z = theObject.transform.position.z;

        //return new position vector
        return worldPos;
    }

    /*
    //convert gaze coordinates (raw or smoothed) to vector
    public static Vector2 gazePointToVector2(Point2D theGazePoint) {
        //return vector created from given point
        Vector2 newVector = new Vector2((float)theGazePoint.X, (float)theGazePoint.Y);
        return newVector;
    }

    //convert screen coordinates to window coordinates
    //Unity origin = bottom-left corner; max = (windowW, windowH)
    //relative to full screen resolution
    public static Point2D screenPointToWindowPoint(Point2D theScreenPoint) {
        //convert gaze X and Y coordinates to Unity window
        double newX = theScreenPoint.X * ((double)Screen.width / GazeManager.Instance.ScreenResolutionWidth);
        double newY = theScreenPoint.Y * ((double)Screen.height / GazeManager.Instance.ScreenResolutionHeight);
        //UnityEngine.Debug.Log("Screen Resolution: " + Screen.width + " x " + Screen.height);
        //UnityEngine.Debug.Log("GazeManager Resolution: " + GazeManager.Instance.ScreenResolutionWidth + " x " + GazeManager.Instance.ScreenResolutionHeight);
        //return window coordinates
        Point2D windowPoint = new Point2D(newX, newY);
        UnityEngine.Debug.Log("Screen to Window Point: (" + windowPoint.X + ", " + windowPoint.Y + ")");
        return windowPoint;
    }
    */
} //end class
