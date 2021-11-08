using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class rEntity : MonoBehaviour
{

    //references
    protected SpriteRenderer myRenderer;
    public GameObject Attacker;
    public Transform[] mySpheres;
    public Color[] MyColors;
    public Color currentColor;
    protected int cColorIndex;
    public int StartingColor;
    protected Rigidbody2D rb;

    //constants
    [Space(25)]
    public float rotDuration = 2f;
    public float walkDist;
    public float minDist = 2f;

    //beats related
    bool added;
    protected float lastmove = 0f;
    protected int beat = 0;
    protected int rStep = 0;
    protected int mStep = 0;

    //information between frames
    protected movement myMovement;
    protected Vector3 beatPos;
    protected bool mClock;
    protected Vector2[][] rotationInfo = null;
    

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
        rb = GetComponent<Rigidbody2D>();
        if (!added)
        {
            beatCheck();
            SetupColors();
        }

        }
    protected virtual void OnDisable() 
    {
        Debug.Log("removing beats at " + transform.name);
        rRythmManager.instance.beat.RemoveListener(Beat);
        rRythmManager.instance.halfBeat.RemoveListener(halfBeat);
        rRythmManager.instance.fourthBeat.RemoveListener(fourthBeat);
        rRythmManager.instance.eigthBeat.RemoveListener(eigthBeat);
        Debug.Log("removed beats at " + transform.name);
    }
    protected virtual void Start() 
    {
        if (!added)
        {
            beatCheck();
            SetupColors();
        }
        }
    // ------------------------- // // ------------------------- //
    #endregion

    #region beats
    // ------------------------- // // ------------------------- //
    protected virtual void Beat()
    {
        beat = rRythmManager.instance.getBeat();
    }
    protected virtual void halfBeat()
    {

    }
    protected virtual void fourthBeat()
    {

    }
    protected virtual void eigthBeat() 
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
        float xVal = 0f;
        float yVal = 0f;

        if (fracX < .3f)
            xVal = 0f;
        else if (fracX < .6f)
            xVal = .5f;
        else
            xVal = 1f;

        if (fracY < .3f)
            yVal = 0f;
        else if (fracY < .6f)
            yVal = .5f;
        else
            yVal = 1f;

        transform.position = new Vector3(Mathf.Floor(pos.x) + xVal, Mathf.Floor(pos.y) + yVal, pos.z);

    }
    protected virtual void beatCheck() 
    {
        if (rRythmManager.instance == null)
            return;

        Debug.Log("adding beats at " + transform.name);
        rRythmManager.instance.beat.AddListener(Beat);
        rRythmManager.instance.halfBeat.AddListener(halfBeat);
        rRythmManager.instance.fourthBeat.AddListener(fourthBeat);
        rRythmManager.instance.eigthBeat.AddListener(eigthBeat);
        Debug.Log("added beats at " + transform.name);
        added = true;

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

    //the commom use case in this class only has the clockwise input necessary
    //this is just a better readability func of rUtilities.lerpDiscRotation
    protected virtual Vector2[][] eigthRotateMarkerOneZero( Vector2[][] passOn, bool clockWise)
    {

        Vector2[][] info = passOn;
        float mult = clockWise ? -1f : 1f;
        if (rStep >= 0)
            info = mySpheres.lerpDiscRotation( (360f / mySpheres.Length) * mult, rStep, info);
        rStep++;

        if (rStep == 8)
        {
            rStep = 0;
            ChangeColor();
        }

        return info;

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
        cColorIndex = iterator;

    }
    protected void SetupColors() 
    {

        if (rRythmManager.instance == null)
            return;

        myRenderer = GetComponent<SpriteRenderer>();
        
        Color[] managerCols = rRythmManager.instance.GetColors();
        MyColors = new Color[managerCols.Length];

        int withOffset = StartingColor;
        int lowest = mySpheres.Length < MyColors.Length ? mySpheres.Length : MyColors.Length;

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
        
        for (int i = 0; i < lowest; i++) 
        {
            withOffset = StartingColor + i;
            if (withOffset >= mySpheres.Length)
                withOffset = mySpheres.Length - withOffset;
            MyColors[i] = managerCols[withOffset];
            mySpheres[i].GetComponent<SpriteRenderer>().color = MyColors[i];
        }
        currentColor = managerCols[StartingColor];
        myRenderer.color = currentColor;

    }
    bool clockWise(Color chasingColor) 
    {

        int index = -1;
        for (int i = 0; i < MyColors.Length; i++) 
        {
            if (MyColors[i] == chasingColor)
                index = i;
        }
        if (index < 0)
        {
            Debug.LogError("NO COLOR PRESENT");
            return false;
        }
        float distAnti = MyColors.marchDist<Color>(cColorIndex, index, true);
        float distCloc = MyColors.marchDist<Color>(cColorIndex, index, false);
        
        return distAnti > distCloc;

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
        rb.MovePosition(Vector3.Lerp(start, end, interpolator));

        //Debug.Log("moving " + transform.name + " at step " + theStep + " with " + (Time.time - lastmove) + " interval");
        lastmove = Time.time;

        return theStep + 1;

    }
    protected virtual void staticEigthMoveTo(Vector3 start, Vector3 end, int theStep)
    {

        if (theStep > 8)
            return;
        theStep = theStep == 0 ? 1 : theStep;

        float interpolator = (float)theStep / 8f;
        rb.MovePosition(Vector3.Lerp(start, end, interpolator));

        //Debug.Log("moving " + transform.name + " at step " + theStep + " with " + (Time.time - lastmove) + " interval");
        lastmove = Time.time;

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
                Attacker.transform.snapToGrid(true);
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
            //Debug.Log("x and y equal");
            if(beat % 2 == 0)
                decision = dir.x > 0 ? movement.pX : movement.nX;
            else
                decision = dir.y > 0 ? movement.pY : movement.nY;
        }

        if (aDir.x <= minDist && aDir.y <= minDist && aDir.x != aDir.y)
        {
            //Debug.Log("x and y dif, x < mindist and y < mindist");
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



#region unused code
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
