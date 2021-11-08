using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class rPlayer : rEntity
{

    //reference
    public static rPlayer Instance;
    float PerfectTime;
    float HalfTime;

    //player status
    [SerializeField]
    private int maxHealth;
    [SerializeField]
    private int health;

    //inputs
    bool pressingAttack = false;
    bool pressingMove = false;
    bool pressingChange = false;
    float attackTime = 0f;
    float walkTime = 0f;
    float changeTime = 0f;
    bool saveable;

    //information between frames
    movement attackDir = movement.stop;
    bool moving = false;
    Vector3 destination;
    bool attacking;
    Vector3 attackPos;
    bool changingColor;
    bool clockWise;    
    float waitingForBeat = -1;

    //beat counts
    bool fullBeatRunning;
    int halfBeatCount;
    int fourthBeatCount;
    int eigthBeatCount;


    #region setup
    void Awake()
    {
        if (Instance != null) 
        {
            Debug.LogError("TWO PLAYER INSTANCES AT " + transform.name + " AND " + Instance.transform.name);
            return;
        }
        Instance = this;
    }
    protected override void OnEnable()
    {
        base.OnEnable();
        stopAllMovement();
    }
    protected override void OnDisable()
    {
        base.OnDisable();
    }
    #endregion

    #region beats
    protected override void Beat()
    {

        fullBeatRunning = true;
        //warm up set up
        base.Beat();
        transform.snapToGrid(false);
        beatPos = transform.position;
        mClock = true;
        mStep = 0;        

        //has to be after the setup bc it can invoke movement
        setMoveFalse();

    }
    protected override void halfBeat()
    {
        base.halfBeat();
        halfBeatCount++;
    }

    protected override void fourthBeat()
    {
        base.fourthBeat();
        fourthBeatCount++;

        if (fourthBeatCount == 4)
            saveable = true;
        else if (fourthBeatCount == 2)
            saveable = false;

        mClock = false;       
    }

    protected override void eigthBeat()
    {
        base.eigthBeat();
        eigthBeatCount++;

        bool isPressing = pressingMove || pressingAttack || pressingChange;
        bool noAction = !moving && !changingColor && !attacking;

        if (changingColor)
            rotationInfo = eigthRotateMarkerOneZero(rotationInfo, clockWise);
        if (changingColor && rStep == 0)
            changingColor = false;

        if (!attacking) // third attacking frame deactivates attacker and on rest of frames as well
            Attacker.SetActive(false);

        if (attacking && !Attacker.activeSelf) // first attacking frame
        {
            Attacker.transform.position = attackPos;
            Attacker.SetActive(true);
        }
        else if (Attacker.activeSelf) // second attacking frame
            attacking = false;


        if (moving)
        {
            staticEigthMoveTo(beatPos, destination, mStep);
            Debug.Log("moving from " + beatPos + " to " + destination);
        }
        mStep++;

        
        if (noAction && isPressing && saveable)
        {
            Move();
            saveable = false;
        }        

        fullBeatRunning = false;

    }

    protected override void beatCheck()
    {
        base.beatCheck();
        if (rRythmManager.instance != null)
        {
            PerfectTime = rRythmManager.instance.GetBeatDelay() / 8;
            HalfTime = rRythmManager.instance.GetBeatDelay() / 4;
        }
    }
    #endregion

    #region Updates

    private void Update()
    {

        bool pW = Input.GetKeyDown(KeyCode.W);
        bool pA = Input.GetKeyDown(KeyCode.A);
        bool pS = Input.GetKeyDown(KeyCode.S);
        bool pD = Input.GetKeyDown(KeyCode.D);

        bool pF = Input.GetKeyDown(KeyCode.UpArrow);
        bool pB = Input.GetKeyDown(KeyCode.DownArrow);
        bool pL = Input.GetKeyDown(KeyCode.LeftArrow);
        bool pR = Input.GetKeyDown(KeyCode.RightArrow);

        bool pE = Input.GetKeyDown(KeyCode.E);
        bool pQ = Input.GetKeyDown(KeyCode.Q);

        if ((pF || pB || pL || pR) && !pressingAttack)
        {

            pressingAttack = true;
            attackTime = Time.time;

            if (pF)
                attackDir = movement.pY;
            else if (pB)
                attackDir = movement.nY;
            else if (pR)
                attackDir = movement.pX;
            else if (pL)
                attackDir = movement.nX;            

            myMovement = movement.attack;

        }
        if ((pW || pS || pA || pD) && !pressingMove)
        {

            pressingMove = true;
            walkTime = Time.time;

            if (pW)
                myMovement = movement.pY;
            else if (pS)
                myMovement = movement.nY;
            else if (pD)
                myMovement = movement.pX;
            else if (pA)
                myMovement = movement.nX;

        }

        if ((pE || pQ) && !pressingChange)
        {
            if (pE && pQ)
            {
                //handles the both button pressed without much randomness while being kinda random
                clockWise = beat % 2 == 0 ? pE : pQ;
            }
            else
                clockWise = pE ? true : false;
            pressingChange = true;
        }

    }

    private void FixedUpdate()
    {
        
    }

    #endregion

    #region movement related

    void Move() 
    {

        Debug.Log("called move");

        float inputTime = pressingMove ? walkTime :
                          pressingAttack ? attackTime : changeTime;

        float inputDelay =  eigthBeatCount < 2 ? PerfectTime : 
                            eigthBeatCount < 3 ? HalfTime :
                            Mathf.Abs(Time.time - inputTime);

        bool twoInputs = (pressingMove && pressingAttack);

        #region before beat handling
        //this will be "solved" on the setMoveFalse() function
        if (halfBeatCount > 1 && waitingForBeat < 0 && !fullBeatRunning)
        {
            waitingForBeat = eigthBeatCount > 7 ? 1f :
                             eigthBeatCount > 6 ? 0f : -1f;
            Debug.Log("post halfBeat, will be solved on beat: " + waitingForBeat);
            return;
        }

        inputDelay = waitingForBeat < 0 ? inputDelay :
                     waitingForBeat < 1 ? HalfTime : PerfectTime;
        #endregion

        if (inputDelay <= PerfectTime)
        {

            //attack first because it alligns better
            if (pressingAttack)
            {

                attackPos = beatPos + (attackDir.movementToVec3() * walkDist);
                attacking = true;

            }

            if (pressingMove) 
            {

                destination = beatPos + (myMovement.movementToVec3() * walkDist);
                moving = true;

            }

            Debug.Log("moving at perfect time " + myMovement);
            

        }
        else if (inputDelay <= HalfTime)
        {
            if (pressingMove)
            {

                destination = beatPos + (myMovement.movementToVec3() * walkDist);
                moving = true;
                attacking = false;

            }
            else if ( pressingAttack )
            {
                
                attackPos = beatPos + (attackDir.movementToVec3() * walkDist);
                attacking = true;
                moving = false;

            }
            Debug.Log("moving at half time " + myMovement);
        }
        else 
        {
            Debug.Log("TOO SLOOOW BY " + (inputDelay - HalfTime));
        }

        if(pressingChange && Time.time - changeTime < HalfTime)
            changingColor = true;

        pressingMove = false;
        pressingAttack = false;
        pressingChange = false;

    }

    void setMoveFalse()
    {       

        halfBeatCount = 0;
        fourthBeatCount = 0;
        eigthBeatCount = 0;

        //movement setup
        moving = false;
        attacking = false;
        //changingColor = false; // can't be here as it is off the beat clock

        //before beat input handler
        if (waitingForBeat >= 0)
        {
            Debug.Log("solving move on beat");
            Move();
        }

        //post setup
        pressingMove = false;
        pressingAttack = false;
        pressingChange = false;
        saveable = true;
        waitingForBeat = -1f;
        myMovement = movement.stop;
        #region explanation:
        /* 
         * if left alone and you input right before the beat changes
         * with current script it would teleport you to the end and let you input again
         * to avoid that this makes it wait for the next beat and then starts from 0
         * 
         * enemies don't have this bc what they will do is decided perfectly on beat
         * player input is decided on update which runs apart from input
         */
        #endregion

    }

    #endregion

    #region outputs for other classes
    public int GetHealth() 
    {
        return health;
    }
    public int GetMaxHealth() 
    {
        return maxHealth;
    }
    #endregion

    #region utility

    void stopAllMovement() 
    {
        destination = transform.position;
        attackPos = transform.position;
        moving = false;
        pressingAttack = false;
        pressingChange = false;
        pressingMove = false;
        myMovement = movement.stop;
    }

    #endregion

}
