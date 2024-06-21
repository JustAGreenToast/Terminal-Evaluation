using UnityEngine;

public class PrincessLightdressScript : EnemyScript
{
    enum States { Asleep, WakingUp, Awake, Headpatted, Attack };
    States currentState;
    float stateTimer;
    const float patienceTime = 10;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(7.5f, 12.5f) : Random.Range(15f, 25f); } }
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
    int coralAnimCounter;
    float coralAnimTimer;
    BoxCollider clickTrigger;
    SpriteWobbleScript wobbleAnim;
    AudioClip squeakSound;
    bool tetra;
    bool alt;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.LightDress; }
    protected override void OnStart()
    {
        if (aiLevel == 0) { gameObject.SetActive(false); }
        squeakSound = Resources.Load<AudioClip>("SFX/squeak");
        clickTrigger = GetComponent<BoxCollider>();
        clickTrigger.enabled = tetra && alt;
        wobbleAnim = GetComponent<SpriteWobbleScript>();
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        if (sprites.Length > 4)
        {
            coralAnimTimer += Time.deltaTime;
            if (coralAnimTimer >= 0.125f)
            {
                coralAnimCounter++;
                coralAnimCounter %= 4;
                UpdateSprite();
                coralAnimTimer -= 0.125f;
            }
        }
        switch (currentState)
        {
            case States.Asleep:
                if (stateTimer > 0)
                {
                    stateTimer -= Time.deltaTime * balanceFactor;
                    if (stateTimer <= 0 && Random.Range(0, 10) >= aiLevel) { stateTimer = moveCooldown; }
                }
                break;
            case States.WakingUp:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0)
                {
                    currentState = States.Awake;
                    stateTimer = Random.Range(2.5f, 7.5f);
                    manager.TriggerRoomOverlay();
                    UpdateSprite();
                }
                break;
            case States.Awake:
                stateTimer -= Time.deltaTime * balanceFactor;
                if (stateTimer <= 0) { Attack(); }
                break;
            case States.Headpatted:
                if (transform.position.x == 0)
                {
                    stateTimer += Time.deltaTime;
                    if (stateTimer >= 0.75f)
                    {
                        manager.TriggerRoomOverlay();
                        transform.position = new Vector3(2.25f, 0, 3.4f);
                        r.color = new Color(0.6f, 0.6f, 0.6f);
                    }
                }
                break;
            case States.Attack:
                // Force Monitor To Be Closed (Player Not Locked)
                if (tetra && !alt) { manager.CloseMonitor(); }
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                    manager.ExamFailed("Princess Lightdress will be sleeping next to the door, click on her if she wakes up. If she stays awake for too long, you lose.");
                break;
        }
    }
    protected override void OnMonitorFlipped()
    {
        switch (currentState)
        {
            case States.Asleep:
                if (isMonitorUp && stateTimer <= 0)
                {
                    currentState = States.WakingUp;
                    stateTimer = patienceTime;
                    manager.TriggerRoomOverlay();
                    UpdateSprite();
                    clickTrigger.enabled = true;
                }
                break;
            case States.Headpatted:
                currentState = States.Asleep;
                stateTimer = moveCooldown;
                manager.TriggerRoomOverlay();
                transform.position = new Vector3(2.25f, 0, 3.4f);
                UpdateSprite();
                r.color = new Color(0.6f, 0.6f, 0.6f);
                break;
        }
    }
    public void OnHeadpat()
    {
        if (currentState != States.Awake && tetra && alt)
        {
            Attack();
            manager.PlaySound(squeakSound);
            wobbleAnim.PlayAnim();
        }
        else if (currentState != States.Asleep)
        {
            if (currentState == States.Attack)
            {
                manager.ResumeMusic();
                manager.UnlockEnemies();
            }
            currentState = States.Headpatted;
            stateTimer = 0;
            UpdateSprite();
            clickTrigger.enabled = tetra && alt;
            manager.PlaySound(squeakSound);
            wobbleAnim.PlayAnim();
        }
    }
    void Attack()
    {
        currentState = States.Attack;
        stateTimer = 1.5f;
        manager.TriggerRoomOverlay();
        transform.position = Vector3.forward * 2;
        UpdateSprite();
        r.color = Color.white;
        manager.CloseMonitor();
        if (!tetra || alt) { manager.LockPlayer(); }
        manager.LockEnemies(this);
        manager.FadeOutMusic();
        clickTrigger.enabled = tetra && !alt;
    }
    protected override void OnEnemyLocked()
    {
        if (transform.position.x == 0)
        {
            manager.TriggerRoomOverlay();
            transform.position = new Vector3(2.25f, 0, 3.4f);
            r.color = new Color(0.6f, 0.6f, 0.6f);
        }
    }
    public override void OnTexturePackChanged(string _folderName)
    {
        if (_folderName == "10" || _folderName == "11")
        {
            tetra = false;
            alt = false;
            sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Coral");
        }
        else
        {
            tetra = (SettingsManager.settings.selectedConsoleTheme == SettingsManager.Settings.ConsoleThemes.Tetris || SettingsManager.settings.tetrisCartridge) && Random.value < 0.25f;
            alt = Random.value > 0.5f;
            sprites = Resources.LoadAll<Sprite>($"Sprites/Characters/Princess {(tetra ? "Tetra" : "LightDress")}{(alt ? " Alt" : "")}");
        }
        UpdateSprite();
    }
    void UpdateSprite()
    {
        switch (currentState)
        {
            case States.Asleep:
                r.sprite = sprites[sprites.Length > 5 ? coralAnimCounter : 0];
                break;
            case States.WakingUp:
                r.sprite = sprites[sprites.Length > 5 ? 4 + coralAnimCounter : 1];
                break;
            case States.Awake:
                r.sprite = sprites[sprites.Length > 5 ? 8 + coralAnimCounter : 2];
                break;
            case States.Headpatted:
                r.sprite = sprites[sprites.Length > 5 ? 12 + coralAnimCounter : 3];
                break;
            case States.Attack:
                r.sprite = sprites[sprites.Length > 5 ? 16 + coralAnimCounter : sprites.Length > 4 ? 4 : 2];
                break;
        }
    }
}