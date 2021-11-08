using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[RequireComponent(typeof(AudioSource))]
[RequireComponent(typeof(Image))]
public class rRythmManager : MonoBehaviour
{

    [Space(25)]
    //public Material debugHeal;
    //public Material debugDamage;
    public Material debugHealth;
    public AudioClip clipDebug;
    public bool DEBUGGINGEIGTH;
    public float trueFill;

    [Space(25)]
    public static rRythmManager instance;
    public UnityEvent beat;
    public UnityEvent halfBeat;
    public UnityEvent fourthBeat;
    public UnityEvent eigthBeat;

    [SerializeField]
    Color[] rColors;

    rPlayer myPlayer;
    CanvasRenderer rend;
    Material[] myUi;
    //Material[] myDmg;
    Material[] myHp;
    //Material[] myHeal;
    AudioSource source;
    AudioSource backupSource;
    AudioSource eigthSource;    

    [Space(25)]    
    //public Image damageBar;
    public Image healthBar;
    //public Image healBar;
    int maxHealth;
    int playerHealth;
    float fillNumber;
    //float damageFill;
    //float healFill;
    //float showingDamageDelay;
    //float showingHealDelay;

    [Space(25)]
    public float BPM;
    float beatDelay;
    public int beatHighlight;

    int currentBeat = 0;
    float lastBeat = 0f;
    float currentTime = 0f;

    float pitch;

    private void Awake()
    {

        if (instance != null)
        {
            Debug.LogError("MORE THAN ONE RYTHM INSTANCE AT " + transform.name + " AND " + instance.transform.name);
            return;
        }
        instance = this;

        rend = GetComponent<CanvasRenderer>();

        maxHealth = 0;
        beat = new UnityEvent();
        halfBeat = new UnityEvent();
        fourthBeat = new UnityEvent();
        eigthBeat = new UnityEvent();

        source = GetComponent<AudioSource>();
        backupSource = gameObject.AddComponent<AudioSource>();
        eigthSource = gameObject.AddComponent<AudioSource>();
        backupSource.copyInfo(source);
        eigthSource.copyInfo(source);

        pitch = source.pitch;

        if (BPM == 0)
            BPM = 60;
        if (beatHighlight == 0)
            beatHighlight = 4;

        beatDelay = 60f / BPM;

        if ((beatDelay * 2) - source.clip.length < 0)
            Debug.LogError("BPM TOO HIGH FOR THIS SOUND BY " + ((beatDelay * 2) - source.clip.length < 0) + " SECONDS");

    }

    void Update()
    {
        beatDelay = 60 / BPM;
        float beatDif = Time.time - lastBeat;
        if (beatDif > beatDelay)
        {
            //Debug.Log(" beat manager beat");
            StartCoroutine(AllBeats());
            lastBeat = Time.time;
        }
        moveShader(beatDelay * Time.deltaTime);

    }

    IEnumerator AllBeats() 
    {

        
        float eightTime = beatDelay / 8f;
        //Debug.Log("calling all beats at " + Time.time + " with the 1/8 of time being " + eightTime);
        PlaySound();
        healthGraphicBumpPercent(2.5f);
        beat.Invoke();

        for (int i = 0; i < 8; i++) 
        {          

            //  i % 8 == 0
            //  beat.Invoke();
            if (i % 4 == 0)
                halfBeat.Invoke();
            if (i % 2 == 0)
                fourthBeat.Invoke();
            //  i % 1 == 0
            eigthBeat.Invoke();

            checkOnPlayer();

            if (DEBUGGINGEIGTH)
                eigthSource.PlayOneShot(clipDebug, .1f);

            yield return new WaitForSeconds(eightTime);

        }

        //Debug.Log("done with all beats at " + Time.time);

        #region proof
        //proof:
        /*
        beat.Invoke();
        halfBeat.Invoke();
        fourthBeat.Invoke();
        eigthBeat.Invoke();
        await Task.Delay(eightTime); // 4 beat 8/8

        eigthBeat.Invoke();
        await Task.Delay(eightTime); // 1 beat 1/8

        fourthBeat.Invoke();
        eigthBeat.Invoke();
        await Task.Delay(eightTime); // 2 beat 2/8

        eigthBeat.Invoke();
        await Task.Delay(eightTime); // 1 beat 3/8

        halfBeat.Invoke();
        fourthBeat.Invoke();
        eigthBeat.Invoke();
        await Task.Delay(eightTime); // 3 beat 4/8

        eigthBeat.Invoke();
        await Task.Delay(eightTime); // 1 beat 5/8

        fourthBeat.Invoke();
        eigthBeat.Invoke();
        await Task.Delay(eightTime); // 2 beat 6/8

        eigthBeat.Invoke();
        await Task.Delay(eightTime); // 1 beat 7/8
         */
        #endregion

    }

