/*
 * 
 * All content created and copyright © 2014 by John M. Quick.
 * 
*/

using UnityEngine;
using System.Collections;

//utility script for managing waves of objects
//creates a wave based on specified start/end conditions and a quantity of objects
//an external script, such as a game loop or wave manager, should handle the overall wave structure and use this utility script to generate individual waves
//assumes wave objects are added to a single container game object that contains no other objects
public class Wave : MonoBehaviour {
    //properties
    public float maxDuration; //duration after which to end wave; if 0, waits until all objects are destroyed to end wave; if all objects destroyed before duration, wave will automatically be ended
    public int maxCurrentPrefabs; //maximum number of prefabs to maintain at a given time
    public int maxTotalPrefabs; //maximum total number of prefabs to spawn
    public bool isEnded; //whether the wave has ended

    private int numCurrentPrefabs; //current number of active prefabs
    private int numTotalPrefabs; //total number of prefabs spawned
    private float startTime; //time the wave started

    //prefabs
    private Transform prefabObject; //the prefab object to be included in the wave
    private GameObject parentObject; //the parent object that prefabs will be added to
    
    //constructor
    //for creating wave with specified prefab, parent game object, max concurrent prefabs, max total prefabs, and max life duration
    public Wave waveWithPrefabParentLimits(Transform thePrefab, GameObject theParent, int theMaxCurrentPrefabs, int theMaxTotalPrefabs, float theMaxDuration) {
        //init properties
        prefabObject = thePrefab; //prefab object
        parentObject = theParent; //prefab parent object
        maxCurrentPrefabs = theMaxCurrentPrefabs; //max concurrent spawns
        maxTotalPrefabs = theMaxTotalPrefabs; //max total spawns
        maxDuration = theMaxDuration; //max life duration
        isEnded = false; //set initial flag
        numCurrentPrefabs = 0; //start with zero prefabs spawned
        numTotalPrefabs = 0; //start with zero prefabs spawned
        startTime = Time.time; //set initial start time
        
        //return
        return this;

    } //end function

    //update
    void Update () {
        //update only if not paused
        if (isEnded == false) {

            //update current duration
            float currentDuration = Time.time - startTime;
            //Debug.Log("[Wave] current duration: " + currentDuration);

            //stop spawning once max total is reached
            if (numTotalPrefabs >= maxTotalPrefabs) {
                //set number of spawns to max total
                numTotalPrefabs = maxTotalPrefabs;
                //check whether wave has ended
                //either all objects are destroyed
                if (parentObject.transform.childCount <= 0) {
                    Debug.Log("[Wave] Wave ended on all objects destroyed");
                    //end wave
                    isEnded = true;
                }
                //or the max duration (if not infinite) is exceeded
                else if (currentDuration >= maxDuration && maxDuration != 0) {
                    //destroy all prefabs in parent object
                    foreach (Transform aPrefab in parentObject.transform) {
                        //destroy
                        Destroy(aPrefab.gameObject);
                    }
                    Debug.Log("[Wave] Wave ended on max duration reached");
                    //end wave
                    isEnded = true;
                }
            }

            //spawn up to the maximum number of concurrent objects
            if (numCurrentPrefabs < maxCurrentPrefabs && numTotalPrefabs < maxTotalPrefabs) {
                //increment counter
                numTotalPrefabs++;

                //update score
                //TODO: debug - sometimes misses one target
                //ScoreManager.Instance.totalObjects++;
                //Debug.Log("[Wave] updated score, totalObjects: " + ScoreManager.Instance.totalObjects);
                
                //add a new prefab to the parent container game object
                Transform newPrefab = (Transform)Instantiate(prefabObject); //clone prefab
                newPrefab.parent = parentObject.transform; //add to container
            }
            
            //update number of current objects
            numCurrentPrefabs = parentObject.transform.childCount;

        } //end outer if
            
    } //end function

    //reset wave
    public void resetWave() {
        //properties
        numCurrentPrefabs = 0; //start with zero prefabs spawned
        numTotalPrefabs = 0; //start with zero prefabs spawned
        isEnded = false; //set initial flag
    }

} //end class
