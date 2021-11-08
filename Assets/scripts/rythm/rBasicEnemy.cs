using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class rBasicEnemy : rEntity
{

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Beat()
    {
        base.Beat();
        transform.snapToGrid(false);
        beatPos = transform.position;
        myMovement = mClock ? decideMove() : movement.stop;
        mClock = !mClock;
        mStep = 0;
        //Debug.Log(" beat with " + myMovement);
    }
    protected override void halfBeat()
    {
        base.halfBeat();
    }

    protected override void fourthBeat()
    {
        base.fourthBeat();
    }

    protected override void eigthBeat()
    {
        base.eigthBeat();
        eigthRotateMarkerOneTwo();
        mStep = eigthMovementTo(beatPos, myMovement, mStep);
    }

}
