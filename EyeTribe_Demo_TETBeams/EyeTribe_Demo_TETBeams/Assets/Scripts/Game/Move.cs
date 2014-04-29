/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;
using System; //for Math

//script for managing basic movement in 2D
//attach to object in inspector
//object should have rigidbody2d and collider2d

//TODO LATER: add option to pause at destination/origin before looping
//TODO LATER: add options to set max/min distance for destination path
//TODO LATER: add options for straight line movement and curved movement
public class Move : MonoBehaviour {
    //constants
	private const float UNITS_TO_PIXELS = 100.0f; //default units to pixels conversion for rendered textures

	//properties
	public float duration; //duration in seconds the movement should last
    public bool easeSine; //sinusoidal easing
    public bool easeQuadratic; //quadratic easing
    public bool easeCubic; //cubic easing
    public bool easeQuartic; //quartic easing
    public bool easeQuintic; //quintic easing
    public bool easeExponent; //exponential easing
    public bool easeCircular; //circular easing
    public bool easeIn; //whether speed should be eased in
    public bool easeOut; //whether speed should be eased out
	public float edgeBuffer; //buffer distance for edges of screen
    public bool randSpawn; //whether object should spawn at a random position
    public bool spawnOffScreen; //whether object should spawn off screen, only relevant if spawn is randomized
    public Vector3 destPos; //destination position
    public float destPosThreshold; //threshold at which object's position is considered close enough to the specified destination
    public bool randDest; //whether object should move to random destination position
    public bool randDestLoop; //whether to loop through random destination positions once reached
    public bool originDestLoop; //whether to loop back and forth from origin to destination
    public int maxLoopSegments; //max number of loop segments to complete before stopping; 0 = inifinite, 1 = to first destination, 2 = a full return loop, etc.;
    public bool destroyOnMaxLoops; //whether to destroy the game object after the specified number of loops
    public bool fadeOnMaxLoops; //whether to fade the game object after the specified number of loops

    private Vector3 originPos; //origin position
    private float moveStartTime; //start time for movement
    private int numLoopSegments; //number of loop segments completed thus far
    private bool destroyAudioPlayed; //whether the destroy audio has already been played

    //scripts
    private Spawn theSpawn; //spawning script
    private Easing theEasing; //easing script

	//init
	void Start () {
        //properties
        //audio
        destroyAudioPlayed = false;

        //scripts
        theSpawn = gameObject.AddComponent<Spawn>(); //add new spawning script
        theEasing = gameObject.AddComponent<Easing>(); //add new easing script

        //check that rididbody2D and collider2d exist
        //check that duration is > 0 to prevent NaN
        if (gameObject.rigidbody2D != null &&
            gameObject.collider2D != null && 
            duration > 0.0f) {

            //spawn at random origin, if indicated
            //note: spawnOffScreen only affects random spawns
            if (randSpawn == true) {
                originPos = theSpawn.randSpawnWithBufferOffScreen(edgeBuffer, spawnOffScreen);
                gameObject.rigidbody2D.transform.position = originPos;
            }
            //otherwise, assume current position is intended spawn point
            else {
                originPos = getCurrentPos();
            }

            //generate random dest pos, if indicated
            if (randDest == true) {
                destPos = getRandDestPos();
            }

            //init private properties
            numLoopSegments = 0; //set initial number of loop segments completed
            moveStartTime = Time.time; //set initial move start time

        }
        //error
        else {
            Debug.Log("[Move] Error: Missing physics component(s) or invalid duration <= 0 - object cannot be moved");
            //disable scripts
            theSpawn.enabled = false;
            theEasing.enabled = false;
            this.enabled = false;
        }
	
	} //end function

	//update
	void Update() {

        //FPS
        //Debug.Log("FPS: " + (1.0f / Time.deltaTime));
		
	} //end function

