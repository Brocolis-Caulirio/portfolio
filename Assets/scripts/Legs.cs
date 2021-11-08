using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Legs : MonoBehaviour
{

    bool assigned = false;
    //the get child/childcount only gets direct child and no grandchildren
    Transform RFLeg;
    Transform RBLeg;
    Transform LFLeg;
    Transform LBLeg;

    leg rf;
    leg rb;
    leg lf;
    leg lb;

    public void Assign(Transform[] legs)
    {

        if (legs.Length != 4)
        {
            Debug.LogError("the number of legs assigned was not 4");
            return;
        }

        Transform[] right = new Transform[2];
        Transform[] left = new Transform[2];
        Transform[] front = new Transform[2];
        Transform[] back = new Transform[2];

        foreach (Transform legOb in legs)
        {

            int winsX = 0;
            int winsZ = 0;
            //Debug.Log("testing " + legOb.name);
            foreach (Transform comp in legs)
            {
                //Debug.Log("with: " + comp.name);
                if (legOb.localPosition.x > comp.localPosition.x && legOb != comp)
                    winsX++;
            }
            if (winsX >= 2)
                right[right.getEmpty()] = legOb;
            else
                left[left.getEmpty()] = legOb;

            foreach (Transform comp in legs)
            {
                if (legOb.localPosition.z > comp.localPosition.z && legOb != comp)
                    winsZ++;
            }
            if (winsZ >= 2)
                front[front.getEmpty()] = legOb;
            else
                back[back.getEmpty()] = legOb;

        }
        //Debug.Log("ran all tests");
        RFLeg = front.getMatching(right);
        RBLeg = right.getMatching(back);
        LFLeg = left.getMatching(front);
        LBLeg = left.getMatching(back);

        rf = new leg();
        rb = new leg();
        lf = new leg();
        lb = new leg();

        rf.Assign(RFLeg);
        rb.Assign(RBLeg);
        lf.Assign(LFLeg);
        lb.Assign(LBLeg);

        assigned = true;

    }

    public void runtime()
    {
        if (!assigned)
        {
            Debug.LogError("unassigned leg at: " + transform.name);
            return;
        }

        rf.runtime();
        lf.runtime();
        rb.runtime();
        lb.runtime();
    }

}

public class leg
{

    bool assigned = false;

    float hkDist;
    float kfDist;
    float ftDist;

    Transform hip;
    Transform knee;
    Transform foot;
    Transform toes;

    Vector3 hFwd;
    Vector3 kFwd;
    Vector3 fFwd;
    Vector3 tFwd;

    float kDist;
    float fDist;
    float tDist;

    public void Assign(Transform Hip)
    {

        hip = Hip;
        knee = hip.GetChild(0);
        foot = knee.GetChild(0);
        toes = foot.GetChild(0);

        hFwd = knee.localPosition;
        kFwd = foot.localPosition;
        fFwd = toes.localPosition;
        tFwd = toes.forward;

        kDist = hFwd.magnitude;
        fDist = kFwd.magnitude;
        tDist = fFwd.magnitude;

        assigned = true;

    }
    public void runtime()
    {
        if (!assigned)
        {
            Debug.LogError("unassigned leg");
            return;
        }

        lookAtEachOther();
    }

    #region movement functions

    void lookAtEachOther()
    {

        if (foot.localPosition != kFwd) 
        {
            Vector3 fp = foot.position;
            Vector3 dir = foot.position - knee.position;
            knee.forward += dir.normalized - kFwd.normalized;            
        }

        foot.localPosition = foot.localPosition.normalized * fDist;
        kFwd = foot.position - knee.position;

    }

    void reverseParentMovement()
    {
        //make them move according to oposite parent order
        //ie: toes move feet, feet move knee and so on
    }
    #endregion

}