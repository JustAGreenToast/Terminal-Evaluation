using UnityEngine;

public class MelissaScript : EnemyScript
{
    enum States { Waiting, BehindMonitor, Staredown, Attack };
    States currentState;
    float stateTimer;
    float dispawnWindow;
    const float gracePeriod = 0.75f;
    const float patienceTime = 10;
    const float staredownTime = 1.5f;
    float moveCooldown { get { return Random.value > 0.75f ? Random.Range(2.5f, 5f) : Random.Range(10f, 15f); } }
    Sprite[] sprites;
    SpriteRenderer r;
    BoxCollider clickTrigger;
    SpriteWobbleScript wobbleAnim;
    AudioClip squeakSound;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Melissa; }
    protected override void OnStart()
    {
        sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Melissa");
        squeakSound = Resources.Load<AudioClip>("SFX/squeak");
        r = GetComponent<SpriteRenderer>();
        r.enabled = false;
        clickTrigger = GetComponent<BoxCollider>();
        clickTrigger.enabled = false;
        wobbleAnim = GetComponent<SpriteWobbleScript>();
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        if (dispawnWindow > 0) { dispawnWindow -= Time.deltaTime; } 
        switch (currentState)
        {
            case States.Waiting:
                if (isMonitorUp)
                {
                    stateTimer -= Time.deltaTime * balanceFactor;
                    if (stateTimer <= 0)
                    {
                        if (Random.Range(0, 10) < aiLevel && manager.IsLocationAvailable(Locations.Monitor))
                        {
                            currentState = States.BehindMonitor;
                            stateTimer = patienceTime;
                            currentLocation = Locations.Monitor;
                            manager.TriggerRoomOverlay();
                            r.sprite = sprites[1];
                            r.enabled = true;
                            clickTrigger.enabled = true;
                            dispawnWindow = gracePeriod;
                        }
                        else { stateTimer = moveCooldown; }
                    }
                }
                break;
            case States.BehindMonitor:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    if (isMonitorUp) { Attack(true); }
                    else
                    {
                        currentState = States.Staredown;
                        stateTimer = staredownTime;
                        manager.TriggerRoomOverlay();
                        r.sprite = sprites[2];
                    }
                }
                break;
            case States.Staredown:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0) { Dispawn(); }
                break;
            case States.Attack:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                    manager.ExamFailed("Melissa will occasionally show up behind your monitor, pull your monitor down and look at her until she leaves. If you take too long or click on her, you lose.");
                break;
        }
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other)
    {
        if (aiLevel == 0 || currentState != States.Waiting || !manager.IsLocationAvailable(Locations.Door)) { return false; }
        switch (_other)
        {
            case EnemyTypes.Chelsea: return Random.value > 0.5f;
            case EnemyTypes.Midnight: return Random.value > 0.9f;
            default: return false;
        }
    }
    public override void ComboTriggered(EnemyTypes _other) { stateTimer = 0; }
    protected override void OnEnemyLocked()
    {
        if (currentState != States.Waiting)
        {
            currentState = States.Waiting;
            stateTimer = moveCooldown;
            manager.TriggerRoomOverlay();
            r.enabled = false;
        }
    }
    protected override void OnMonitorFlipped()
    {
        switch (currentState)
        {
            case States.Waiting:
                if (!isMonitorUp) { stateTimer = moveCooldown; }
                break;
            case States.BehindMonitor:
                stateTimer = isMonitorUp ? patienceTime : 0.75f;
                if (dispawnWindow > 0 && !isMonitorUp) { Dispawn(); }
                break;
            case States.Staredown:
                currentState = States.BehindMonitor;
                stateTimer = patienceTime;
                manager.TriggerRoomOverlay();
                r.sprite = sprites[1];
                break;
        }
    }
    void Dispawn()
    {
        currentState = States.Waiting;
        stateTimer = moveCooldown;
        currentLocation = Locations.None;
        manager.TriggerRoomOverlay();
        r.enabled = false;
        clickTrigger.enabled = false;
    }
    public void Attack(bool _timeout = false)
    {
        if (dispawnWindow > 0) { return; }
        currentState = States.Attack;
        stateTimer = 1.5f;
        //manager.TriggerRoomOverlay();
        manager.CloseMonitor();
        manager.LockPlayer();
        manager.LockEnemies(this);
        manager.FadeOutMusic();
        r.sprite = sprites[3];
        if (!_timeout)
        {
            manager.PlaySound(squeakSound);
            wobbleAnim.PlayAnim();
        }
    }
}