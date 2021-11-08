using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class rEnemies : MonoBehaviour
{

    //references
    protected SpriteRenderer myRenderer;
    public GameObject Attacker;
    public Transform[] mySpheres;
    public Color[] MyColors;
    public Color currentColor;
    public int StartingColor;    

    //constants
    [Space(25)]
    public float rotDuration = 2f;
    public float walkDist;
    public float minDist = 2f;

    //beats related
    protected float lastmove = 0f;
    protected int beat = 0;
    protected int rStep = 0;
    protected int mStep = 0;

    //information between frames
    protected movement myMovement;
    protected Vector3 beatPos;
    protected bool mClock;
    

    #region setup
    // ------------------------- // // ------------------------- //
    protected virtual void OnEnable()
    {
        if (mySpheres == null || mySpheres.Length == 0) 
        {
            mySpheres = GetComponentsInChildren<Transform>();
            Transform[] hold = new Transform[mySpheres.Length - 1];
            for (int i = 1; i < mySpheres.Length; i++)
            {
                hold[i - 1] = mySpheres[i];
            }
            mySpheres = hold;
        }
        mySpheres.applyDisc(.5f);
        checkValidInputs();
        SetupColors();
        Debug.Log("adding beats at " + transform.name);
        rRythmManager.instance.beat.AddListener(Beat);
        rRythmManager.instance.halfBeat.AddListener(halfBeat);
        rRythmManager.instance.fourthBeat.AddListener(fourthBeat);
        rRythmManager.instance.eigthBeat.AddListener(eightBeat);
        Debug.Log("added beats at " + transform.name);

    }
    protected virtual void OnDisable() 
    {
        Debug.Log("removing beats at " + transform.name);
        rRythmManager.instance.beat.RemoveListener(Beat);
        rRythmManager.instance.halfBeat.RemoveListener(halfBeat);
        rRythmManager.instance.fourthBeat.RemoveListener(fourthBeat);
        rRythmManager.instance.eigthBeat.RemoveListener(eightBeat);
        Debug.Log("removed beats at " + transform.name);
    }
    // ------------------------- // // ------------------------- //
    #endregion

    #region beats
    // ------------------------- // // ------------------------- //
    protected virtual void Beat()
    {
        
    }
    protected virtual void halfBeat()
    {

    }
    protected virtual void fourthBeat()
    {

    }
    protected virtual void eightBeat() 
    {
        
        //Debug.Log("eigth beat");
    }
    // ------------------------- // // ------------------------- //
    #endregion

    #region debug related
    // ------------------------- // // ------------------------- //
    protected virtual void checkValidInputs() 
    {
        rotDuration = rotDuration <= 0 ? 1 : rotDuration;
        walkDist = walkDist <= 0 ? 1 : walkDist;
        minDist = minDist <= 0 ? 1 : minDist;
        for (int i = 0; i < MyColors.Length; i++)
        {
            MyColors[i] = MyColors[i] == null ? Color.magenta : MyColors[i];
        }
    }
    protected void snapToGrid() 
    {
        Vector3 pos = transform.position;
        float fracX = rUtilities.frac(pos.x);
        float fracY = rUtilities.frac(pos.y);
        float floorX = Mathf.Floor(pos.x);
        float floorY = Mathf.Floor(pos.y);

        if (fracX != 0.5f)
        {
            pos.x = fracX > 0.5f ? floorX + .5f : floorX - .5f;
        }
        if (fracY != 0.5f)
        {
            pos.y = fracY > 0.5f ? floorY + .5f : floorY - .5f;
        }

        transform.position = pos;

    }
    // ------------------------- // // ------------------------- //
    #endregion

    #region color changing related
    // ------------------------- // // ------------------------- //
    protected virtual void eigthRotateMarkerOneTwo() 
    {

        if(rStep >= 0)
            mySpheres.rotateDisc(360f / mySpheres.Length, 8);
        rStep++;

        if (rStep == 8)
        {
            rStep = -16;
            ChangeColor();
        }        

    }
    protected virtual void eigthRotateMarkerOneOne()
    {

        if (rStep >= 0)
            mySpheres.rotateDisc(360f / mySpheres.Length, 8);
        rStep++;

        if (rStep == 8)
        {
            rStep = -8;
            ChangeColor();
        }

    }
    protected void ChangeColor()
    {

        float highest = 0f;
        int iterator = 0;
        for (int i = 0; i < mySpheres.Length; i++)
        {
            float compared = mySpheres[i].localPosition.y;
            if (compared > highest)
            {
                iterator = i;
                highest = compared;
            }
        }
        currentColor = MyColors[iterator];
        myRenderer.color = currentColor;

    }
    protected void SetupColors() 
    {

        myRenderer = GetComponent<SpriteRenderer>();
        MyColors = new Color[mySpheres.Length];
        Color[] managerCols = rRythmManager.instance.GetColors();
        int withOffset = StartingColor;

        if (mySpheres.Length > managerCols.Length)
        {
            Debug.LogError((mySpheres.Length - managerCols.Length) + " TOO MANY SPHERES AT " + transform.name);
            return;
        }
        else if (StartingColor > managerCols.Length) 
        {
            Debug.LogError("STARTING COLOR INDEX TOO BIG, CHANGING IT TO 0 AT " + transform.name);
            StartingColor = 0;
        }

        for (int i = 0; i < mySpheres.Length; i++) 
        {
            withOffset = StartingColor + i;
            if (withOffset >= mySpheres.Length)
                withOffset = mySpheres.Length - withOffset;
            MyColors[i] = managerCols[withOffset];
            mySpheres[i].GetComponent<SpriteRenderer>().color = MyColors[i];
        }
        currentColor = managerCols[StartingColor];
        
    }
    // ------------------------- // // ------------------------- //
    #endregion

    #region movement related
    // ------------------------- // // ------------------------- //
    protected virtual int eigthMoveTo(Vector3 start, Vector3 end, int theStep) 
    {

        if (theStep > 8)
            return theStep;
        theStep = theStep == 0 ? 1 : theStep;        

        float interpolator = (float)theStep / 8f;
        transform.position = Vector3.Lerp(start, end, interpolator);

        //Debug.Log("moving " + transform.name + " at step " + theStep + " with " + (Time.time - lastmove) + " interval");
        lastmove = Time.time;

        return theStep + 1;

    }
    protected virtual int eigthMovementTo(Vector3 startPos, movement dir, int theStep) 
    {

        Vector3 destination = transform.position;
        switch (dir)
        {
            case movement.stop:
                return 0;

            case movement.pX:
                destination = startPos + (Vector3.right * walkDist);
                break;

            case movement.nX:
                destination = startPos + (Vector3.right * -walkDist);
                break;

            case movement.pY:
                destination = startPos + (Vector3.up * walkDist);
                break;

            case movement.nY:
                destination = startPos + (Vector3.up * -walkDist);
                break;
            
            case movement.attack:
                destination = transform.position + (
                    (rPlayer.Instance.transform.position - transform.position).normalized 
                    * walkDist);
                break;

            default:
                return 0;

        }

        if (transform.position == destination)
            return 0;

        if (dir != movement.stop && dir != movement.attack)
        {
            theStep = eigthMoveTo(startPos, destination, theStep);
            Attacker.SetActive(false);
        }
        else if (dir == movement.attack && Attacker != null)
        {

            if (theStep <= 2)
            {

                Attacker.transform.position = destination;
                Attacker.SetActive(true);

            }
            else
                Attacker.SetActive(false);

            theStep += 1;

        }
        else if (dir == movement.attack)
        {
            Debug.LogWarning(transform.name + " WANTED TO ATTACK BUT THERE WAS NO ATTACKER");
        }

        return theStep;

    }
    protected movement decideMove() 
    {

        Vector3 dir = rPlayer.Instance.transform.position - transform.position;
        Vector3 aDir = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), dir.z);
        movement decision = movement.stop;
        if (aDir.x > aDir.y)
        {
            decision = dir.x > 0 ? movement.pX : movement.nX;
        }
        else if (aDir.x != aDir.y)
        {
            decision = dir.y > 0 ? movement.pY : movement.nY;
        }
        else 
        {
            Debug.Log("x and y equal");
            if(beat % 2 == 0)
                decision = dir.x > 0 ? movement.pX : movement.nX;
            else
                decision = dir.y > 0 ? movement.pY : movement.nY;
        }

        if (aDir.x <= minDist && aDir.y <= minDist && aDir.x != aDir.y)
        {
            Debug.Log("x and y dif, x < mindist and y < mindist");
            return movement.attack;
        }

        return decision;

    }
    protected movement decideUnstoppedMovement() 
    {

        Vector3 dir = rPlayer.Instance.transform.position - transform.position;
        Vector3 aDir = new Vector3(Mathf.Abs(dir.x), Mathf.Abs(dir.y), dir.z);
        movement decision = movement.stop;
        if (aDir.x > aDir.y)
        {
            decision = dir.x > 0 ? movement.pX : movement.nX;
        }
        else if (aDir.x != aDir.y)
        {
            decision = dir.y > 0 ? movement.pY : movement.nY;
        }
        else
        {
            Debug.Log("x and y equal");
            if (beat % 2 == 0)
                decision = dir.x > 0 ? movement.pX : movement.nX;
            else
                decision = dir.y > 0 ? movement.pY : movement.nY;
        }

        return decision;

    }
    // ------------------------- // // ------------------------- //
    #endregion

    protected void test(float testAngle)
    {

        //float testAngle = 90f;
        mySpheres[0].localPosition = new Vector3(0f, 1, mySpheres[0].localPosition.z);
        Vector2 test = new Vector2(mySpheres[0].localPosition.x, mySpheres[0].localPosition.y);
        test = test.rotateVec(testAngle);
        mySpheres[0].localPosition = new Vector3(test.x, test.y);

    }

    public enum movement
    {
        pY, pX, nY, nX, stop, attack
    }

}

