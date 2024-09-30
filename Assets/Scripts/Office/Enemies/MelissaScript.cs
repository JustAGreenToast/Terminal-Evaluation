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
    SpriteRenderer _r;
    SpriteRenderer r
    {
        get
        {
            if (!_r) { _r = GetComponent<SpriteRenderer>(); }
            return _r;
        }
    }
    int meruAnimCounter;
    float meruAnimTimer;
    BoxCollider clickTrigger;
    SpriteWobbleScript wobbleAnim;
    AudioClip squeakSound;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Melissa; }
    protected override void OnStart()
    {
        squeakSound = Resources.Load<AudioClip>("SFX/squeak");
        r.enabled = false;
        clickTrigger = GetComponent<BoxCollider>();
        clickTrigger.enabled = false;
        wobbleAnim = GetComponent<SpriteWobbleScript>();
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        if (manager.isPlayerTurnedAround && currentState != States.Waiting) { Dispawn(); }
        if (dispawnWindow > 0) { dispawnWindow -= Time.deltaTime; }
        if (sprites.Length > 4)
        {
            meruAnimTimer += Time.deltaTime;
            if (meruAnimTimer >= 0.125f)
            {
                meruAnimCounter++;
                meruAnimCounter %= 4;
                UpdateSprite();
                meruAnimTimer -= 0.125f;
            }
        }
        switch (currentState)
        {
            case States.Waiting:
                if (isMonitorUp)
                {
                    stateTimer -= Time.deltaTime * balanceFactor;
                    if (stateTimer <= 0)
                    {
                        if (Random.Range(0, 10) < aiLevel && AbleToSpawn()) { Spawn(); }
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
                        UpdateSprite();
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
                    manager.ExamFailed($"{(sprites.Length > 4 ? "Meru" : "Melissa")} will occasionally show up behind your monitor, pull your monitor down and look at her until she leaves. If you take too long or click on her, you lose.");
                break;
        }
    }
    public override bool IsAvaliableForCombo(EnemyTypes _other)
    {
        if (aiLevel == 0 || isLocked || currentState != States.Waiting || !AbleToSpawn()) { return false; }
        switch (_other)
        {
            case EnemyTypes.Chelsea: return Random.value < 0.5f;
            case EnemyTypes.Midnight: return Random.value < 0.1f;
            default: return false;
        }
    }
    public override void ComboTriggered(EnemyTypes _other)
    {
        switch (_other)
        {
            case EnemyTypes.Chelsea:
            case EnemyTypes.Midnight:
                Spawn();
                break;
        }
    }
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
                UpdateSprite();
                break;
        }
    }
    bool AbleToSpawn()
    {
        if (!manager.IsLocationAvailable(Locations.Monitor)) { return false; }
        if (manager.isPlayerTurnedAround) { return false; }
        if (manager.isMidnightKnocking) { return false; }
        return true;
    }
    void Spawn()
    {
        currentState = States.BehindMonitor;
        stateTimer = patienceTime;
        currentLocation = Locations.Monitor;
        manager.TriggerRoomOverlay();
        UpdateSprite();
        r.enabled = true;
        clickTrigger.enabled = true;
        dispawnWindow = gracePeriod;
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
        manager.CloseMonitor();
        manager.LockPlayer();
        manager.LockEnemies(this);
        manager.FadeOutMusic();
        UpdateSprite();
        if (!_timeout)
        {
            manager.PlaySound(squeakSound, "Squeak! :3", true, true);
            wobbleAnim.PlayAnim();
        }
    }
    public override void OnTexturePackChanged(string _folderName)
    {
        if (_folderName == "10" || _folderName == "11")
        {
            sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Meru");
            transform.localPosition = new Vector3(0, -0.925f, 1.25f);
        }
        else
        {
            sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Melissa");
            transform.localPosition = new Vector3(0, -1.125f, 1.25f);
            meruAnimCounter = 0;
            meruAnimTimer = 0;
        }
        UpdateSprite();
    }
    void UpdateSprite()
    {
        switch (currentState)
        {
            case States.Waiting:
            case States.BehindMonitor:
                r.sprite = sprites[sprites.Length > 4 ? meruAnimCounter : 1];
                break;
            case States.Staredown:
                r.sprite = sprites[sprites.Length > 4 ? 4 + meruAnimCounter : 2];
                break;
            case States.Attack:
                r.sprite = sprites[sprites.Length > 4 ? 8 + meruAnimCounter : 3];
                break;
        }
    }
}