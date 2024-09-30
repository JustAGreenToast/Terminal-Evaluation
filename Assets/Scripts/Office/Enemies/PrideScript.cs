using System.Collections.Generic;
using UnityEngine;

public class PrideScript : EnemyScript
{
    class Lightswitch
    {
        public bool isOn { get; private set; }
        const float smearFrameTime = 0.03f;
        float animTimer;
        Sprite[] sprites;
        SpriteRenderer r;
        BoxCollider cursorTrigger;
        public Lightswitch(GameObject _switchObj)
        {
            sprites = Resources.LoadAll<Sprite>("Sprites/Lightswitch");
            r = _switchObj.GetComponent<SpriteRenderer>();
            cursorTrigger = _switchObj.GetComponent<BoxCollider>();
            Hide();
        }
        public void Update()
        {
            if (animTimer > 0)
            {
                animTimer -= Time.deltaTime;
                if (animTimer <= 0) { r.sprite = sprites[isOn ? 2 : 0]; }
            }
        }
        public void Enable(bool _isOn)
        {
            isOn = _isOn;
            animTimer = 0;
            r.sprite = sprites[isOn ? 2 : 0];
            r.enabled = true;
            cursorTrigger.enabled = true;
        }
        public void Toggle()
        {
            isOn = !isOn;
            animTimer = smearFrameTime;
            r.sprite = sprites[1];
        }
        public void Disable() { cursorTrigger.enabled = true; }
        public void Hide()
        {
            Disable();
            r.enabled = false;
        }
    }
    Lightswitch[] lightswitches = new Lightswitch[4];
    enum States { Hidden, Waiting, Distracted };
    States currentState;
    float stateTimer;
    float moveCooldown { get { return Random.value > 0.75f ? Random.Range(7.5f, 12.5f) : Random.Range(16f, 24f); } }
    int missCounter;
    Transform pride;
    Sprite[] sprites;
    SpriteRenderer r;
    int animCounter;
    float animTimer;
    Sprite[] lampSprites;
    SpriteRenderer lamp;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Tournament_Pride; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Pride");
        pride = transform.GetChild(0);
        r = pride.GetComponent<SpriteRenderer>();
        r.enabled = false;
        stateTimer = moveCooldown;
        //IncreaseAI(10);
        for (int i = 0; i < 4; i++) { lightswitches[i] = new Lightswitch(transform.GetChild(i + 1).gameObject); }
        lampSprites = Resources.LoadAll<Sprite>("Sprites/Lamp");
        lamp = transform.GetChild(5).GetComponent<SpriteRenderer>();
        lamp.enabled = false;
    }
    protected override void OnUpdate()
    {
        //if (Input.GetKeyDown(KeyCode.P))
        //{
        //    missCounter = 10;
        //    stateTimer = 0;
        //}
        foreach (Lightswitch lightswitch in lightswitches) { lightswitch.Update(); }
        switch (currentState)
        {
            case States.Hidden:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (Random.Range(0, 10) < aiLevel + missCounter && !manager.isMidnightKnocking && manager.IsLocationAvailable(Locations.Behind))
                    {
                        missCounter = 0;
                        currentState = States.Waiting;
                        currentLocation = Locations.Behind;
                        manager.TriggerRoomOverlay();
                        manager.CloseMonitor();
                        manager.LockCamera(180);
                        manager.canPlayerTurnAround = false;
                        pride.localPosition = new Vector3(0, -2, -2.75f);
                        r.sprite = sprites[0];
                        r.enabled = true;
                        lamp.sprite = lampSprites[0];
                        lamp.enabled = true;
                        ShuffleSwitches();
                    }
                    else
                    {
                        stateTimer = moveCooldown;
                        if (aiLevel > 0) { missCounter++; }
                    }
                }
                break;
            case States.Distracted:
                stateTimer += Time.deltaTime;
                if (stateTimer >= 3)
                {
                    currentState = States.Hidden;
                    stateTimer = moveCooldown;
                    currentLocation = Locations.None;
                    manager.TriggerRoomOverlay();
                    r.enabled = false;
                    lamp.enabled = false;
                    foreach (Lightswitch lightswitch in lightswitches) { lightswitch.Hide(); }
                }
                else
                {
                    pride.localPosition = new Vector3(0, -1.25f + 0.05f * Mathf.Sin(stateTimer * 420 * Mathf.Deg2Rad), -0.25f);
                    animTimer += Time.deltaTime;
                    if (animTimer >= 0.05f)
                    {
                        animCounter++;
                        animCounter %= 4;
                        r.sprite = sprites[animCounter < 3 ? animCounter + 1 : 2];
                        animTimer -= 0.05f;
                    }
                }
                break;
        }
    }
    void ShuffleSwitches()
    {
        List<int> numBag = new List<int>() { 0, 1, 2, 3 };
        List<int> onSwitches = new List<int>();
        for (int i = Random.Range(0, 3); i >= 0; i--)
        {
            int n = Random.Range(0, numBag.Count);
            onSwitches.Add(numBag[n]);
            numBag.RemoveAt(n);
        }
        for (int i = 0; i < 4; i++) { lightswitches[i].Enable(onSwitches.Contains(i)); }
    }
    public void OnLightswitchClicked(int _i)
    {
        lightswitches[_i].Toggle();
        if (AllSwitchesOn())
        {
            currentState = States.Distracted;
            stateTimer = 0;
            manager.canPlayerTurnAround = true;
            manager.UnlockCamera();
            manager.TriggerRoomOverlay();
            r.sprite = sprites[1];
            foreach (Lightswitch lightswitch in lightswitches) { lightswitch.Disable(); }
            lamp.sprite = lampSprites[1];
        }
    }
    bool AllSwitchesOn()
    {
        foreach (Lightswitch lightswitch in lightswitches) { if (!lightswitch.isOn) { return false; } }
        return true;
    }
}