#region
//deprecated
/*
 public void startRotation(float angle, float duration) 
    {
        if (rotating)
            return;
        rotAngle = angle;
        rotDuration = duration;
        rotate = true;

    }
    void rotationCheck() 
    {
        if (cTime <= rotDuration)
        {
            if (!pDebug)
            {
                //Debug.Log("started with rotation at " + Time.time);
                pDebug = true;
            }
            rotating = true;
            cTime = mySpheres.rotateDisc(45f, rotDuration, cTime);
        }
        else
        {
            //Debug.Log("done with rotation at " + Time.time);
            rotate = false;            
            cTime = 0f;
            pDebug = false;
            rotating = false;
        }
    }
 */
/*
     //not used as it is not reliable
    IEnumerator rotateRoutine(float degree, float time, float cTime) 
    {
        
        float frameRate = Time.fixedDeltaTime;
        int ticks = 0;
        Debug.Log("started rotation coroutine at: " + Time.time + " with framerate: " + frameRate);
        float angIncrease = degree / (time / frameRate);
        Debug.Log("the rotation should take " + ((time) / frameRate) + " ticks");

        float accDif = 0f;
        while (cTime <= time)
        {
            cTime = mySpheres.rotateDisc(degree, time, cTime, frameRate);
            ticks++;
            float timeB = Time.time;
            yield return new WaitForSeconds(frameRate);
            timeB = Time.time - timeB;
            if (timeB != frameRate) 
            {
                accDif += timeB-frameRate;
            }
        }
 
        Debug.Log("done with rotation coroutine at: " + Time.time + " with " + ticks + " ticks and " + (accDif) + " expected difference");

    }
 */
#endregion
