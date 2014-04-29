/*
 * 
 * All content created and copyright © 2014 by John M. Quick, except where noted below.

 * Note: The TET data validation process and functions were lawfully adapted 
 * from documentation, source code, and sample projects provided by the
 * following source.
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//TET SDK
using TETCSharpClient;
using TETCSharpClient.Data;

//utilities for caching and validating gaze data
//used to handle effects of frame drops due to lost tracking
public class Validator {

	//properties
    private double minEyeDist = 0.1f; //min eye distance
    private double maxEyeDist = 0.3f; //max eye distance
    private double prevEyeDist; //last valid eye distance
    private double prevEyeAngle; //last valid eye angle
    private FixedSizeQueue<GazeData> frameQueue; //queue for storing frame data
    private Eye prevEyeL; //previous valid left eye data
    private Eye prevEyeR; //previous valid right eye data
    private Point2D prevRawCoords; //previous valid raw coordinates
    private Point2D prevSmoothCoords; //previous valid smoothed coordinates
    private Point2D prevUserCoords; //previous valid user coordinates

    //init
    public Validator(int theQueueLength) {
        //initialize properties
        frameQueue = new FixedSizeQueue<GazeData>(theQueueLength);
        prevUserCoords = new Point2D();
    }
    	
	//update valid gaze data
	public void UpdateData(GazeData theNewFrame) {
        //queue the new gaze data
        frameQueue.Enqueue(theNewFrame);

        //prepare to store valid gaze data
        Eye eyeL = null;
        Eye eyeR = null;
        Point2D rawCoords = null;
        Point2D smoothCoords = null;
        GazeData gazeData;

        //loop backwards through data frames and set valid values
        for (int i = frameQueue.Count - 1; i > 0; i--) {
            //get current frame
            gazeData = frameQueue.ElementAt(i);

            //check for tracking errors with eyes
            //if tracking state is not reported failed or lost
            if ((gazeData.State & GazeData.STATE_TRACKING_FAIL) == 0 &&
                (gazeData.State & GazeData.STATE_TRACKING_LOST) == 0) {
                //check that left eye coords are not null/0
                if (eyeL == null && 
                    gazeData.LeftEye != null &&
                    gazeData.LeftEye.PupilCenterCoordinates.X != 0 &&
                    gazeData.LeftEye.PupilCenterCoordinates.Y != 0) {
                    //set the left eye data
                    eyeL = gazeData.LeftEye;
                } //end eyeL if
                //check that right eye coords are not null/0
                if (eyeR == null &&
                    gazeData.RightEye != null &&
                    gazeData.RightEye.PupilCenterCoordinates.X != 0 &&
                    gazeData.RightEye.PupilCenterCoordinates.Y != 0) {
                    //set the right eye data
                    eyeR = gazeData.RightEye;
                } //end eyeR if
            } //end outer if

            //check for existing coordinates
            if (rawCoords == null &&
                gazeData.RawCoordinates.X != 0 &&
                gazeData.RawCoordinates.Y != 0) {
                //set raw and smoothed coordinates
                rawCoords = gazeData.RawCoordinates;
                smoothCoords = gazeData.SmoothedCoordinates;            
            }

            //break for loop once values are validated
            if (eyeL != null && eyeR != null && rawCoords != null) {
                //UnityEngine.Debug.Log("EYE L AND R VALID");
                break;
            }

        } //end for

        //once valid values are set, store them in previous variables
        if (eyeL != null) {
            prevEyeL = eyeL;
        }
        if (eyeR != null) {
            prevEyeR = eyeR;
        }
        if (rawCoords != null) {
            prevRawCoords = rawCoords;
            prevSmoothCoords = smoothCoords;
        }

        //update user coordinates, distance, and angle
        //if valid data from eyes is stored
        if (prevEyeL != null && prevEyeR != null) {
            //update user coordinates
            lock (prevUserCoords) {
                //take average of x and y values from L and R eyes
                prevUserCoords.X = (prevEyeL.PupilCenterCoordinates.X + prevEyeR.PupilCenterCoordinates.X) / 2;
                prevUserCoords.Y = (prevEyeL.PupilCenterCoordinates.Y + prevEyeR.PupilCenterCoordinates.Y) / 2;
            } //end lock

            //update distance
            double eyeDist = Point2DDist(prevEyeL, prevEyeR);
            //update max and min distance values
            if (eyeDist < minEyeDist) {
                minEyeDist = eyeDist;
            }
            if (eyeDist > maxEyeDist) {
                maxEyeDist = eyeDist;
            }
            //update previous distance
            prevEyeDist = eyeDist / (maxEyeDist - minEyeDist);

            //update angle
            //convert radians to degrees
            //get radian angle based on rotation of eyes
            prevEyeAngle = ((180 / Math.PI * Math.Atan2(prevEyeR.PupilCenterCoordinates.Y - prevEyeL.PupilCenterCoordinates.Y, prevEyeR.PupilCenterCoordinates.X - prevEyeL.PupilCenterCoordinates.X)));

        } //end outer if
	
	} //end function

    //calculate distance between two points using distance formula
    private double Point2DDist(Eye theFirstEye, Eye theSecondEye) {
        double dist = Math.Abs(Math.Sqrt(
            Math.Pow(theSecondEye.PupilCenterCoordinates.X - theFirstEye.PupilCenterCoordinates.X, 2) +
            Math.Pow(theSecondEye.PupilCenterCoordinates.Y - theFirstEye.PupilCenterCoordinates.Y, 2)
            ));
        return dist;
    } //end function

    //property getters
    public Eye getPrevEyeL() {
        return prevEyeL;
    }
    public Eye getPrevEyeR() {
        return prevEyeR;
    }
    public Point2D getPrevRawCoords() {
        return prevRawCoords;
    }
    public Point2D getPrevSmoothCoords() {
        return prevSmoothCoords;
    }
    public Point2D getPrevUserCoords() {
        return prevUserCoords;
    }
    public double getPrevEyeDist() {
        return prevEyeDist;
    }
    public double getPrevEyeAngle() {
        return prevEyeAngle;
    }


    //fixed size queue class
    class FixedSizeQueue<T>:Queue<T> {
        //properties
        private int limit = -1;

        //setter and getters
        public int Limit {
            get {
                return limit;
            }
            set {
                limit = value;
            }
        }
        //initialization
        public FixedSizeQueue(int theLimit) : base(theLimit) {
            this.Limit = theLimit;
        }
        //enqueue function
        public new void Enqueue(T theItem) {
            while (this.Count >= this.Limit) {
                this.Dequeue();
            }
            base.Enqueue(theItem);
        }
        
    } //end inner class

} //end class
