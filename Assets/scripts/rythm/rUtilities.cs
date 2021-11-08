using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class rUtilities
{

    // ------------------------- // // ------------------------- // // ------------------------- // 
    
    #region transform functions
    // ------------------------- // // ------------------------- //

    #region disc functions
    // ------------------------- // 

    public static Vector2 rotateVec(this Vector2 v, float angle) 
    {

        angle = Mathf.Abs(angle);
        angle = Mathf.Abs(angle) > 360 ? (angle % 360) : angle;
        angle *= Mathf.Deg2Rad;

        float cA = Mathf.Cos(angle);
        //Debug.Log("cosin of " + angle + " is: " + cA);
        float sA = Mathf.Sin(angle);
        //Debug.Log("sin of " + angle + " is: " + sA);

        Vector2 rot = new Vector2(  (cA * v.x) - (sA * v.y),
                                    (sA * v.x) + (cA * v.y) );

        //Debug.Log(v + " rotated by " + angle + " is: " + rot);

        return rot;
    }
    public static Vector2[] getDisc(this Vector2[] array, float dist) 
    {

        if (array.Length <= 1)
            return array;
        float angle = 360f / array.Length;
        Vector2 st = new Vector2(0f, 1f);

        for (int i = 0; i < array.Length; i++) 
        {
            array[i] = st.rotateVec(angle * i).normalized * dist;
        }

        return array;

    }
    
    // ------------------------- // 

    public static void applyDisc(this Transform[] array)
    {

        Vector2 st = new Vector2(0f, 1f);
        float angle = 360f / array.Length;
        float dist = 0f;

        if (array.Length == 1)
        {
            dist = array[0].localPosition.magnitude;
            array[0].localPosition = st * dist;
            return;
        }
        else if (array.Length == 0)
            return;

        for (int i = 0; i < array.Length; i++)
        {
            dist = array[i].localPosition.magnitude;
            array[i].localPosition = st.rotateVec(angle * i).normalized * dist;
        }

    }
    public static void applyDisc(this Transform[] array, float dist) 
    {

        Vector2 st = new Vector2(0f, 1f).normalized;
        float angle = 360f / array.Length;
        //Debug.Log("angle of disc: " + angle);

        if (array.Length == 1)
        {
            array[0].localPosition = st * dist;
            Debug.Log("only one disc at angle of: " + angle);
            return;
        }
        else if (array.Length == 0)
            return;     

        for (int i = 0; i < array.Length; i++)
        {
            array[i].localPosition = st.rotateVec(angle * i).normalized * dist;
            //Debug.Log("angle of disc " + array[i].name + ": " + (angle * i));
        }

    }
    public static void applyDisc(this Transform[] array, float dist, Vector2 startPos)
    {

        Vector2 st = new Vector2(0f, 1f);
        float angle = 360f / array.Length;

        if (array.Length == 1)
        {
            array[0].localPosition = st * dist;
            return;
        }
        else if (array.Length == 0)
            return;

        for (int i = 0; i < array.Length; i++)
        {
            array[i].localPosition = st.rotateVec(angle * i).normalized * dist + startPos;
        }

    }
    
    // ------------------------- // 

    /// <summary>
    /// use it in a while loop controlled by cTime <= duration
    /// </summary>
    /// <param name="angle"> final angle of the rotation.</param>
    /// <param name="duration"> duration of rotation in seconds.</param>
    /// <param name="cTime"> current time of the rotation, input the same value as duration for instant rotation.</param>
    /// <returns>Returns the current time after the transformation.</returns>
    public static float rotateDisc(this Transform[] array, float angle, float duration, float cTime) 
    {

        if (Time.fixedDeltaTime > duration)
        {
            Debug.LogError("TIME INCREASE BIGGER THAN DURATION");
            return 0f;
        }
        if(cTime == 0)
            cTime = Time.fixedDeltaTime;
        angle = angle / (duration/Time.fixedDeltaTime) ;
        //array.applyDisc();
        foreach (Transform vec in array) 
        {
            Vector2 rotVec = new Vector2(vec.localPosition.x, vec.localPosition.y);
            rotVec = rotVec.rotateVec( angle );
            vec.localPosition = new Vector3( rotVec.x, rotVec.y , vec.localPosition.z);
        }

        return cTime + Time.fixedDeltaTime;

    }
    /// <summary>
    /// use it in a while loop controlled by cTime <= duration
    /// </summary>
    /// <param name="angle"> final angle of the rotation.</param>
    /// <param name="duration"> duration of rotation in seconds.</param>
    /// <param name="cTime"> current time of the rotation, input the same value as duration for instant rotation.</param>
    /// <param name="timeIncrease"> the amount of time to increase each iteration</param>
    /// <returns>Returns the current time after the transformation.</returns>
    public static float rotateDisc(this Transform[] array, float angle, float duration, float cTime, float timeIncrease)
    {

        if (timeIncrease > duration)
        {
            Debug.LogError("TIME INCREASE BIGGER THAN DURATION");
            return 0f;
        }

        if (cTime == 0)
            cTime = timeIncrease;

        angle = angle / (duration / timeIncrease);
        //array.applyDisc();
        foreach (Transform vec in array)
        {
            Vector2 rotVec = new Vector2(vec.localPosition.x, vec.localPosition.y);
            rotVec = rotVec.rotateVec(angle);
            vec.localPosition = new Vector3(rotVec.x, rotVec.y, vec.localPosition.z);
        }

        return cTime + timeIncrease;

    }
    public static void rotateDisc(this Transform[] array, float angle, int steps)
    {

        angle = angle / steps;
        //array.applyDisc();
        foreach (Transform vec in array)
        {
            Vector2 rotVec = new Vector2(vec.localPosition.x, vec.localPosition.y);
            rotVec = rotVec.rotateVec(angle);
            vec.localPosition = new Vector3(rotVec.x, rotVec.y, vec.localPosition.z);
        }

    }

    // ------------------------- // 

    /// <summary>
    /// use it in an eigthBeat by rRythmManager ONLY
    /// </summary>
    /// <param name="endAngle"> final angle of the rotation.</param>
    /// <param name="cStep"> current eight beat in the beat cycle.</param>
    /// <param name="calcAngles"> MUST set up to null on ONLY the first iteration</param>
    /// <returns> Returns the calculations for the lerp so it does not calculate every frame \n use the returned value on the calc angles param of next iteration.</returns>
    public static Vector2[][] lerpDiscRotation(this Transform[] array, float endAngle, int cStep, Vector2[][] calcAngles)
    {

        int steps = 8;
        //this is because it is for the eigth beat and readability
        //so whenever this shows up just know it is bc this is the amount of iterations this code is suposed to run
        //until completion, sorry for the mess ç-ç
        //cStep--; //uncomment if you use the same val as movement
        //this is because my "engine" works with it starting on one, don't remember why tho
        //check:
        //protected virtual void staticEigthMoveTo(Vector3 start, Vector3 end, int theStep)
        //on rEntity.cs

        if (cStep > steps)
        {
            Debug.LogError("CURRENT STEP HIGHER THAN THE AMOUNT OF STEPS");
            return null;
        }

        float angle = endAngle / steps;
        Vector2[][] positions;

        // if you don't have the calculation it does for you
        if (calcAngles == null) 
        {
            positions = new Vector2[array.Length][];
            for (int obj = 0; obj < array.Length; obj++) 
            {
                positions[obj] = new Vector2[steps];
                for (int i = 0; i < steps; i++)
                {
                    if (i > 0)
                        positions[obj][i] = positions[obj][i - 1].rotateVec(angle);
                    else
                        positions[obj][i] = new Vector2(array[i].localPosition.x, array[i].localPosition.y).rotateVec(angle);
                }
            }
        }
        else
            positions = calcAngles;
        //if you have done it already it just passes information forward

        for (int obj = 0; obj < array.Length; obj++) 
        {
            array[obj].localPosition = new Vector3(
                positions[obj][cStep].x,
                positions[obj][cStep].y, 
                array[obj].localPosition.z);
        }

        if (cStep == steps)
            positions = null;
        //as it is done it returns null to make life easier
        //so next base beat you don't have to run a setup to set it to null and stuff
        //which can get complicated as there are the "save" mechanics on the first couple of eigths

        return positions;

    }

    // ------------------------- //
    #endregion

    public static void snapToGrid(this Transform transform, bool pointFive)
    {

        float pX = transform.position.x;
        float pY = transform.position.y;

        if (!pointFive)
        {
            transform.position = new Vector3(Mathf.Round(pX), Mathf.Round(pY), transform.position.z);
            return;
        }
        else 
        {
            transform.position = new Vector3(Mathf.Round(pX * 2f) / 2f, Mathf.Round(pY * 2f) / 2f, transform.position.z);
            return;
        }

    }

    // ------------------------- // // ------------------------- //
    #endregion

    // ------------------------- // // ------------------------- // // ------------------------- // 

    #region audio source functions
    // ------------------------- // // ------------------------- //

    public static void playUnDeaf(this AudioSource source) 
    {
        if (source.isPlaying)
            return;
        source.Play();
    }
    public static void playPriority(this AudioSource source)
    {
        if (source.isPlaying)
            source.Stop();
        source.Play();
    }
    public static void copyInfo(this AudioSource source, AudioSource toCopy) 
    {

        source.bypassEffects = toCopy.bypassEffects;
        source.bypassListenerEffects = toCopy.bypassListenerEffects;
        source.bypassReverbZones = toCopy.bypassReverbZones;
        source.clip = toCopy.clip;
        source.dopplerLevel = toCopy.dopplerLevel;
        source.hideFlags = toCopy.hideFlags;
        source.ignoreListenerPause = toCopy.ignoreListenerPause;
        source.ignoreListenerVolume = toCopy.ignoreListenerVolume;
        source.loop = toCopy.loop;
        source.maxDistance = toCopy.maxDistance;
        source.minDistance = toCopy.minDistance;
        source.outputAudioMixerGroup = toCopy.outputAudioMixerGroup;
        source.panStereo = toCopy.panStereo;
        source.pitch = toCopy.pitch;
        source.priority = toCopy.priority;
        source.reverbZoneMix = toCopy.reverbZoneMix;
        source.rolloffMode = toCopy.rolloffMode;
        source.spatialBlend = toCopy.spatialBlend;
        source.spatialize = toCopy.spatialize;
        source.spatializePostEffects = toCopy.spatializePostEffects;
        source.spread = toCopy.spread;
        source.velocityUpdateMode = toCopy.velocityUpdateMode;
        source.volume = toCopy.volume;

    }

    // ------------------------- // // ------------------------- //
    #endregion

    // ------------------------- // // ------------------------- // // ------------------------- // 

    #region math and logic functions
    // ------------------------- // // ------------------------- //

    public static float frac(this float val)
    {
        float floored = Mathf.Floor(val);
        return (floored - val);
    }

    public static int marchDist<t>(this t[] array, int start, int end,bool increasing) 
    {

        if (end > array.Length || start > array.Length) 
        {
            Debug.LogError(" values out of array length");
            return 0;
        }

        int steps = 0;
        int current = start;
        for (int i = 0; i < array.Length; i++) 
        {
            if (current == end)
                break;
            current = increasing ? current + 1 : current - 1;
            current = current > array.Length ? 0 :
                      current < 0 ? array.Length : current;
            steps++;
        }

        return steps;


    }

    // ------------------------- // // ------------------------- //
    #endregion

    // ------------------------- // // ------------------------- // // ------------------------- // 

    #region weirdly specific stuff
    // ------------------------- // // ------------------------- //

    public static Vector3 movementToVec3(this rEntity.movement theMove)
    {

        Vector3 ret = Vector3.zero;
        switch (theMove)
        {

            case rEntity.movement.attack:
                ret = Vector3.zero;
                break;

            case rEntity.movement.nX:
                ret = Vector3.left;
                break;

            case rEntity.movement.pX:
                ret = Vector3.right;
                break;

            case rEntity.movement.nY:
                ret = Vector3.down;
                break;

            case rEntity.movement.pY:
                ret = Vector3.up;
                break;

            case rEntity.movement.stop:
                ret = Vector3.zero;
                break;

        }

        return ret;

    }

    // ------------------------- // // ------------------------- //
    #endregion

    // ------------------------- // // ------------------------- // // ------------------------- // 

}


