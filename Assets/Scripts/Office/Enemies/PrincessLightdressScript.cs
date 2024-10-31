using UnityEngine;

public class PrincessLightdressScript : EnemyScript
{
    enum States { Asleep, WakingUp, Awake, Headpatted, Attack };
    States currentState;
    float stateTimer;
    const float patienceTime = 10;
    float moveCooldown { get { return Random.value < 0.1f ? Random.Range(7.5f, 12.5f) : Random.Range(16f, 24f); } }
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
    enum Appearances { Lightdress, Lighterdress, Tetra, TetraAlt, Coral, Lighterdress_Witch, LalaPuppet, LuluPuppet };
    Appearances appearance;
    string failMessage
    {
        get
        {
            switch (appearance)
            {
                case Appearances.Lightdress: return "Princess Lightdress will be sleeping next to the door, click on her if she wakes up. If she stays awake for too long, you lose.";
                case Appearances.Lighterdress: return "Princess Lighterdress will be sleeping next to the door, click on her if she wakes up. If she stays awake for too long, you lose.";
                case Appearances.Tetra: return "Princess Tetra will be sleeping next to the door, click on her if she wakes up or if she gets too close. If she stays awake for too long, you lose.";
                case Appearances.TetraAlt: return "Queen Tetra will be sleeping next to the door, click on her once she FULLY wakes up. If she stays awake for too long or you click on her before she FULLY wakes up, you lose.";
                case Appearances.Coral: return "Coral will be sleeping next to the door, click on her if she wakes up. If she stays awake for too long, you lose.";
                case Appearances.Lighterdress_Witch:
                case Appearances.LalaPuppet:
                case Appearances.LuluPuppet:
                    return "Happy Halloween!";
                default: throw new System.Exception($"oi dumbass you forgor to give Appearances.{appearance} a fail msg bozo");
            }
        }
    }
    protected override EnemyTypes GetEnemyType() { return EnemyTypes.Lightdress; }
    protected override void OnStart()
    {
        if (aiLevel == 0) { gameObject.SetActive(false); }
        squeakSound = Resources.Load<AudioClip>("SFX/squeak");
        clickTrigger = GetComponent<BoxCollider>();
        clickTrigger.enabled = appearance == Appearances.TetraAlt;
        wobbleAnim = GetComponent<SpriteWobbleScript>();
        stateTimer = moveCooldown;
    }
    protected override void OnUpdate()
    {
        if (appearance == Appearances.Coral)
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
                // Force Monitor To Be Closed If Player Not Locked
                if (appearance == Appearances.Tetra)
                {
                    manager.CloseMonitor();
                    manager.LockCamera();
                }
                stateTimer -= Time.deltaTime;
                if (stateTimer <= 0)
                    manager.ExamFailed(failMessage);
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
        if (currentState != States.Awake && appearance == Appearances.TetraAlt)
        {
            Attack();
            manager.PlaySound(squeakSound, "Squeak! :3", true, true);
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
            clickTrigger.enabled = appearance == Appearances.TetraAlt;
            manager.PlaySound(squeakSound, "Squeak! :3", true, true);
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
        if (appearance != Appearances.Tetra) { manager.LockPlayer(); }
        manager.LockCamera();
        manager.LockEnemies(this);
        manager.FadeOutMusic();
        clickTrigger.enabled = appearance == Appearances.Tetra;
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
        switch (_folderName)
        {
            case "10": // Sandy Shores
            case "11": // Pool Party
                appearance = Appearances.Coral;
                break;
            case "13": // Mischief Mansion
                appearance = new Appearances[3] { Appearances.Lighterdress_Witch, Appearances.LalaPuppet, Appearances.LuluPuppet }[Random.Range(0, 3)];
                break;
            default:
                if (SettingsManager.settings.selectedConsoleTheme == SettingsManager.Settings.ConsoleThemes.Tetris || SettingsManager.settings.tetrisCartridge) { appearance = Random.value < 0.5f ? Appearances.Tetra : Appearances.TetraAlt; }
                else { appearance = Random.value < 0.5f ? Appearances.Lightdress : Appearances.Lighterdress; }
                break;
        }
        LoadSprites();
        UpdateSprite();
    }
    void LoadSprites()
    {
        switch (appearance)
        {
            case Appearances.Lightdress:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Princess LightDress");
                break;
            case Appearances.Lighterdress:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Princess LightDress Alt");
                break;
            case Appearances.Tetra:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Princess Tetra");
                break;
            case Appearances.TetraAlt:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Princess Tetra Alt");
                break;
            case Appearances.Coral:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Coral");
                break;
            case Appearances.Lighterdress_Witch:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Princess Lighterdress Halloween");
                break;
            case Appearances.LalaPuppet:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Lala Puppet");
                break;
            case Appearances.LuluPuppet:
                sprites = Resources.LoadAll<Sprite>("Sprites/Characters/Lulu Puppet");
                break;
        }
    }
    void UpdateSprite()
    {
        switch (currentState)
        {
            case States.Asleep:
                r.sprite = sprites[appearance == Appearances.Coral ? coralAnimCounter : 0];
                break;
            case States.WakingUp:
                r.sprite = sprites[appearance == Appearances.Coral ? 4 + coralAnimCounter : 1];
                break;
            case States.Awake:
                r.sprite = sprites[appearance == Appearances.Coral ? 8 + coralAnimCounter : 2];
                break;
            case States.Headpatted:
                r.sprite = sprites[appearance == Appearances.Coral ? 12 + coralAnimCounter : 3];
                break;
            case States.Attack:
                r.sprite = sprites[appearance == Appearances.Coral ? 16 + coralAnimCounter : sprites.Length > 4 ? 4 : 2];
                break;
        }
    }
}