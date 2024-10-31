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
    float moveCooldown { get { return Random.value < 0.25f ? Random.Range(8f, 12f) : Random.Range(16f, 24f); } }
    int missCounter;
    int moleCombo;
    float moleTimer;
    Sprite[] sprites;
    SpriteRenderer r;
    AudioClip digUpSound;
    AudioClip digDownSound;
    AudioClip squeakSound;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Tournament_Molly; }
    protected override void OnStart()
    {
        digUpSound = Resources.Load<AudioClip>("SFX/dig");
        digDownSound = Resources.Load<AudioClip>("SFX/dig_2");
        squeakSound = Resources.Load<AudioClip>("SFX/squeak");
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
//#if UNITY_EDITOR
//        if (Input.GetKeyDown(KeyCode.N)) { ShuffleMoles(); }
//        if (Input.GetKeyDown(KeyCode.M)) { Attack(); }
//#endif
        foreach (Mole mole in moles) { mole.Update(); }
        if (stateCounter < 3)
        {
            stateTimer -= Time.deltaTime * balanceFactor;
            if (stateTimer <= 0)
            {
                if (Random.Range(0, 10) < aiLevel + missCounter && (stateCounter > 0 || manager.IsLocationAvailable(Locations.Behind)))
                {
                    missCounter = 0;
                    stateCounter++;
                    stateTimer = moveCooldown;
                    r.sprite = sprites[stateCounter];
                    manager.TriggerRoomOverlay();
                    switch (stateCounter)
                    {
                        case 1:
                            currentLocation = Locations.Behind;
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
            if (stateTimer <= 0) { manager.ExamFailed("Molly will slowly crawl her way up her hole, fully coming out after 3 movements. Click on the moles behind her in the correct order to send her back down."); }
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
        manager.PlaySound(digUpSound, "Dig Up", true, true);
    }
    public void OnMoleClicked(int _molePos)
    {
        Mole mole = moles[_molePos];
        if (mole.signNumber == moleCombo)
        {
            mole.Clicked();
            manager.PlaySound(squeakSound, $"Squeak Combo [x{moleCombo + 1}]", true, true, 1 + 0.1f * moleCombo);
            moleCombo++;
            if (moleCombo == 3)
            {
                manager.TriggerRoomOverlay();
                stateCounter = 0;
                currentLocation = Locations.None;
                r.sprite = sprites[0];
                foreach (Mole m in moles) { m.Hide(); }
            }
        }
        else
        {
            foreach (Mole m in moles) { m.Hide(); }
            moleTimer = Random.Range(0.5f, 4.5f);
            manager.PlaySound(digDownSound, "Dig Down", true, true);
        }
    }
    void Attack()
    {
        foreach (Mole m in moles) { m.Hide(); }
        manager.LockEnemies(this);
        manager.LockPlayer();
        manager.LockCamera(180);
        manager.FadeOutMusic();
        stateTimer = 1.5f;
    }
}