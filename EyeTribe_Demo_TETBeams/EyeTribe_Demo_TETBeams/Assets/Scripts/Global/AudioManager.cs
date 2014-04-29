/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//manages all audio for application, including music and sound effects
//uses singleton instance to manage all scenes
public class AudioManager : MonoBehaviour {
    //singleton properties
    private static AudioManager _Instance; //the singleton instance of the class

    //constants
    private const string TAG_AUDIO = "audioManager"; //tag for audio manager object
    private const float SFX_VOL_MAX = 1.0f; //max volume for sfx
    private const float BGM_VOL_MAX = 0.8f; //max volume for bgm
    private const float BGM_VOL_MIN = 0.0f; //min volume for bgm
    private const float BGM_DURATION_DEFAULT = 1.0f; //default duration of 1 second

    //properties
    public float bgmFadeDuration; //how long, in seconds, the fade in/out effects should last

    private bool _bgmIsFadingIn; //whether currently fading in or out
    private bool _bgmIsHoldFade; //whether to hold the fading
    private float _bgmFadeStartTime; //start time for latest bgm fade in/out effect
    private float _bgmVolume; //the current bgm volume

    //objects
    //bgm
    public AudioClip bgmMenu; //the menu background music
    public AudioClip bgmGame; //the game background music
    public AudioClip bgmSummary; //the summary background music

    private AudioSource _bgmSource; //audio source for music
    private AudioClip _bgmNext; //the next bgm

    //sfx
    public AudioClip sfxBtnClick; //button click
    public AudioClip sfxHit; //destroy target
    public AudioClip sfxMiss; //target not destroyed by player
    public AudioClip sfxVisorDecharge; //visor is decharging
    //public AudioClip sfxVisorEmpty; //visor has lost all charge
    public AudioClip sfxVisorFull; //visor is fully charged
    public AudioClip sfxVisorRecharge; //visor is charging

    private AudioSource _sfxBtnClickSource; //audio source for sound effects
    private AudioSource _sfxHitSource; //audio source for destroy target
    private AudioSource _sfxMissSource; //audio source for target not destroyed by player
    private AudioSource _sfxVisorDechargeSource; //audio source for visor is decharging
    //private AudioSource _sfxVisorEmptySource; //audio source for visor has lost all charge
    private AudioSource _sfxVisorFullSource; //audio source for visor is fully charged
    private AudioSource _sfxVisorRechargeSource; //audio source for visor is charging

