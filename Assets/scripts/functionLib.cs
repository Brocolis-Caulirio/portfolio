using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class functionLib 
{
    public static int getEmpty<T>(this T[] input)
    {
        if (input.Length <= 0)
            return 0;

        for (int i = 0; i < input.Length; i++)
        {
            if (input[i] == null)
            {
                return i;
            }
        }

        return input.Length - 1;

    }

    public static T getMatching<T>(this T[] val, T[] comp) 
    {

        foreach (T mine in val) 
        {
            foreach (T c in comp)
                if (c.Equals(mine))
                {
                    //Debug.Log("found match");
                    return c;                    
                }
        }
        return val[0];

    }

}
