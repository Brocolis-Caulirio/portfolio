using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class procedural_quadruped : MonoBehaviour
{

    public Transform[] myLegs;
    public Legs legs;

    void Start()
    {
        if (myLegs == null)
        {
            Debug.LogError("NO LEGS AT " + transform.name);
            return;
        }
        legs = new Legs();
        legs.Assign(myLegs);
    }

    void Update()
    {
        //transform.position = transform.position + transform.forward * Time.deltaTime;
        legs.runtime();
        //transform.forward = transform.forward + (transform.right * Time.deltaTime);
    }

}
