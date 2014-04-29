/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//manages the main game loop
public class GameManager : MonoBehaviour {
    //constants
	private const float PIXELS_TO_UNITS = 1.0f / 100.0f; //default pixels to units conversion for rendered textures
    private const string TAG_GAME = "gameManager"; //tag for game manager object
    private const string TAG_DIMVIEW = "dimView"; //tag for background dim view game object
	private const string TAG_VISOR = "visor"; //tag for visor game object
    private const string TAG_VISOR_FRAMEVIEW = "visorFrameView"; //tag for visor frame view game object
	private const string TAG_TARGETS = "targets"; //tag for targets game object container
    private const string TAG_TUTORIAL = "tutorial"; //tag for tutorial game object container

    //properties
    private int currentLevel; //number representing the current level in game state
    private bool isGameOver; //whether the game has ended

    //prefabs
    //assign in inspector window
	public Transform target_00; //type 0: moves extremely slow to center of screen and stops
	public Transform target_01; //type 1: moves very slow to rand dest and stops
	public Transform target_02; //type 2: moves medium speed, rand dest loop
    public Transform target_03; //type 3: moves medium speed, rand dest loop 10, destroy
    public Transform target_04; //type 4: moves medium speed to rand dest, origin loop
    public Transform target_05; //type 5: moves medium speed to rand dest, origin loop 11, destroy
    public Transform target_06; //type 6: moves fast to rand dest, origin loop 9, destroy
    public Transform target_07; //type 7: moves fast to rand dest, origin loop 7, destroy
    public Transform target_08; //type 8: moves very fast to rand dest, origin loop 2, destroy

    //objects
    private Camera theCam; //main camera
    private GameObject theGame; //game manager object in scene
    private GameObject theDimView; //dim view game object in scene
	private GameObject theVisor; //visor object in scene
    private GameObject theVisorFrameView; //visor frame view object in scene
    private GameObject theTargets; //targets game object container in scene
    private GameObject theTutorial; //tutorial game object container in scene
	
	//scripts
	private Bounds theBounds; //boundary script for managing object positions
	private GazeDataManager theData; //manager script for incoming gaze data
    private Wave theWave; //script for managing waves
	private Recharge theVisorRechargeScript; //recharge script associated with visor object
    private GazeMove theVisorGazeMoveScript; //gazemove script associated with visor object
    
    //debug
    public bool debugOn; //toggle debug mode

    //awake
    //called before start function
    void Awake () {
        //set the camera's size based on the current resolution and pixels to units ratio
        Camera.main.orthographicSize = Screen.height * PIXELS_TO_UNITS / 2; //half of the current window size

        //hide mouse cursor
        Screen.showCursor = false;
    }

	//init
	void Start () {
        
        //debug
        debugOn = false; //default to off

        //properties
        currentLevel = 0; //initial level
        isGameOver = false; //initial flag

        //objects
        theCam = Camera.main; //main camera in scene
        theGame = GameObject.FindWithTag(TAG_GAME); //game manager object in scene
        theDimView = GameObject.FindWithTag(TAG_DIMVIEW); //dim view game object in scene
		theVisor = GameObject.FindWithTag(TAG_VISOR); //visor object in scene
        theVisorFrameView = GameObject.FindWithTag(TAG_VISOR_FRAMEVIEW); //visor frame view object in scene
        theTargets = GameObject.FindWithTag(TAG_TARGETS); //targets game object container in scene
        theTutorial = GameObject.FindWithTag(TAG_TUTORIAL); //tutorial game object container in scene

		//scripts
		theBounds = theGame.AddComponent<Bounds>(); //add new bounds script
		theData = theGame.AddComponent<GazeDataManager>(); //add new data manager script
        theWave = theGame.AddComponent<Wave>(); //add new wave script
		theVisorRechargeScript = theVisor.GetComponent<Recharge>(); //visor recharge script
        theVisorGazeMoveScript = theVisor.GetComponent<GazeMove>(); //visor gazemove script

        //init tutorial wave
        //a single target that must be destroyed to advance
        theWave = theWave.waveWithPrefabParentLimits(target_00, theTargets, 1, 1, 0);

        //update score
        ScoreManager.Instance.resetScore();
        
        UnityEngine.Debug.Log("[GameManager] Starting the game!");

        //transition into scene via state manager
        if (StateManager.Instance != null) {
            TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
            theTransition.toggleFade();
        }

        //audio
        if (AudioManager.Instance != null) {
            AudioManager.Instance.toggleFade();
        }

	}  //end function
	
