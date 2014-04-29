/*
 * 
 * All content created and copyright © 2014 by John M. Quick, except where noted below.

 * Note: The TET calibration process and functions were lawfully adapted 
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
using System.Collections;
using System.Collections.Generic; //for List<>
using System; //for Math
//TET SDK
using TETCSharpClient;
using TETCSharpClient.Data;

//calibrates the TET server
//attach to game object in the calibration scene
//implement IGazeListener interface to receive data from TET server
//implement ICalibrationProcessHandler to calibrate TET server
public class CalibrationManager : MonoBehaviour, IGazeListener, ICalibrationProcessHandler {

    //constants
    private const float PIXELS_TO_UNITS = 1.0f / 100.0f; //default pixels to units conversion for rendered textures
    private const string TAG_EYEL = "eyeL"; //tag for left eye game object in scene
    private const string TAG_EYER = "eyeR"; //tag for right eye game object in scene
    private const string TAG_CALPOINT = "calPoint"; //tag for calibration point game object in scene
    private const int MAX_CAL_ATTEMPT = 3; //maximum number of times calibration will be attempted
    private const int MAX_CAL_RESAMPLE = 4; //maximum number of times calibration points will be resampled
    private const int MIN_CAL_POINTS = 9; //minimum number of calibration points
    private const float CAL_BUFFER_PCT = 0.15f; //percentage of buffer around screen when generating calibration points
    private const float CAL_SAMPLE_SWITCH_DELAY = 0.25f; //delay before getting next calibration point
    private const float CAL_SAMPLE_START_DELAY = 0.25f; //delay before starting to sampling calibration point
    private const float CAL_SAMPLE_END_DELAY = 1.50f; //delay before ending sampling of calibration point
	private const float EYE_DIST_MOD = 0.5f; //modifier for calculating eye distance and scale

    //properties
    private Camera cam; //main camera
    private GameObject eyeL; //left eye object
    private GameObject eyeR; //right eye object

	private double eyeDist; //distance between eyes and tracker
	private Vector3 eyeScale; //scale eye objects based on distance to tracker

    private GameObject calPointObj; //calibration point game object
    private Point2D calPointCoords; //calibration point coordinates
    private List<Point2D> calPointList; //list of calibration point coordinates
    private int calAttempts; //current number of calibration attempts
    private bool calSuccess; //whether calibration has been successfully completed
    public delegate void taskDelegate(); //delegate for queueing tasks
    private Queue<taskDelegate> _taskQueue; //queue for holding delegated tasks

    private Validator gazeValidator; //validator of tracking data

    private bool hideGUI; //whether to show or hide the GUI

    //awake
    //called before start function
    void Awake() {
        //set the camera's size based on the current resolution and pixels to units ratio
        Camera.main.orthographicSize = Screen.height * PIXELS_TO_UNITS / 2; //half of the current window size
    }

	//init
	void Start() {
        //get game objects from scene
        cam = Camera.main; //main camera
        eyeL = GameObject.FindWithTag(TAG_EYEL); //l eye
        eyeR = GameObject.FindWithTag(TAG_EYER); //r eye
		eyeScale = eyeL.transform.localScale; //baseline scale for eye objects
        calPointObj = GameObject.FindWithTag(TAG_CALPOINT); //calibration point
        calPointObj.renderer.enabled = false; //invisible to start
        calPointObj.transform.position = Vector3.zero; //position to 0, 0, 0
        calPointList = new List<Point2D>(); //prepare to store calibration points

        //init properties
        _taskQueue = new Queue<taskDelegate>();
        calAttempts = 0;
        calSuccess = false;
        hideGUI = false;
        Screen.showCursor = true; //show cursor

        //validate gaze data
        gazeValidator = new Validator(30);

        //listen for gaze events
        GazeManager.Instance.AddGazeListener(this);

        //transition into scene via state manager
        if (StateManager.Instance != null) {
            TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
            theTransition.toggleFade();
        }

        //audio
        /*
        if (AudioManager.Instance != null) {
            AudioManager theAudioManager = AudioManager.Instance.gameObject.GetComponent<AudioManager>();
            theAudioManager.toggleFade();
        }
        */
    }

    //IGazeListener onGazeUpdate used to receive tracking data
    //called each time new gaze data is sampled
    public void OnGazeUpdate(GazeData theData) {
        //update the latest gaze data
        gazeValidator.UpdateData(theData);
    }

    //called each frame
    void Update() {
        //show/hide mouse cursor based on gui
        Screen.showCursor = !hideGUI;

        //check whether calibration is in process
        //if not calibrating, update the eye game objects
        if (GazeManager.Instance.IsCalibrating == false) {
            //get the last validated gaze coordinates
            Point2D prevUserCoords = gazeValidator.getPrevUserCoords();

            //if previous user coordinates exist
            if (prevUserCoords != null) {
                //set visibility of eye game objects
                //L eye
                if (eyeL.renderer.enabled == false) {
                    eyeL.renderer.enabled = true;
                }
                //R eye
                if (eyeR.renderer.enabled == false) {
                    eyeR.renderer.enabled = true;
                }

				//change size of eye game objects based on distance from tracker
				eyeDist = gazeValidator.getPrevEyeDist() * EYE_DIST_MOD;
                Vector3 eyeScaleMod = new Vector3((float)eyeDist, (float)eyeDist, (float)eyeScale.z);
				double eyeAngleMod = gazeValidator.getPrevEyeAngle();
                
                //get previous data for eyes
                Eye prevEyeL = gazeValidator.getPrevEyeL();
                Eye prevEyeR = gazeValidator.getPrevEyeR();

                //update eye game object positions based on gaze data
                //L eye
                if (prevEyeL != null) {
                    //convert gaze coords to screen coords
                    Point2D pointL = DataUtilities.gazePointToScreenPixels(prevEyeL.PupilCenterCoordinates);
                    //update position
                    eyeL.transform.position = DataUtilities.positionObjectWithScreenPoint(cam, eyeL, pointL);
					//update scale
					eyeL.transform.localScale = eyeScaleMod;
					//update angle
					eyeL.transform.localEulerAngles = new Vector3(eyeL.transform.eulerAngles.x, eyeL.transform.eulerAngles.y, (float)eyeAngleMod);
                }
                //R eye
                if (prevEyeR != null) {
                    //convert gaze coords to screen coords
                    Point2D pointR = DataUtilities.gazePointToScreenPixels(prevEyeR.PupilCenterCoordinates);
                    //update position
                    eyeR.transform.position = DataUtilities.positionObjectWithScreenPoint(cam, eyeR, pointR);
					//update scale
					eyeR.transform.localScale = eyeScaleMod;
					//update angle
					eyeR.transform.localEulerAngles = new Vector3(eyeR.transform.eulerAngles.x, eyeR.transform.eulerAngles.y, (float)eyeAngleMod);
                }

            } //end prevUserCoords check
        } //end IsCalibrating if
        //if calibrating, hide the eye game objects
        else {
            //L eye
            if (eyeL.renderer.enabled == true) {
                eyeL.renderer.enabled = false;
            }
            //R eye
            if (eyeR.renderer.enabled == true) {
                eyeR.renderer.enabled = false;
            }
        } //end IsCalibrating else

        //handle task queue via Update()
        lock (_taskQueue) {
            while (_taskQueue.Count > 0) {
                _taskQueue.Dequeue()();
            }
        }
    
    } //end Update function

    //generate points to be used during calibration process based on window size and given number of points
    private void generateCalPoints(int numPoints) {
        //window properties
        double buffer = (double)Screen.height * CAL_BUFFER_PCT;
        double centerW = (double)Screen.width / 2;
        double centerH = (double)Screen.height / 2;

        //clear current point list
        calPointList.Clear();

        //generate points
        calPointList.Add(new Point2D(buffer, buffer)); //TL
        calPointList.Add(new Point2D(buffer, centerH)); //ML
        calPointList.Add(new Point2D(buffer, Screen.height - buffer)); //BL
        calPointList.Add(new Point2D(centerW, buffer)); //TM
        calPointList.Add(new Point2D(centerW, centerH)); //MM
        calPointList.Add(new Point2D(centerW, Screen.height - buffer)); //BM
        calPointList.Add(new Point2D(Screen.width - buffer, buffer)); //TR
        calPointList.Add(new Point2D(Screen.width - buffer, centerH)); //MR
        calPointList.Add(new Point2D(Screen.width - buffer, Screen.height - buffer)); //BR

        //randomize point order
        ShuffleListOfType<Point2D>(calPointList);
    }

    //randomize list order
    private void ShuffleListOfType<theType>(IList<theType> theList) {
        //get random number generator
        System.Random randGen = new System.Random();
        //get number of list items
        int numItems = theList.Count;
        //reorder each list item
        while (numItems > 1) {
            //increment num items
            numItems--;
            //get a random value
            int randValue = randGen.Next(numItems + 1);
            //get random item from list
            theType listItem = theList[randValue];
            //swap positions
            theList[randValue] = theList[numItems];
            theList[numItems] = listItem;
        }
    }

    //called when calibration process has started
    public void OnCalibrationStarted() {
        UnityEngine.Debug.Log("Initiating calibration process");
        //handle in task queue
        addTaskToQueue(new taskDelegate(delegate {
            //get next calibration point after given delay
            //Invoke(method name string, time float in seconds);
            Invoke("nextCalPoint", CAL_SAMPLE_SWITCH_DELAY);
        }));
    }

    //called when a single calibration point is sampled
    public void OnCalibrationProgress(double theSamplePoint) {
        //UnityEngine.Debug.Log("New sample point processed: " + theSamplePoint);
    }

    //called when processing of all calibration results begins
    public void OnCalibrationProcessing() {
       // UnityEngine.Debug.Log("Processing calibration results");
    }

    //called when calibration results are finished processing
    public void OnCalibrationResult(CalibrationResult theCalibrationResult) {
        //UnityEngine.Debug.Log("Calibration results: " + theCalibrationResult);

        //check for invalid result
        if (theCalibrationResult.Result == false) {
            UnityEngine.Debug.Log("Calibration Result Invalid - Resample");
            //validate points
            foreach (var calPoint in theCalibrationResult.Calibpoints) {
                //if point is flagged for resampling or without data
                if (calPoint.State == CalibrationPoint.STATE_RESAMPLE ||
                    calPoint.State == CalibrationPoint.STATE_NO_DATA) {
                    //add point back to calibration list
                    calPointList.Add(new Point2D(calPoint.Coordinates.X, calPoint.Coordinates.Y));
                } //end if
            } //end foreach

            //abort calibration if max attempts or calibration points exceeded
            calAttempts++;
            if (calAttempts >= MAX_CAL_ATTEMPT ||
                calPointList.Count >= MAX_CAL_RESAMPLE) {
                //clear list
                calPointList.Clear();
                //abort
                GazeManager.Instance.CalibrationAbort();
                //show GUI so calibration can be attempted again
                hideGUI = false;
                //return (exit)
                return;
            }

            //handle in task queue
            addTaskToQueue(new taskDelegate(delegate {
                //get next calibration point after given delay
                Invoke("nextCalPoint", CAL_SAMPLE_SWITCH_DELAY);
            }));
        } //end outer if
        //valid result, handle post-calibration activity
        else {
            UnityEngine.Debug.Log("Calibration Result Valid");
            //toggle calibration success flag
            calSuccess = true;
			//show GUI so player can advance to next scene
            hideGUI = false;
        } //end outer else
    } //end function

    //display the next calibration point
    private void nextCalPoint() {
        //check whether there are points left to display
        if (calPointList.Count > 0) {
            //set current cal point
            calPointCoords = calPointList[0];
            //remove cal point from list
            calPointList.RemoveAt(0);
            //position game object to calibration point
            calPointObj.transform.position = DataUtilities.positionObjectWithScreenPoint(cam, calPointObj, calPointCoords);
            //set game object visibility
            calPointObj.renderer.enabled = true;
            //sample calibration point after given delay
            Invoke("sampleCalPointStart", CAL_SAMPLE_START_DELAY);
            //end sampling after given delay
            Invoke("sampleCalPointEnd", CAL_SAMPLE_END_DELAY);

        }
    }

    //start sample calibration point
    private void sampleCalPointStart() {
        //start calibration at current point
        GazeManager.Instance.CalibrationPointStart((int)Math.Round(calPointCoords.X), (int)Math.Round(calPointCoords.Y));
    }

    //end sample calibration point
    private void sampleCalPointEnd() {
        //end calibration at current point
        GazeManager.Instance.CalibrationPointEnd();
        //set game object visibility
        calPointObj.renderer.enabled = false;
        //audio
        AudioManager.Instance.playBtnClick(); //sfx
        //if points remain, get next point after given delay
        if (calPointList.Count > 0) {
            Invoke("nextCalPoint", CAL_SAMPLE_SWITCH_DELAY);
        }
    }

    //add task to queue for later handling in Unity's Update()
    public void addTaskToQueue(taskDelegate theTask) {
        lock (_taskQueue) {
            _taskQueue.Enqueue(theTask);
        }
    }

    //GUI
    void OnGUI() {
        //set gui skin
        if (GameObject.FindWithTag("global")) {
            GUI.skin = GameObject.FindWithTag("global").GetComponent<Global>().menuGUI;
        }

        //check whether GUI should be shown
        if (hideGUI == false) {
            //create calibration button
            int btnW = 300;
            int btnH = 100;
            float btnX = Screen.width / 2 - btnW / 2;
            float btnY = Screen.height / 2 - btnH / 2;
            Rect btnRect = new Rect(btnX, btnY, btnW, btnH);
            string btnText = "Click to calibrate";
            if (calAttempts > 0) {
                btnText = "Calibration failed: Recalibrate";
            }
            if (calSuccess == true) {
                btnText = "Calibration successful: Continue";
            }
            if (GUI.Button(btnRect, btnText)) {
                //proceed to next scene
                if (calSuccess == true) {
                    UnityEngine.Debug.Log("Load Main Menu");
                    //remove gaze listener
                    GazeManager.Instance.RemoveGazeListener(this);
                    //handle in task queue
                    addTaskToQueue(new taskDelegate(delegate {
                        //transition to next scene
                        TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
                        theTransition.toggleFade();
                        StateManager.Instance.switchSceneAfterDelay("mainMenu", theTransition.duration);
                        //audio
                        AudioManager theAudioManager = AudioManager.Instance.gameObject.GetComponent<AudioManager>();
                        theAudioManager.playBtnClick(); //sfx
                        //theAudioManager.toggleFade(); //bgm
                    }));
                }
                //restart calibration process
                else if (GazeManager.Instance.IsCalibrating == false && calSuccess == false) {
                    //reset attempts
                    calAttempts = 0;
                    //generate calibration points
                    generateCalPoints(MIN_CAL_POINTS);
                    //CalibrationStart(num points {min = 9}, calibration process handler)
                    GazeManager.Instance.CalibrationStart(MIN_CAL_POINTS, this);
                    //audio
                    AudioManager theAudioManager = AudioManager.Instance.gameObject.GetComponent<AudioManager>();
                    theAudioManager.playBtnClick(); //sfx
                }

                //hide GUI
                hideGUI = true;
            } //end inner if

        } //end outer if    
    } //end function
	
} //end class
