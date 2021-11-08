using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class unparentedLegs : MonoBehaviour
{
    public Transform hip;
    public Transform knee;
    public Transform foot;

    float kDist;
    float fDist;

    void Start()
    {

        kDist = (knee.position - hip.position).magnitude;
        fDist = (foot.position - knee.position).magnitude;

    }

    void Update()
    {

        //if changing the knee, preserve foot angle
        Vector3 hFwd = (knee.position - hip.position).normalized;        
        if (hip.forward != hFwd && false)
        {
            Vector3 footPreserve = Vector3.Cross(foot.forward, knee.forward);
            hip.forward = hFwd;
            knee.position = hip.forward * kDist;
            foot.position = knee.forward * fDist;
            foot.forward = Vector3.Cross(footPreserve, knee.forward);
        }

        Vector3 kFwd = (foot.position - knee.position).normalized;
        if (knee.forward != kFwd)
        {
            Vector3 footPreserve = Vector3.Cross(foot.forward, knee.forward);
            knee.forward = kFwd;
            foot.position = knee.forward * fDist;
            //foot.forward = Vector3.Cross(footPreserve, knee.forward);
        }





    }
}
