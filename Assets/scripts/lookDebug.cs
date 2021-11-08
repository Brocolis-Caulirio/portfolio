using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class lookDebug : MonoBehaviour
{
    public Transform lookAt;
    void Update()
    {

        transform.forward = (lookAt.position - transform.position).normalized;

    }
}
