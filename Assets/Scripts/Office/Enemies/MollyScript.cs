using System.Collections.Generic;
using UnityEngine;

public class MollyScript : EnemyScript
{
    class Mole
    {
        public int signNumber { get; private set; }
        enum States { Hidden, Waiting, Clicked }
        States currentState;
        int stateCounter;
        float stateTimer;
        const float smearAnimTimer = 0.03f;
        const float clickedAnimTimer = 0.05f;
        Sprite[] sprites;
        SpriteRenderer r;
        SpriteWobbleScript spriteWobble;
        BoxCollider cursorTrigger;
        public Mole(GameObject _moleObj)
        {
            sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Mole");
            r = _moleObj.GetComponent<SpriteRenderer>();
            spriteWobble = _moleObj.GetComponent<SpriteWobbleScript>();
            cursorTrigger = _moleObj.GetComponent<BoxCollider>();
            r.sprite = sprites[0];
            cursorTrigger.enabled = false;
            currentState = States.Hidden;
            stateCounter = 1;
            stateTimer = 0;
        }
        public void Update()
        {
            switch (currentState)
            {
                case States.Hidden:
                    if (stateCounter == 0)
                    {
                        stateTimer += Time.deltaTime;
                        if (stateTimer >= smearAnimTimer)
                        {
                            r.sprite = sprites[0];
                            stateCounter++;
                        }
                    }
                    break;
                case States.Waiting:
                    if (stateCounter == 0)
                    {
                        stateTimer += Time.deltaTime;
                        if (stateTimer >= smearAnimTimer)
                        {
                            r.sprite = sprites[signNumber + 2];
                            stateCounter++;
                        }
                    }
                    break;
                case States.Clicked:
                    if (stateCounter < 3)
                    {
                        stateTimer += Time.deltaTime;
                        if (stateTimer >= clickedAnimTimer)
                        {
                            stateCounter++;
                            r.sprite = sprites[7 + stateCounter];
                            stateTimer -= clickedAnimTimer;
                        }
                    }
                    break;
            }
        }
        public void Show(int _signNumber)
        {
            signNumber = _signNumber;
            currentState = States.Waiting;
            stateCounter = 0;
            stateTimer = 0;
            r.sprite = sprites[1];
            cursorTrigger.enabled = true;
        }
        public void Clicked()
        {
            spriteWobble.PlayAnim();
            cursorTrigger.enabled = false;
            currentState = States.Clicked;
            stateCounter = 0;
            stateTimer = 0;
            r.sprite = sprites[5 + signNumber];
        }
        public void Hide()
        {
            currentState = States.Hidden;
            stateCounter = 0;
            stateTimer = 0;
            r.sprite = sprites[1];
        }
    }
    Mole[] moles = new Mole[3];
    int stateCounter;
    float stateTimer;
    float moveCooldown { get { return Random.value > 0.75f ? Random.Range(2.5f, 5f) : Random.Range(7.5f, 12.5f); } }
    int missCounter;
    int moleCombo;
    float moleTimer;
    Sprite[] sprites;
    SpriteRenderer r;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Tournament_Molly; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Molly");
        r = transform.GetChild(0).GetComponent<SpriteRenderer>();
        r.sprite = sprites[0];
        stateTimer = moveCooldown;
        //IncreaseAI(10);
        for (int i = 0; i < 3; i++) { moles[i] = new Mole(transform.GetChild(i + 1).gameObject); }
    }
    protected override void OnUpdate()
    {
        if (moleTimer > 0)
        {
            moleTimer -= Time.deltaTime;
            if (moleTimer <= 0) { ShuffleMoles(); }
        }
        //if (Input.GetKeyDown(KeyCode.N)) { ShuffleMoles(); }
        //if (Input.GetKeyDown(KeyCode.M)) { Attack(); }
        foreach (Mole mole in moles) { mole.Update(); }
        if (stateCounter < 3)
        {
            stateTimer -= Time.deltaTime * balanceFactor;
            if (stateTimer <= 0)
            {
                if (Random.Range(0, 10) < aiLevel + missCounter)
                {
                    missCounter = 0;
                    stateCounter++;
                    switch (stateCounter)
                    {
                        case 1:
                            ShuffleMoles();
                            break;
                        case 3:
                            Attack();
                            break;
                    }
                }
                else
                {
                    stateTimer = moveCooldown;
                    if (aiLevel > 0) { missCounter++; }
                }
            }
        }
        else
        {
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0) { manager.ExamFailed("..."); }
        }
    }
    void ShuffleMoles()
    {
        List<int> numBag = new List<int>() { 0, 1, 2 };
        for (int i = 0; i < 3; i++)
        {
            int n = Random.Range(0, numBag.Count);
            moles[i].Show(numBag[n]);
            numBag.RemoveAt(n);
        }
        moleCombo = 0;
    }
    public void OnMoleClicked(int _molePos)
    {
        Mole mole = moles[_molePos];
        if (mole.signNumber == moleCombo)
        {
            mole.Clicked();
            moleCombo++;
            if (moleCombo == 3)
            {
                manager.TriggerRoomOverlay();
                stateCounter = 0;
                r.sprite = sprites[0];
                foreach (Mole m in moles) { m.Hide(); }
            }
        }
        else
        {
            foreach (Mole m in moles) { m.Hide(); }
            moleTimer = Random.Range(0.5f, 4.5f);
        }
    }
    void Attack()
    {
        manager.LockEnemies(this);
        manager.LockCamera(180);
        manager.LockPlayer();
        stateTimer = 1.5f;
    }
}