    //fixed frame rate update
    //use for updating physics engine components
    void FixedUpdate() {  
        //check destination
        checkDest();
   
        //move the object
        moveObject();

    } //end function

    //check destination
    public void checkDest() {
        //get current position
        Vector3 currentPos = getCurrentPos();

        //compare current position and destination position
        //destination reached
        if (Math.Abs(currentPos.x - destPos.x) <= destPosThreshold &&
            Math.Abs(currentPos.y - destPos.y) <= destPosThreshold) {
            //Debug.Log("[Move] Destination point reached");
            //set position to exact destination
            gameObject.rigidbody2D.transform.position = destPos;
            //update current position marker
            currentPos = destPos;
            //increment number of loop segments completed
            //only increment if a non-infinite value is defined
            if (maxLoopSegments != 0) {
                numLoopSegments++;
            }

            //if set to loop between origin and destination
            //and either loop segments is below max or set to infinite
            if (originDestLoop == true && 
                (numLoopSegments < maxLoopSegments || maxLoopSegments == 0)) {
                //Debug.Log("[Move] Return to origin");
                //set previous origin as new destination
                destPos = originPos;
                //update origin to current position
                originPos = currentPos;
                //reset start time
                moveStartTime = Time.time;
            }
            //if set to loop though random destinations
            //note: if both originDestLoop and randDestLoop are selected, originDestLoop takes priority
            //note: if randDestLoop is selected, but randDest is not, object will travel to set destination first, then random destinations thereafter
            else if (randDestLoop == true && 
                (numLoopSegments < maxLoopSegments || maxLoopSegments == 0)) {
                //Debug.Log("[Move] Go to new random destination");
                //update origin to current position
                originPos = currentPos;
                //reset start time
                moveStartTime = Time.time;
                //generate new destination
                destPos = getRandDestPos();
            }
            //otherwise, final destination reached
            else {
                //Debug.Log("[Move] Final destination reached");

                //audio
                //for targets
                if (gameObject.tag == "target" && destroyAudioPlayed == false) {
                    //toggle flag
                    destroyAudioPlayed = true;
                    //play audio
                    AudioManager.Instance.playMiss(); //sfx
                }

                //destroy object if set to do so
                if (destroyOnMaxLoops == true) {
                    Debug.Log("[Move] Game Object Destroyed on Max Loops");
                    //destroy
                    Destroy(gameObject);
                }
                //fade out on max loops if set to do so
                if (fadeOnMaxLoops == true) {
                    Debug.Log("[Move] Game Object Faded on Max Loops");
                    //tell fade script to begin
                    gameObject.GetComponent<Fade>().isHoldFade = false;
                }

            } //end final destination else

        } //end outer if
        
    } //end function

    //move object
    public void moveObject() {
        float pctComplete; //percentage of movement complete
        int easeEquation = Easing.EQ_LINEAR; //easing equation, default to linear
        int easeType = Easing.TYPE_NONE; //easing type, default to none

        //check equation type
        //sinusoidal
        if (easeSine == true) {
            easeEquation = Easing.EQ_SINE;
        }
        //quadratic
        else if (easeQuadratic == true) {
            easeEquation = Easing.EQ_QUADRATIC;
        }
        //cubic
        else if (easeCubic == true) {
            easeEquation = Easing.EQ_CUBIC;
        }
        //quartic
        else if (easeQuartic == true) {
            easeEquation = Easing.EQ_QUARTIC;
        }
        //quintic
        else if (easeQuintic == true) {
            easeEquation = Easing.EQ_QUINTIC;
        }
        //exponential
        else if (easeExponent == true) {
            easeEquation = Easing.EQ_EXPONENT;
        }
        //circular
        else if (easeCircular == true) {
            easeEquation = Easing.EQ_CIRCULAR;
        }

        //check ease type(s)
        if (easeIn == true &&
            easeOut == true) {
            easeType = Easing.TYPE_IN_OUT;
        }
        else if (easeIn == true) {
            easeType = Easing.TYPE_IN;
        }
        else if (easeOut == true) {
            easeType = Easing.TYPE_OUT;
        }

        //calculate percentage complete for time-based linear interpolation with selected easing
        pctComplete = theEasing.easeWithEquationAndType(easeEquation, easeType, moveStartTime, duration);

        //moved game object using time-based linear interpolation
        gameObject.rigidbody2D.transform.position = Vector3.Lerp(originPos, destPos, pctComplete);
        
    } //end function