	// Update is called once per frame
	void Update () {
        //only update game if not over
        if (isGameOver == false) {
            //dim background when visor is charging
            //otherwise disable the texture and show everything
            theDimView.guiTexture.enabled = theVisorRechargeScript.isCharging;

            //update the visor position
            //get the gazemove script from the visor and call its positioning function
            theVisor.transform.position = theVisorGazeMoveScript.positionObjectWithRendererBoundsData(theVisor, theVisorFrameView.renderer, theBounds, theData);
            //UnityEngine.Debug.Log("[Game Manager] Updated Visor Pos: (" + theVisor.transform.position.x + ", " + theVisor.transform.position.y + ")");

            //check the game state and update as needed
            checkGameState();
        }

        //debug
        //in debug mode, move the visor with the mouse cursor
        if (debugOn == true) {

            //get current cursor position
            float cursorX = Input.mousePosition.x;
            float cursorY = Input.mousePosition.y;
            float cursorZ = theVisor.transform.position.z - theCam.transform.position.z;

            //convert cursor position to world position vector
            Vector3 cursorPos = new Vector3(cursorX, cursorY, cursorZ);
            Vector3 worldPos = theCam.ScreenToWorldPoint(cursorPos);

            //set visor position to mouse cursor position
            theVisor.transform.position = worldPos;

        }

	}  //end function
    
    //check the game state and update as needed
    //used to manage levels, waves, and other content experienced during gameplay
    public void checkGameState() {
        //check whether the current wave has ended
        //if so, setup a new wave
        //otherwise continue with current wave
        if (theWave.isEnded == true) {
            //fade out and destroy tutorial object
            if (theTutorial != null) {
                Destroy(theTutorial);
            }
            //increment the current level
            currentLevel++;
            Debug.Log("[GameManager] Wave starting for level: " + currentLevel);
            //1, 2, 4, 8, 10, 9, 9, 6, 1
            //set up a new wave based on the current level
            switch (currentLevel) {
                //note: level 0 tutorial already init in start function
                //level 1: introduce random destination, two targets
                case 1:
					//one wave of two slow targets that move to a random destination and stop
					theWave = theWave.waveWithPrefabParentLimits(target_01, theTargets, 2, 2, 0);
                    break;
                //level 2: introduce random destination loops, medium speed, two waves
                case 2:
                    //two waves of two medium speed targets that complete random destination loops
                    theWave = theWave.waveWithPrefabParentLimits(target_02, theTargets, 2, 4, 0);
                    break;
                //level 3: introduce destroy on completion of random loops, three waves
                case 3:
                    //three waves of two medium speed targets that complete 10 random destination loops
                    theWave = theWave.waveWithPrefabParentLimits(target_03, theTargets, 2, 8, 0);
                    break;
                //level 4: introduce origin loops, three targets
                case 4:
                    //10 medium speed targets (three at once) that loop between origin and random destination
                    theWave = theWave.waveWithPrefabParentLimits(target_04, theTargets, 3, 10, 0);
                    break;
                //level 5: introduce destroy on completion of 11 origin loops
                case 5:
					//three waves of three medium speed targets that complete 11 random destination to origin loops
                    theWave = theWave.waveWithPrefabParentLimits(target_05, theTargets, 3, 9, 0);
                    break;
				//level 6: introduce fast speed, reduce loops
                case 6:
                    //three waves of three fast targets that complete 9 random destination to origin loops
                    theWave = theWave.waveWithPrefabParentLimits(target_06, theTargets, 3, 9, 0);
                    break;
				//level 7: reduce waves, reduce loops
                case 7:
					//two waves of three fast targets that complete 7 random destination to origin loops
                    theWave = theWave.waveWithPrefabParentLimits(target_07, theTargets, 3, 6, 0);
                    break;
				//level 8: introduce most difficult speed, one shot target
                case 8:
					//an extremely fast target that makes a single random destination to origin loop
                    theWave = theWave.waveWithPrefabParentLimits(target_08, theTargets, 1, 1, 0);
                    break;
				//game complete
                case 9:
					//game complete
					Debug.Log("All waves completed - Game Over");
                    //toggle flag
                    isGameOver = true;
                    //update score
                    ScoreManager.Instance.totalObjects = 50; //50 targets in demo
                    ScoreManager.Instance.levelScore();
                    break;
                //default
                default:
                    Debug.Log("[GameManager] Error: level not found");
                    break;
            } //end switch

            //reset wave if game is not over
            if (isGameOver == false) {
                theWave.resetWave();
            }
            //end game
            else if (isGameOver == true) {
                Debug.Log("[GameManager] Game Over - End Game");
                //load level summary
                //transition to next scene
                TransitionFade theTransition = StateManager.Instance.gameObject.GetComponent<TransitionFade>();
                theTransition.toggleFade();
                StateManager.Instance.switchSceneAfterDelay("summary", theTransition.duration);
                //audio
                AudioManager theAudioManager = AudioManager.Instance.gameObject.GetComponent<AudioManager>();
                theAudioManager.toggleFade();
                AudioManager.Instance.switchBgmAfterDelay(theAudioManager.bgmSummary, theAudioManager.bgmFadeDuration);
                //show mouse cursor
                Screen.showCursor = true;
            }

        } //end if

    } //end function



} //end class
