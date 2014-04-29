/*
 * All content created and copyright © 2014 by John M. Quick, except where noted below.

 * Note: The mathematical easing formulas used in this class, for instance, 
 * -0.5f * ((float)Math.Cos(pctTime * Math.PI) - 1.0f), were lawfully
 * adapted from the following source.

 * Source:
 * Copyright © 2001 Robert Penner
 * All rights reserved.
 * Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
 * Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
 * Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
 * Neither the name of the author nor the names of contributors may be used to endorse or promote products derived from this software without specific prior written permission.
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/

using UnityEngine;
using System.Collections;
using System; //for Math

//utility script for managing easing for moving objects
//assumes linear interpolation is being used
//therefore, returns a percentage complete based on the selected easing type and equation
public class Easing : MonoBehaviour {
    //constants
    public const int TYPE_NONE = 0; //no easing
    public const int TYPE_IN_OUT = 1; //ease in and out
    public const int TYPE_IN = 2; //ease in only
    public const int TYPE_OUT = 3; //ease out only
    public const int EQ_LINEAR = 0; //linear equation
    public const int EQ_SINE = 1; //sinusoidal easing
    public const int EQ_EXPONENT = 2; //exponential easing
    public const int EQ_CIRCULAR = 3; //circular easing
    public const int EQ_QUADRATIC = 4; //quadratic easing
    public const int EQ_CUBIC = 5; //cubic easing
    public const int EQ_QUARTIC = 6; //quartic easing
    public const int EQ_QUINTIC = 7; //quintic equation  

    //return time-based linear interpolation percentage according to given easing method and type, and the percentage of time complete thus far
    //note: in equations, 0.0f = minimum value, 1.0f = maximum value, 0.5f = half maximum value, 2.0f = doubling value
    public float easeWithEquationAndType(int theEquation, int theType, float theStartTime, float theDuration) {
        //set up return variable
        float cumulativeTime = Time.time - theStartTime; //time completed thus far
        float pctTime = Mathf.Clamp(cumulativeTime / theDuration, 0.0f, 1.0f); ; //percentage time completed thus far
        float pctComplete = 0; //percentage completed with easing applied to be returned to linear interpolation function
        
        //check equation
        switch (theEquation) {

            //sine
            case EQ_SINE:
            //check type
            //ease in and out
            if (theType == TYPE_IN_OUT) {
                pctComplete = -0.5f * ((float)Math.Cos(pctTime * Math.PI) - 1.0f);
            }
            //ease in only
            else if (theType == TYPE_IN) {
                pctComplete = 1.0f + (-1.0f * (float)Math.Cos(pctTime * (Math.PI / 2)));
            }
            //ease out only
            else if (theType == TYPE_OUT) {
                pctComplete = 1.0f * (float)Math.Sin(pctTime * (Math.PI / 2));
            }
            break;

            //quadratic
            case EQ_QUADRATIC:
            //check type
            //ease in and out
            if (theType == TYPE_IN_OUT) {
                if (pctTime < 0.5f) {
                    pctComplete = 0.5f * (float)Math.Pow(pctTime * 2.0f, 2);
                }
                else {
                    pctComplete = -0.5f * ((pctTime * 2.0f - 1.0f) * (pctTime * 2.0f - 3.0f) - 1.0f);
                }
            }
            //ease in only
            else if (theType == TYPE_IN) {
                pctComplete = 1.0f * (float)Math.Pow(pctTime, 2);
            }
            //ease out only
            else if (theType == TYPE_OUT) {
                pctComplete = -1.0f * pctTime * (pctTime - 2.0f);
            }
            break;

            //cubic
            case EQ_CUBIC:
            //check type
            //ease in and out
            if (theType == TYPE_IN_OUT) {
                if (pctTime < 0.5f) {
                    pctComplete = 0.5f * (float)Math.Pow(pctTime * 2.0f, 3);
                }
                else {
                    pctComplete = 0.5f * (2.0f + (float)Math.Pow(pctTime * 2.0f - 2.0f, 3));
                }
            }
            //ease in only
            else if (theType == TYPE_IN) {
                pctComplete = 1.0f * (float)Math.Pow(pctTime, 3);
            }
            //ease out only
            else if (theType == TYPE_OUT) {
                pctComplete = 1.0f * (1.0f + (float)Math.Pow(pctTime - 1.0f, 3));
            }
            break;

            //quartic
            case EQ_QUARTIC:
            //check type
            //ease in and out
            if (theType == TYPE_IN_OUT) {
                if (pctTime < 0.5f) {
                    pctComplete = 0.5f * (float)Math.Pow(pctTime * 2.0f, 4);
                }
                else {
                    pctComplete = -0.5f * ((float)Math.Pow(pctTime * 2.0f - 2.0f, 4) - 2.0f);
                }
            }
            //ease in only
            else if (theType == TYPE_IN) {
                pctComplete = 1.0f * (float)Math.Pow(pctTime, 4);
            }
            //ease out only
            else if (theType == TYPE_OUT) {
                pctComplete = -1.0f * ((float)Math.Pow(pctTime - 1.0f, 4) - 1.0f);
            }
            break;
            
            //quintic
            case EQ_QUINTIC:
                //check type
                //ease in and out
                if (theType == TYPE_IN_OUT) {
                    //ease in for first half
                    if (pctTime < 0.5f) { //accelerate for first half of movement only
                        pctComplete = 0.5f * (float)Math.Pow(pctTime * 2.0f, 5); //multiply by 0.5 since it applies to only the first half of the movement; multiply pctTime by 2 to double movement speed over half the time
                    } 
                    //ease out for second half
                    else { //decelerate for second half of movement
                        pctComplete = 0.5f * (2.0f + (float)Math.Pow(pctTime * 2.0f - 2.0f, 5)); //multiply by 0.5 since it applies to only the second half of movement; multiply pctTime by 2 to double movement speed over half the time
                    }
                }
                //ease in only
                else if (theType == TYPE_IN) {
                    pctComplete = 1.0f * (float)Math.Pow(pctTime, 5);
                }
                //ease out only
                else if (theType == TYPE_OUT) {
                    pctComplete = 1.0f * (1.0f + (float)Math.Pow(pctTime - 1.0f, 5));
                }
            break;

            //exponential
            case EQ_EXPONENT:
            //check type
            //ease in and out
            if (theType == TYPE_IN_OUT) {
                if (pctTime == 0.0f) {
                    pctComplete = 0.0f;
                }
                else if (pctTime == 1.0f) {
                    pctComplete = 1.0f;
                }
                else if (pctTime < 0.5f) {
                    pctComplete = 0.5f * (float)Math.Pow(2.0f, 10.0f * (pctTime * 2.0f - 1.0f));
                }
                else {
                    pctComplete = 0.5f * (2.0f + -1.0f * (float)Math.Pow(2, -10.0f * (pctTime * 2.0f - 1.0f)));
                }
            }
            //ease in only
            else if (theType == TYPE_IN) {
                pctComplete = 1.0f * (float)Math.Pow(2.0f, 10.0f * (pctTime - 1.0f));
            }
            //ease out only
            else if (theType == TYPE_OUT) {
                //since exponential value will never exactly equal 1, set it once it is close enough
                if (pctTime == 1.0f) {
                    pctComplete = 1.0f;
                }
                //otherwise use exponential value
                else {
                    pctComplete = 1.0f * (1.0f + (-1.0f * (float)Math.Pow(2.0f, -10.0f * pctTime)));
                }
            }
            break;

            //circular
            case EQ_CIRCULAR:
            //check type
            //ease in and out
            if (theType == TYPE_IN_OUT) {
                if (pctTime < 0.5f) {
                    pctComplete = -0.5f * ((float)Math.Sqrt(1.0f - Math.Pow(pctTime * 2.0f, 2)) - 1.0f);
                }
                else {
                    pctComplete = 0.5f * ((float)Math.Sqrt(1.0f - Math.Pow(pctTime * 2.0f - 2.0f, 2)) + 1.0f);
                }
            }
            //ease in only
            else if (theType == TYPE_IN) {
                pctComplete = -1.0f * ((float)Math.Sqrt(1.0f - Math.Pow(pctTime, 2)) - 1.0f);
            }
            //ease out only
            else if (theType == TYPE_OUT) {
                pctComplete = 1.0f * (float)Math.Sqrt(1.0f - Math.Pow(pctTime - 1.0f, 2));
            }
            break;

            //linear (default)
            case EQ_LINEAR:
            default:
                pctComplete = pctTime;
                //Debug.Log("[Easing] Linear easing used by default");
            break;

        } //end switch

        //Debug.Log("[Easing] Cumulative time: " + cumulativeTime);
        //Debug.Log("[Easing] Pct time: " + pctTime);
        //Debug.Log("[Easing] Pct complete: " + pctComplete);

        //return 
        return pctComplete;
    }

} //end class