using UnityEngine;

public class MelissaScript : EnemyScript
{
    enum States { Waiting, BehindMonitor, Staredown, Attack, Headpatted };
    States currentState;
    float stateTimer;
    float dispawnWindow;
    const float gracePeriod = 0.75f;
    const float patienceTime = 10;
    const float staredownTime = 1.5f;
    float moveCooldown { get { return Random.value < 0.25f ? Random.Range(2.5f, 5f) : Random.Range(10f, 15f); } }
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
    enum Appearances { Melissa, Meru, Lena, Rena };
    Appearances appearance;
    int animCounter;
    float animTimer;
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
        UpdateAnim();
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
            case States.Headpatted:
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                    manager.ExamFailed($"{new string[4] { "Melissa", "Meru", "Lena", "Rena" }[(int)appearance]} will occasionally show up behind your monitor, pull your monitor down and look at her until she leaves. If you take too long or click on her, you lose.");
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
        currentState = _timeout ? States.Attack : States.Headpatted;
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
        switch (_folderName)
        {
            case "1":
            case "2":
            case "3":
            case "4":
                appearance = Random.value < 0.25f ? Random.value < 0.5f ? Appearances.Rena : Appearances.Lena : Appearances.Melissa;
                break;
            case "10":
            case "11":
                appearance = Appearances.Meru;
                break;
            case "12":
                appearance = Random.value < 0.1f ? Random.value < 0.5f ? Appearances.Rena : Appearances.Lena : Appearances.Melissa;
                break;
            default:
                appearance = Appearances.Melissa;
                break;
        }
        if (System.DateTime.Now.Month == 12 && appearance == Appearances.Melissa && _folderName != "13" && Random.value < 0.4f) { appearance = Random.value < 0.5f ? Appearances.Rena : Appearances.Lena; }
        float height = -1.125f;
        float scale = 1.65f;
        switch (appearance)
        {
            case Appearances.Melissa:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Melissa");
                break;
            case Appearances.Meru:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Meru");
                height = -0.925f;
                break;
            case Appearances.Lena:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Lena");
                scale = 2;
                break;
            case Appearances.Rena:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Rena");
                scale = 2;
                break;
        }
        transform.localPosition = new Vector3(0, height, 1.25f);
        transform.localScale = Vector3.one * scale;
        animCounter = 0;
        animTimer = 0;
        UpdateSprite();
    }
    void UpdateAnim()
    {
        if (appearance == Appearances.Meru)
        {
            animTimer += Time.deltaTime;
            if (animTimer >= 0.125f)
            {
                animCounter++;
                animCounter %= 4;
                UpdateSprite();
                animTimer -= 0.125f;
            }
        }
    }
    void UpdateSprite()
    {
        switch (appearance)
        {
            case Appearances.Melissa:
                r.sprite = sprites[Mathf.Clamp((int)currentState, 1, 3)];
                break;
            case Appearances.Meru:
                r.sprite = sprites[Mathf.Clamp((int)currentState - 1, 0, 2) * 4 + animCounter];
                break;
            case Appearances.Lena:
            case Appearances.Rena:
                r.sprite = sprites[Mathf.Clamp((int)currentState - 1, 0, 2)];
                if (appearance == Appearances.Rena && currentState == States.Attack) { r.sprite = sprites[1]; }
                break;
        }
    }
}