    //get a random destination position
    public Vector3 getRandDestPos() {
		//store new random position
		float randX;
		float randY;

		//check that object has renderer
		float halfObjW; //0.5w of object in world space
		float halfObjH; //0.5h of object in world space
		if (gameObject.renderer != null) {
			//offset by half size to ensure object stays on screen
			//in viewport space
			halfObjW = (UNITS_TO_PIXELS * gameObject.renderer.bounds.extents.x) / Screen.width;
			halfObjH = (UNITS_TO_PIXELS * gameObject.renderer.bounds.extents.y) / Screen.height;
            //Debug.Log("[Move] Object Half Size: (" + halfObjW + ", " + halfObjH + ")");
		}
		else {
			//no offset based on object size
			halfObjW = 0;
			halfObjH = 0;
			Debug.Log("[Move] Object has no renderer - randDest may not keep object on screen");
		}

		//convert set edge buffer to world units
		//allows user to set buffer in pixels from inspector, rather than world units
		//also ensures that values are positive
		float edgeBufferX = Math.Abs(edgeBuffer / Screen.width);
		float edgeBufferY = Math.Abs(edgeBuffer / Screen.height);
        //Debug.Log("[Move] Edge buffer: " + edgeBufferX);

		//generate random position values in viewport space
        randX = UnityEngine.Random.Range(halfObjW + edgeBufferX, 1.0f - halfObjW - edgeBufferX);
        randY = UnityEngine.Random.Range(halfObjH + edgeBufferY, 1.0f - halfObjH - edgeBufferY);
        //randX = 1.0f - halfObjW - edgeBufferX;
        //randY = 1.0f - halfObjH - edgeBufferY;
        //Debug.Log("[Move] Rand Dest X/Y: (" + randX + ", " + randY + ")");

		//create new random destination
		//convert from view space to world space
        Vector3 randDestView = new Vector3(randX, randY, 0);
		Vector3 randDestWorld = Camera.main.ViewportToWorldPoint(randDestView);
        randDestWorld.z = gameObject.rigidbody2D.transform.position.z; //maintain z position
        //Debug.Log("[Move] New Random Destination (" + randDestView.x + ", " + randDestView.y + ", " + randDestView.z + ")");
        //Debug.Log("[Move] New Random Destination (" + randDestWorld.x + ", " + randDestWorld.y + ", " + randDestWorld.z + ")");

        //verify that object doesn't already overlap destination
        //if so, generate a new destination on the next update
        //if collider is present and overlapping
        if (gameObject.collider2D.OverlapPoint(new Vector2(randDestWorld.x, randDestWorld.y)) == true) {
            //Debug.Log("[Move] Rand Destination Overlap - regenerate next frame");
            //reset the dest pos to the current position, so a new one can be generated next frame 
            destPos = getCurrentPos();
        }       

        //return
        return randDestWorld;
    } //end function

    //get the current position as a vector3
    public Vector3 getCurrentPos() {
        //store current position
        float currentX = gameObject.rigidbody2D.transform.position.x;
        float currentY = gameObject.rigidbody2D.transform.position.y;
        float currentZ = gameObject.rigidbody2D.transform.position.z;

        //convert to vector
        Vector3 currentPos = new Vector3(currentX, currentY, currentZ);
        //Debug.Log("[Move] Current pos: " + currentX + ", " + currentY + ", " + currentZ + ")");

        //return
        return currentPos;
    } //end function

} //end class