    //create instance via getter
    //access AudioManager.Instance from other classes
    public static AudioManager Instance {
        get {
            //check for existing instance
            //if no instance
            if (_Instance == null) {
                //set instance to existing game object
                if (GameObject.FindWithTag(TAG_AUDIO) != null) {
                    _Instance = GameObject.FindWithTag(TAG_AUDIO).GetComponent<AudioManager>();
                }
                //otherwise, create game object
                else {
                    //create new game object
                    GameObject AudioManagerObj = new GameObject();
                    AudioManagerObj.name = "AudioManager";

                    //create instance
                    _Instance = AudioManagerObj.AddComponent<AudioManager>();
                    
                }

                //add audio sources
                //bgm
                _Instance._bgmSource = _Instance.gameObject.AddComponent<AudioSource>(); //add new audio source
                _Instance._bgmSource.playOnAwake = false; //set play on awake
                _Instance._bgmSource.loop = true; //set looping
                _Instance._bgmSource.volume = BGM_VOL_MIN; //set volume
                _Instance._bgmSource.priority = 0; //set highest priority

                //sfx
                _Instance._sfxBtnClickSource = _Instance.gameObject.AddComponent<AudioSource>(); //add new audio source
                _Instance._sfxBtnClickSource.playOnAwake = false; //set play on awake
                _Instance._sfxBtnClickSource.loop = false; //set looping
                _Instance._sfxBtnClickSource.volume = SFX_VOL_MAX; //set volume

                _Instance._sfxHitSource = _Instance.gameObject.AddComponent<AudioSource>(); //add new audio source
                _Instance._sfxHitSource.playOnAwake = false; //set play on awake
                _Instance._sfxHitSource.loop = false; //set looping
                _Instance._sfxHitSource.volume = SFX_VOL_MAX; //set volume

                _Instance._sfxMissSource = _Instance.gameObject.AddComponent<AudioSource>(); //add new audio source
                _Instance._sfxMissSource.playOnAwake = false; //set play on awake
                _Instance._sfxMissSource.loop = false; //set looping
                _Instance._sfxMissSource.volume = SFX_VOL_MAX; //set volume

                _Instance._sfxVisorDechargeSource = _Instance.gameObject.AddComponent<AudioSource>(); //add new audio source
                _Instance._sfxVisorDechargeSource.playOnAwake = false; //set play on awake
                _Instance._sfxVisorDechargeSource.loop = true; //set looping
                _Instance._sfxVisorDechargeSource.volume = SFX_VOL_MAX; //set volume
                /*
                _Instance._sfxVisorEmptySource = _Instance.gameObject.AddComponent<AudioSource>(); //add new audio source
                _Instance._sfxVisorEmptySource.playOnAwake = false; //set play on awake
                _Instance._sfxVisorEmptySource.loop = false; //set looping
                _Instance._sfxVisorEmptySource.volume = SFX_VOL_MAX; //set volume
                */
                _Instance._sfxVisorFullSource = _Instance.gameObject.AddComponent<AudioSource>(); //add new audio source
                _Instance._sfxVisorFullSource.playOnAwake = false; //set play on awake
                _Instance._sfxVisorFullSource.loop = false; //set looping
                _Instance._sfxVisorFullSource.volume = SFX_VOL_MAX; //set volume

                _Instance._sfxVisorRechargeSource = _Instance.gameObject.AddComponent<AudioSource>(); //add new audio source
                _Instance._sfxVisorRechargeSource.playOnAwake = false; //set play on awake
                _Instance._sfxVisorRechargeSource.loop = true; //set looping
                _Instance._sfxVisorRechargeSource.volume = SFX_VOL_MAX; //set volume

            } //end outer if

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
    public void Start() {
        //ensure audio clips are defined in inspector
        //error
        if (bgmMenu == null || bgmGame == null || bgmSummary == null || 
            sfxBtnClick == null || sfxHit == null || sfxMiss == null || 
            sfxVisorDecharge == null || /*sfxVisorEmpty == null ||*/ 
            sfxVisorFull == null || sfxVisorRecharge == null) {
            Debug.Log("[AudioManager] Error: Missing audio clips - define in inspector");
            //disable script
            this.enabled = false;
        }
        //proceed
        else {
            //objects
            //add sfx audio clips to respective sources
            _Instance._sfxBtnClickSource.clip = sfxBtnClick;
            _Instance._sfxHitSource.clip = sfxHit;
            _Instance._sfxMissSource.clip = sfxMiss;
            _Instance._sfxVisorDechargeSource.clip = sfxVisorDecharge;
            //_Instance._sfxVisorEmptySource.clip = sfxVisorEmpty;
            _Instance._sfxVisorFullSource.clip = sfxVisorFull;
            _Instance._sfxVisorRechargeSource.clip = sfxVisorRecharge;

            //properties
            //ensure a valid duration value is used
            if (bgmFadeDuration <= 0.0f) {
                bgmFadeDuration = BGM_DURATION_DEFAULT;
                Debug.Log("[AudioManager] Invalid bgm fade duration provided - set to default of 1 second instead");
            }

            //assume min volume
            _bgmVolume = BGM_VOL_MIN;

            //establish the start time
            _bgmFadeStartTime = Time.time;
        }

    } //end function

    //update
    void Update() {
        //only update volume if not on hold
        if (_bgmIsHoldFade == false) {
            //fade in
            if (_bgmIsFadingIn == true) {
                bgmFadeIn();
            }

            //fade out
            if (_bgmIsFadingIn == false) {
                bgmFadeOut();
            }

            //update playback volume
            _bgmSource.volume = _bgmVolume;

        } //end outer if

    } //end function

    //fade in the volume
    private void bgmFadeIn() {
        //calculate the time completed thus far
        float cumulativeTime = Time.time - _bgmFadeStartTime; //cumulative time completed
        float pctTime = Mathf.Clamp(cumulativeTime / bgmFadeDuration, 0.0f, 1.0f); //percentage time completed

        //volume is less than max
        if (_bgmVolume < BGM_VOL_MAX) {
            //update
            _bgmVolume = pctTime;
        }
        //volume has reached max
        else {
            //set to max
            _bgmVolume = BGM_VOL_MAX;
            //hold
            _bgmIsHoldFade = true;
        }

    } //end function

    //fade out the volume
    private void bgmFadeOut() {
        //calculate the time completed thus far
        float cumulativeTime = Time.time - _bgmFadeStartTime; //cumulative time completed
        float pctTime = 1.0f - Mathf.Clamp(cumulativeTime / bgmFadeDuration, 0.0f, 1.0f); //percentage time completed

        //volume is greater than min
        if (_bgmVolume > BGM_VOL_MIN) {
            //update
            _bgmVolume = pctTime;
        }
        //volume has reached min
        else {
            //set to min
            _bgmVolume = BGM_VOL_MIN;
            //hold
            _bgmIsHoldFade = true;
        }

    } //end function

    //toggle fade
    //call before triggering transition
    public void toggleFade() {
        //switch fade effect to prepare for next segment
        _bgmIsFadingIn = !_bgmIsFadingIn;
        //turn off hold
        _bgmIsHoldFade = false;
        //reset time
        _bgmFadeStartTime = Time.time;

    } //end function

    //switch bgm after delay
    public void switchBgmAfterDelay(AudioClip theBgm, float theDelay) {
        //set the next bgm
        _bgmNext = theBgm;

        //invoke the switch after the given delay
        //used to allow transition to occur before switch
        Invoke("switchBgm", theDelay);
    }

    //switch bgm
    private void switchBgm() {
        _bgmSource.Stop(); //stop playback
        _bgmSource.clip = _bgmNext; //switch bgm
        _bgmSource.Play(); //start playback
    } //end function

    //sfx playback functions
    public void playBtnClick() {
        _sfxBtnClickSource.Play();
    }
    public void playHit() {
        _sfxHitSource.Play();
    }
    public void playMiss() {
        _sfxMissSource.Play();
    }
    public void playVisorDecharge() {
        stopVisorRecharge();
        _sfxVisorDechargeSource.Play();
    }
    public void stopVisorDecharge() {
        _sfxVisorDechargeSource.Stop();
    }
    /*
    public void playVisorEmpty() {
        stopVisorDecharge();
        _sfxVisorEmptySource.Play();
    }
    */
    public void playVisorFull() {
        stopVisorRecharge();
        _sfxVisorFullSource.Play();
    }
    public void playVisorRecharge() {
        stopVisorDecharge();
        _sfxVisorRechargeSource.Play();
    }
    public void stopVisorRecharge() {
        _sfxVisorRechargeSource.Stop();
    }
    public void stopAllSfx() {
        _sfxBtnClickSource.Stop();
        _sfxHitSource.Stop();
        _sfxMissSource.Stop();
        _sfxVisorDechargeSource.Stop();
        //_sfxVisorEmptySource.Stop();
        _sfxVisorFullSource.Stop();
        _sfxVisorRechargeSource.Stop();
    }

    //setters and getters
    public AudioSource bgmSource {
        get { return _bgmSource; }
        set { _bgmSource = value; }
    }
    public AudioClip bgmNext {
        get { return _bgmNext; }
        set { _bgmNext = value; }
    }
    public bool bgmIsFadingIn {
        get { return _bgmIsFadingIn; }
        set { _bgmIsFadingIn = value; }
    }
    public bool bgmIsHoldFade {
        get { return _bgmIsHoldFade; }
        set { _bgmIsHoldFade = value; }
    }
    
} //end class