    #region feedback related
    void checkOnPlayer() 
    {

        if (myPlayer == null)
        {
            myPlayer = rPlayer.Instance;
            return;//to avoid needless bugs
        }

        if (maxHealth == 0) 
        {
            maxHealth = myPlayer.GetMaxHealth();
            playerHealth = maxHealth;
        }
        else
            maxHealth = myPlayer.GetMaxHealth();
        
        playerHealth = myPlayer.GetHealth();

        /*
        int pHealth = myPlayer.GetHealth();        
        if (playerHealth < pHealth)
        {
            Debug.Log("player took damage");
            DamageDisplay(playerHealth, pHealth, .5f);
            playerHealth = pHealth;
        }
        else if(playerHealth > pHealth)
        {
            Debug.Log("player healed damage");
            HealDisplay(playerHealth, pHealth, .5f);
            playerHealth = pHealth;
        }
        */

    }
    void PlaySound()
    {

        AudioSource current = currentBeat % 2 == 0 ? source : backupSource;
        AudioSource paralel = currentBeat % 2 == 0 ? backupSource : source;
        current.volume = 1f;
        paralel.volume = 0.75f;
        current.pitch = currentBeat == beatHighlight ? pitch * 2f : pitch;
        current.playPriority();
        lastBeat = Time.time;
        currentBeat++;
        if (currentBeat == beatHighlight)
            currentBeat = 0;     

    }
    void moveShader(float toAdd) 
    {

        float goalTime = currentTime + toAdd;

        #region beat graphics
        int count = rend.materialCount;
        if ((myUi == null || myUi.Length != count) && count > 0)
        {
            Debug.Log("conditions met and setting up myUi");
            myUi = new Material[count];
            for (int i = 0; i < count; i++)
                myUi[i] = rend.GetMaterial(i);
        }
        else 
        {
            if (myUi == null)
                Debug.Log("myUI is null");
            else if (myUi.Length != count)
                Debug.Log("myUI length != material count");
            else if (count <= 0)
                Debug.Log("my material count: " + count);
        }

        if (myUi != null) 
        {
            for (int i = 0; i < myUi.Length; i++)
                myUi[i].SetFloat("_Bpm", goalTime);
        }
        #endregion

        #region player hp display

        // ------------------------- // // ------------------------- //
        // pre calculations

        trueFill = (float)playerHealth / (float)maxHealth; // health percentage
        float addVal = 0;// (Mathf.Sin(currentTime) / 2f + .5f) * .1f; // positive sin wave of length .1
        float fillGoal = addVal + trueFill; // health percentage + sin wave
        
        // ------------------------- // // ------------------------- //

        #region check nan vals and 0 vals
        if ((fillNumber == 0 && playerHealth != 0) || float.IsNaN(fillNumber))
            fillNumber = trueFill;
        /*
        if ((damageFill == 0 && playerHealth != 0) || float.IsNaN(damageFill))
            damageFill = trueFill;
        if ((healFill == 0 && playerHealth != 0) || float.IsNaN(healFill))
            healFill = trueFill;
        */
        #endregion

        // ------------------------- // // ------------------------- //
        // material settings

        checkDamageAndHPMats(); // checks for existence of materials, assigns if missing
        fillNumber = Mathf.Lerp(fillNumber, fillGoal, toAdd*2); // interpolates fillnumber to desired number
        if (!float.IsNaN(fillNumber))
        {
            foreach (Material i in myHp)
                i.SetFloat("_FillAmount", fillNumber);

            debugHealth.SetFloat("_FillAmount", fillNumber);
        }

        // ------------------------- // // ------------------------- //

        // color setting
        Vector3 blue = new Vector3(.25f, .5f, 1f);
        Vector3 red = new Vector3(1f, .0f, .2f);
        Vector3 green = new Vector3(.0f, 1f, .5f);
        Vector3 vecCol = Vector3.Lerp(red, green, trueFill);
        //vecCol = Vector3.Lerp(vecCol, green, trueFill);

        float opacity = .75f;
        Color col = trueFill == 1 ? new Color(blue.x, blue.y, blue.z, opacity) :  
                                    new Color(vecCol.x, vecCol.y, vecCol.z, opacity);
        healthBar.color = col;


        #endregion

        currentTime = goalTime;
        return;

        #region heal/damge bar display logic
        /*
        Color damageColor = new Color(1f, 0.3f, .2f, 1f);
        Color healColor = new Color(.2f, .3f, 1f, 1f);
        Color transparent = new Color(0f, 0f, 0f, 0f);

        if (showingHealDelay > 0 || trueFill - healFill < .01)
            healBar.color = healColor; //gameObject.SetActive(true);
        else
            healBar.color = transparent;

        if (showingDamageDelay > 0 || trueFill - damageFill < .01)
            damageBar.color = damageColor;
        else
            damageBar.color = transparent;
        */
        #endregion
        #region heal and damage lerps
        /*
        if (showingDamageDelay < 0 && !float.IsNaN(damageFill))
        {
            damageFill = Mathf.Lerp(damageFill, trueFill - .1f, toAdd);
            foreach (Material i in myDmg)
                i.SetFloat("_FillAmount", damageFill);

            debugDamage.SetFloat("_FillAmount", damageFill);
        }
        if (showingHealDelay < 0 && !float.IsNaN(healFill))
        {
            healFill = Mathf.Lerp(healFill, trueFill, toAdd);
            foreach (Material i in myHeal)
                i.SetFloat("_FillAmount", healFill);

            debugHeal.SetFloat("_FillAmount", healFill);
        }
        */
        #endregion
        /*
        showingDamageDelay -= toAdd;
        showingHealDelay -= toAdd;
        */


    }

