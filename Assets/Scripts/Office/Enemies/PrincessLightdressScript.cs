using UnityEngine;

public class PrincessLightdressScript : EnemyScript
{
    enum States { Asleep, WakingUp, Awake, Headpatted, Attack };
    States currentState;
    float stateTimer;
    const float patienceTime = 10;
    float moveCooldown { get { return Random.value > 0.9f ? Random.Range(7.5f, 12.5f) : Random.Range(15f, 25f); } }
    Sprite[] sprites;
    SpriteRenderer r;
    BoxCollider clickTrigger;
    SpriteWobbleScript wobbleAnim;
    AudioClip squeakSound;
    bool tetra;
    bool alt;
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.LightDress; }
    protected override void OnStart()
    {
        tetra = (SettingsManager.settings.selectedConsoleTheme == SettingsManager.Settings.ConsoleThemes.Tetris || SettingsManager.settings.tetrisCartridge) && Random.value < 0.25f;
        alt = Random.value > 0.5f;
        sprites = Resources.LoadAll<Sprite>($"Sprites/Characters/Princess {(tetra ? "Tetra" : "LightDress")}{(alt ? " Alt" : "")}");
        squeakSound = Resources.Load<AudioClip>("SFX/squeak");
        r = GetComponent<SpriteRenderer>();
        if (aiLevel == 0) { gameObject.SetActive(false); }
        else { r.sprite = sprites[0]; }
        clickTrigger = GetComponent<BoxCollider>();
        clickTrigger.enabled = tetra && alt;
        wobbleAnim = GetComponent<SpriteWobbleScript>();
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
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
                    r.sprite = sprites[2];
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
                    r.sprite = sprites[1];
                    clickTrigger.enabled = true;
                }
                break;
            case States.Headpatted:
                currentState = States.Asleep;
                stateTimer = moveCooldown;
                manager.TriggerRoomOverlay();
                transform.position = new Vector3(2.25f, 0, 3.4f);
                r.sprite = sprites[0];
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
            r.sprite = sprites[3];
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
        if (sprites.Length > 4) { r.sprite = sprites[4]; }
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
}