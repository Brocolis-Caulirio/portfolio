using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class CustomExtensionMethods
{

    public static float floor(this float i)
    {
        return Mathf.Floor(i);
    }

    public static Vector3 sinWaveNormal(float input, out Vector3 normal) 
    {
        
        float S = Mathf.Sin(input);
        Vector3 vec = Vector3.zero;
        Vector3 nor = Vector3.zero;

        vec = new Vector3(input, S, 0f);
        Vector3 dir = new Vector3(input + .1f, Mathf.Sin(input + .1f) - S, 0f);
        nor = new Vector3(-dir.y, dir.x, 0f);
        nor = Vector3.Normalize(nor);

        normal = nor;
        return vec;

    }

}