    void checkDamageAndHPMats() 
    {

        /*
        if (myDmg != null && myHp != null && myHeal != null &&
            myDmg.Length > 0 && myHp.Length > 0 && myHeal.Length > 0)
            return;       
        */
        if (myHp != null && myHp.Length > 0 )
            return;

        //CanvasRenderer cDmg = damageBar.canvasRenderer;
        CanvasRenderer cHp = healthBar.canvasRenderer;
        //CanvasRenderer cHeal = healBar.canvasRenderer;

        //int count = cDmg.materialCount;
        //if(myDmg == null || myDmg.Length != count)
        //    myDmg = new Material[count];

        //for (int i = 0; i < count; i++)
        //    myDmg[i] = cDmg.GetMaterial(i);

        int count = cHp.materialCount;
        if (myHp == null || myHp.Length != count)
            myHp = new Material[count];

        for (int i = 0; i < count; i++)
            myHp[i] = cHp.GetMaterial(i);

        //count = cHeal.materialCount;
        //if (myHeal == null || myHeal.Length != count)
        //    myHeal = new Material[count];

        //for (int i = 0; i < count; i++)
        //    myHeal[i] = cHeal.GetMaterial(i);

    }

    public void healthGraphicBump(float bump) 
    {
        bump /= maxHealth;
        bump = Mathf.Min(1f, bump);
        fillNumber += bump;
        fillNumber = Mathf.Min(1.3f, fillNumber);// just to be sure
    }
    public void healthGraphicBumpPercent(float bump)
    {
        bump /= maxHealth;
        bump = Mathf.Min(1f, bump);
        fillNumber += (1 - fillNumber) * bump;
        fillNumber = Mathf.Min(1.3f, fillNumber);// just to be sure
    }

    #region heal and damgage deprecated functions
    /*
    void DamageDisplay(int prevHealth, int curHealth,float delay) 
    {

        if (showingDamageDelay < 0) 
        {
            showingDamageDelay = delay;
            foreach(Material i in myDmg)
                i.SetFloat("_FillAmount", prevHealth / maxHealth);
        }
        else
            showingDamageDelay += delay;

        healFill = trueFill;
        healthGraphicBump(curHealth - prevHealth);


    }
    void HealDisplay(int prevHealth, int curHealth, float delay)
    {

        if (showingHealDelay < 0)
        {
            showingHealDelay = delay;
            foreach (Material i in myHeal)
                i.SetFloat("_FillAmount", prevHealth / maxHealth);
        }
        else
            showingHealDelay += delay;

        damageFill = trueFill;
        healthGraphicBump(prevHealth - curHealth);

    }
    IEnumerator healTimer(float time)
    {
        healBar.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        healBar.gameObject.SetActive(false);
    }
    IEnumerator damageTimer(float time)
    {
        damageBar.gameObject.SetActive(true);
        yield return new WaitForSeconds(time);
        damageBar.gameObject.SetActive(false);
    }
    */
    #endregion

    #endregion

    #region getters
    public int getBeat() 
    {
        return currentBeat;
    }
    public Color[] GetColors() 
    {
        return rColors;
    }
    public float GetBeatDelay() 
    {
        return beatDelay;
    }
    #endregion


}
