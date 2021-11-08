using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class line_thingie : MonoBehaviour
{

    [Range(1f, 5f)]
    public float size = 1;
    public int frequency = 100;
    public LineRenderer dirDemo;
    public LineRenderer norDemo;

    LineRenderer myLine;
    bool ready;

    void Start()
    {
        myLine = GetComponent<LineRenderer>();
    }

    void Update()
    {

        float T = Time.time;
        float mySin = Mathf.Sin(T);
        //float myDer = (Mathf.Sin(T + .1f) - Mathf.Sin(T)) / .1f;
        Vector3 sinDir = new Vector3(
            T + .1f - T,
            Mathf.Sin(T + .1f) - Mathf.Sin(T),
            0f
            );
        sinDir = Vector3.Normalize(sinDir) * 5f;
        Vector3 func = new Vector3(
                            T,
                            Mathf.Sin(T),
                            0f);

        setDemo(sinDir, func);
        Vector3 normal = new Vector3(-sinDir.y, sinDir.x, sinDir.z);
        setNor(normal, func);
        

        //Debug.Log(func);

        if (((int)Time.time * frequency) % 2f == 0f && ready)
        {
            Debug.Log("do the stuff");
            myLine.positionCount = myLine.positionCount + 1;            
            myLine.SetPosition(myLine.positionCount - 1, func);
            ready = false;
        }
        else
        {
            Debug.Log("do not do the stuff");
            ready = true;
        }
        

    }

    void setDemo(Vector3 dir)
    {
        dirDemo.positionCount = 2;
        dirDemo.SetPosition(0, Vector3.zero);
        dirDemo.SetPosition(1, dir * size);
    }
    void setDemo(Vector3 dir, Vector3 startPos)
    {
        dirDemo.positionCount = 2;
        dirDemo.SetPosition(0, startPos);
        dirDemo.SetPosition(1, startPos + dir * size);
    }

    void setNor(Vector3 dir)
    {
        norDemo.positionCount = 2;
        norDemo.SetPosition(0, Vector3.zero);
        norDemo.SetPosition(1, dir * size);
    }
    void setNor(Vector3 dir, Vector3 startPos)
    {
        norDemo.positionCount = 2;
        norDemo.SetPosition(0, startPos);
        norDemo.SetPosition(1, startPos + dir * size);
    }

}
