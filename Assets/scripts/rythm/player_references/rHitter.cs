using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class rHitter : MonoBehaviour
{

    public bool OverrideHit = false;
    public bool Transparent = true;
    public Color myColor;
    
    Collider2D col;
    SpriteRenderer myRend;
    bool canRend;
    bool tried = false;

    private void OnEnable()
    {
        if (col == null)
            col = GetComponent<Collider2D>();
        col.isTrigger = true;
        if(Transparent)
            myColor = new Color(myColor.r, myColor.g, myColor.b, .5f);
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {

        Debug.Log("hitting " + collision.name);
        if (myRend == null && !tried) 
        {
            tried = true;
            myRend = gameObject.GetComponent<SpriteRenderer>();
            if (myRend == null)
                canRend = false;
            else
                canRend = true;
        }

        if (OverrideHit)
            return;

        if (canRend)
            myRend.color = myColor;
        if (collision.CompareTag("Player"))
            rPlayer.Instance.GetHit(1, myColor.oneAlpha());
        else if (collision.CompareTag("Enemy"))
            collision.gameObject.GetComponent<rEntity>().GetHit();
    }

}
