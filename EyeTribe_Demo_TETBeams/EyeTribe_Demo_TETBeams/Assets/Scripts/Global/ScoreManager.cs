/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//manages score tracking for the game
//uses singleton instance to manage all scoring across game
public class ScoreManager : MonoBehaviour {
    //singleton properties
    private static ScoreManager _Instance; //the singleton instance of the class

    //properties
    private int _totalObjects; //total number of objects spawned in session
    private int _hitObjects; //total number of objects hit by the player
    private float _pctHit; //percentage of objects hit by the player
    private float _startTime; //time the session started
    private float _duration; //total time it took to complete the session
    private float _chargeUsed; //total charge used by player during session

    //create instance via getter
    //access ScoreManager.Instance from other classes
    public static ScoreManager Instance {
        get {
            //check for existing instance
            //if no instance
            if (_Instance == null) {
                //create game object
                GameObject ScoreManagerObj = new GameObject();
                ScoreManagerObj.name = "ScoreManager";
                _Instance = ScoreManagerObj.AddComponent<ScoreManager>();
            }

            //return the instance
            return _Instance;
        } //end get
    } //end accessor

    //awake
    void Awake() {
        //prevent this script from being destroyed when application switches scenes
        DontDestroyOnLoad(this);
    } //end function
    
	//init
	void Start () {
	    //properties
        _totalObjects = 0;
        _hitObjects = 0;
        _pctHit = 0.0f;
        _startTime = 0.0f;
        _duration = 0.0f;
        _chargeUsed = 0.0f;
	}
	
    //reset scoring variables
    //call when a new session is started
    public void resetScore() {
        Debug.Log("[ScoreManager] Score Reset");

        //properties
        _totalObjects = 0;
        _hitObjects = 0;
        _pctHit = 0.0f;
        _startTime = Time.time;
        _duration = 0.0f;
        _chargeUsed = 0.0f;
    }

    //calculate the score for the current session
    //call when a session has ended
    public void levelScore() {
        //duration
        float endTime = Time.time;
        _duration = endTime - _startTime;

        //accuracy percentage
        //ensure no division by 0
        if (_totalObjects >= 0) {
            _pctHit = Mathf.Clamp((float)_hitObjects / _totalObjects, 0.0f, 1.0f);
        }
        else {
            _pctHit = 0.0f;
        }
        /*
        Debug.Log(
            "[ScoreManager] Level stats: \n" 
            + "total objects: " + _totalObjects + " \n" 
            + "hit objects: " + _hitObjects + " \n"
            + "hit pct: " + _pctHit + " \n"
            + "charge used: " + _chargeUsed + " \n"
            + "duration: " + _duration + " \n" 
            );
        */
    }

    //getters and setters
    public int totalObjects {
        get { return _totalObjects; }
        set { _totalObjects = value; } //call when a new object is spawned
    }
    public int hitObjects {
        get { return _hitObjects; }
        set { _hitObjects = value; } //call when the player hits an object
    }
    public float pctHit {
        get { return _pctHit; }
        set { _pctHit = value; } //call when the player hits an object
    }
    public float duration {
        get { return _duration; }
        set { _duration = value; } //call when the player hits an object
    }
    public float chargeUsed {
        get { return _chargeUsed; }
        set { _chargeUsed = value; } //update as the player drains charge
    }

} //end class
