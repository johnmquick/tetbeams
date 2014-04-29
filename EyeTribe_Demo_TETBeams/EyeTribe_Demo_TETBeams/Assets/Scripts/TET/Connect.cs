/*
 * 
 * All content created and copyright © 2014 by John M. Quick, except where noted below.

 * Note: The TET connection process and functions were lawfully adapted 
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
using System.Diagnostics; //for checking process
using Microsoft.Win32; //for registry access
using System.IO; //for checking file existence
//TET SDK
using TETCSharpClient;

//connects to the TET server
//attach to designated object in the calibration scene
public class Connect : MonoBehaviour {
    //constants
    private const string TAG_GLOBAL = "global"; //tag for global game object

    //properties
    private GameObject globalObj; //global object
    private Global globalScript; //global script 

    //init
    void Start() {
        //get game objects from scene
        globalObj = GameObject.FindWithTag(TAG_GLOBAL); //global game object
        globalScript = globalObj.GetComponent<Global>(); //global script

        //check whether the tet server process is running and start it if needed
        bool tetRunning = checkTetProcess();

        //start the process if it is not running
        if (tetRunning == false) {
            //start process
            startTetProcess();
        }
        
    }

    //update
    void Update() {
        //check for prepared tracker state
        if (GazeManager.Instance.Trackerstate == GazeManager.TrackerState.TRACKER_CONNECTED) {
        //if (GazeManager.Instance.IsConnected == true) {
            //update global
            updateGlobal();
        }
    }

    //update global flags
    private void updateGlobal() {
        //update global flag
        globalScript.tetConnected = true;
            
        //disable script once tet server is running
        this.enabled = false;

    }

    //check whether the tet server process is running
    public bool checkTetProcess() {
        bool tetRunning = false; //flag to check whether tet server is already running
        string tetProcessName = "EyeTribe"; //name of tet server process
        Process[] allProcesses = Process.GetProcesses(); //store any running processes

        //loop through current processes to find tet server
        foreach (Process theProcess in allProcesses) {
            try {
                //if tet server process is found
                if (theProcess.ProcessName == tetProcessName) {
                    //toggle flag
                    tetRunning = true;
                    UnityEngine.Debug.Log("TET Server Already Running");
                }
            }
            //ignore any processes that Unity can't access
            catch (System.InvalidOperationException /*theException*/) {
                //UnityEngine.Debug.Log("Process not accessible - ignore: " + theException);
            }
        }

        //return
        return tetRunning;
    }

    //start the tet process
    public void startTetProcess() {
        UnityEngine.Debug.Log("Starting TET Server");
        //registry path for the tet server exe
        //Registry.GetValue(key name, value name, default return)
        string tetRegistry = (string)Registry.GetValue(@"HKEY_CURRENT_USER\Software\EyeTribe\EyeTribe Service", "InstallDir", string.Empty);

        //check whether registry path exists
        if (tetRegistry != string.Empty) {
            //exe name for the tet server process
            string tetExeName = "EyeTribe.exe";

            //full path for tet server process
            string tetExePath = tetRegistry + tetExeName;

            //start the tet server
            Process.Start(tetExePath);
        }
        //check the default paths
        else {
            //default x86 tet server exe path
            string tetPathX86 = @"C:\Program Files (x86)\EyeTribe\Server\EyeTribe.exe";

            //default x64 tet server exe path
            string tetPathX64 = @"C:\Program Files\EyeTribe\Server\EyeTribe.exe";

            //x86
            if (File.Exists(tetPathX86)) {
                //start the tet server
                Process.Start(tetPathX86);
            }
            //x64
            else if (File.Exists(tetPathX64)) {
                //start the tet server
                Process.Start(tetPathX64);
            }
            //process not found in any checks
            else {
                UnityEngine.Debug.Log("Error: tet server process not found or not installed");
            }
        }
    }

} //end class
